using FiapSrvGames.Infrastructure.Configuration;
using Microsoft.AspNetCore.DataProtection;
using MongoDB.Driver;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using FiapSrvGames.Infrastructure.Mappings;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Infrastructure.Repository;
using Microsoft.OpenApi.Models;
using FiapSrvGames.Infrastructure.Middleware;


[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.ConfigureSerilog();
builder.Host.UseSerilog();

string mongoConnectionString;
string databaseName = builder.Configuration.GetSection("MongoDbSettings:DatabaseName").Value ?? "";
string jwtSigningKey;

if (!builder.Environment.IsDevelopment())
{

    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    var secretName = builder.Configuration["KeyVault:DatabaseSecretName"];
    var blobUrl = builder.Configuration["Blob:Url"];
    var keyName = builder.Configuration["KeyVault:BlobKeyName"];
    var jwtSecretName = builder.Configuration["KeyVault:JwtSigningKeyName"];

    var credential = new DefaultAzureCredential();
    var managedCredential = new ManagedIdentityCredential();

    var client = new SecretClient(
        new Uri(keyVaultUrl),
        credential
    );

    builder.Services.AddDataProtection()
        .SetApplicationName("FiapSrvGames")
        .PersistKeysToAzureBlobStorage(new Uri(blobUrl), managedCredential)
        .ProtectKeysWithAzureKeyVault(new Uri(keyVaultUrl + "keys/" + keyName), managedCredential);


    KeyVaultSecret jwtKey = await client.GetSecretAsync(jwtSecretName);
    jwtSigningKey = jwtKey.Value;

    KeyVaultSecret mongoConnectionSecret = await client.GetSecretAsync(secretName);
    mongoConnectionString = mongoConnectionSecret.Value;

}
else
{
    Log.Information("Ambiente de Desenvolvimento/Local detectado. Obtendo string de conexão do appsettings. ??");
    mongoConnectionString = builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value ?? "";
    jwtSigningKey = builder.Configuration.GetSection("Jwt:DevKey").Value ?? "";
}

builder.Services.AddSingleton<IMongoClient>(sp =>
        new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

MongoMappings.ConfigureMappings();

// Configuração do JWT


builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Cloud Games - Games API", Version = "v1" });

    /*
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
   {
       {
           new OpenApiSecurityScheme
           {
               Reference = new OpenApiReference
               {
                   Type = ReferenceType.SecurityScheme,
                   Id = "Bearer"
               }
           },
           Array.Empty<string>()
       }
   });
    */

    //opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "FiapSrvGames.API.xml"));
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
