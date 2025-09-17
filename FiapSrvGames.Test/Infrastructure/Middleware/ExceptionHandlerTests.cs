using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Xunit;
using MongoDB.Driver.Core.Connections;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;
using System.Reflection;

public class ExceptionHandlerTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<ExceptionHandler>>();
        var wasCalled = false;

        RequestDelegate next = ctx =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandler(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.True(wasCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenHttpException_ReturnsCustomStatusAndMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new System.IO.MemoryStream();
        var loggerMock = new Mock<ILogger<ExceptionHandler>>();

        // HttpException já é pública e recebe status, mensagem e erro
        var httpEx = new HttpException(404, "Não encontrado", "Não encontrado");

        RequestDelegate next = ctx => throw httpEx;
        var middleware = new ExceptionHandler(next, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var response = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();

        var json = JsonDocument.Parse(response);
        Assert.Equal("Não encontrado", json.RootElement.GetProperty("message").GetString());
        Assert.Equal("Não encontrado", json.RootElement.GetProperty("error").GetString());
        Assert.Equal(404, json.RootElement.GetProperty("statusCode").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new System.IO.MemoryStream();
        var loggerMock = new Mock<ILogger<ExceptionHandler>>();
        var ex = new Exception("Erro genérico");

        RequestDelegate next = ctx => throw ex;

        var middleware = new ExceptionHandler(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var response = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();

        // Parse do JSON e validação das propriedades
        var json = JsonDocument.Parse(response);
        Assert.Equal("Erro genérico", json.RootElement.GetProperty("message").GetString());
        Assert.Equal("Internal Server Error", json.RootElement.GetProperty("error").GetString());
        Assert.Equal(500, json.RootElement.GetProperty("statusCode").GetInt32());
    }


}
