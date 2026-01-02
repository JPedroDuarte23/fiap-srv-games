using Amazon.SQS;
using AspNetCore.DataProtection.Aws.S3;
using Elastic.Clients.Elasticsearch;
using FiapCloudGames.Infrastructure.Configuration;
using FiapSrvGames.API.Workers;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Infrastructure.Configuration;
using FiapSrvGames.Infrastructure.Mappings;
using FiapSrvGames.Infrastructure.Middleware;
using FiapSrvGames.Infrastructure.Repository;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Prometheus;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.ConfigureSerilog();
builder.Host.UseSerilog();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();  
builder.Services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();
builder.Services.AddAWSService<IAmazonSQS>();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection")
    ?? throw new InvalidOperationException("Connection string MongoDbConnection not found.");

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] 
    ?? builder.Configuration["Jwt:DevKey"] 
    ?? throw new InvalidOperationException("JWT Signing Key not found.");

var elasticSearchUrl = builder.Configuration["ElasticSearch:Url"]
    ?? throw new InvalidOperationException("ElasticSearch Url not found.");

var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"] 
    ?? throw new InvalidOperationException("Database Name not found.");

if (!builder.Environment.IsDevelopment())
{
    var s3Bucket = builder.Configuration["DataProtection:S3BucketName"];
    var s3KeyPrefix = builder.Configuration["DataProtection:S3KeyPrefix"];
    
    if (!string.IsNullOrEmpty(s3Bucket) && !string.IsNullOrEmpty(s3KeyPrefix))
    {
        var s3DataProtectionConfig = new S3XmlRepositoryConfig(s3Bucket) { KeyPrefix = s3KeyPrefix };
        builder.Services.AddDataProtection()
            .SetApplicationName("FiapSrvGames")
            .PersistKeysToAwsS3(s3DataProtectionConfig);
    }
}

var settings = new ElasticsearchClientSettings(new Uri(elasticSearchUrl));
var client = new ElasticsearchClient(settings);
builder.Services.AddSingleton(client);

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
MongoMappings.ConfigureMappings();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditEventRepository, AuditEventRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddHostedService<LibraryUpdateWorker>();

builder.Services.ConfigureJwtBearer(builder.Configuration, jwtSigningKey);
builder.Services.AddAuthorization();


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
