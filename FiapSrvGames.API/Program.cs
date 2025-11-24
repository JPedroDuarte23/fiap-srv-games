using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using AspNetCore.DataProtection.Aws.S3;
using Elastic.Clients.Elasticsearch;
using FiapCloudGames.Infrastructure.Configuration;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Infrastructure.Configuration;
using FiapSrvGames.Infrastructure.Mappings;
using FiapSrvGames.Infrastructure.Middleware;
using FiapSrvGames.Infrastructure.Repository;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Prometheus;
using Amazon.SQS;

[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.ConfigureSerilog();
builder.Host.UseSerilog();

// 1. Configura��o da AWS
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();  
builder.Services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();

string mongoConnectionString;
string jwtSigningKey;
string elasticSearchUrl;
string databaseName = builder.Configuration["MongoDbSettings:DatabaseName"]!;

if (!builder.Environment.IsDevelopment())
{
    Log.Information("Ambiente de Produ��o. Buscando segredos do AWS Parameter Store.");
    var ssmClient = new AmazonSimpleSystemsManagementClient();

    // Busca a Connection String do MongoDB
    var mongoParameterName = builder.Configuration["ParameterStore:MongoConnectionString"];
    var mongoResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = mongoParameterName,
        WithDecryption = true
    });
    mongoConnectionString = mongoResponse.Parameter.Value;

    // Busca a Chave de Assinatura do JWT
    var jwtParameterName = builder.Configuration["ParameterStore:JwtSigningKey"];
    var jwtResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = jwtParameterName,
        WithDecryption = true
    });
    jwtSigningKey = jwtResponse.Parameter.Value;

    var elasticParameterName = builder.Configuration["ParameterStore:ElasticSearchUrl"];
    var elasticResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = elasticParameterName,
        WithDecryption = true
    });
    elasticSearchUrl = elasticResponse.Parameter.Value;


    // 2. Configura��o do Data Protection com AWS S3
    var s3Bucket = builder.Configuration["DataProtection:S3BucketName"];
    var s3KeyPrefix = builder.Configuration["DataProtection:S3KeyPrefix"];
    var s3DataProtectionConfig = new S3XmlRepositoryConfig(s3Bucket) { KeyPrefix = s3KeyPrefix };

    builder.Services.AddDataProtection()
        .SetApplicationName("FiapSrvGames")
        .PersistKeysToAwsS3(s3DataProtectionConfig);
}
else
{
    Log.Information("Ambiente de Desenvolvimento. Usando appsettings.json.");
    mongoConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection")!;
    jwtSigningKey = builder.Configuration["Jwt:DevKey"]!;
    elasticSearchUrl = builder.Configuration["ElasticSearch:Url"]!;
    
}  

var settings = new ElasticsearchClientSettings(new Uri(elasticSearchUrl));
var client = new ElasticsearchClient(settings);

builder.Services.AddSingleton(client);

// 3. Configura��o do MongoDB e Reposit�rios
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
MongoMappings.ConfigureMappings();

builder.Services.AddAWSService<IAmazonSQS>();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditEventRepository, AuditEventRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// 4. Configura��o de Autentica��o e Autoriza��o
builder.Services.ConfigureJwtBearer(builder.Configuration, jwtSigningKey);
builder.Services.AddAuthorization();

// -- Resto da configura��o (Controllers, Swagger, etc.) --
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Cloud Games - Games API", Version = "v1" });
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
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.UseHttpMetrics();

app.MapMetrics();
app.MapControllers();

app.Run();
