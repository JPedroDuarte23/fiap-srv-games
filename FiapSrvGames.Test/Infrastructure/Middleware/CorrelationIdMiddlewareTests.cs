using System.Threading.Tasks;
using FiapSrvGames.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task ShouldAddCorrelationIdHeader_WhenHeaderIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled);
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task ShouldPreserveExistingCorrelationIdHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var existingCorrelationId = "meu-correlation-id";
        context.Request.Headers["X-Correlation-Id"] = existingCorrelationId;
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled);
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        Assert.Equal(existingCorrelationId, context.Response.Headers["X-Correlation-Id"].ToString());
    }
}
