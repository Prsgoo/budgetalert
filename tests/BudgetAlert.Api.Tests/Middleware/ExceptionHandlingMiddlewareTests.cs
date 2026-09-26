using BudgetAlert.Api.Middleware;
using BudgetAlert.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BudgetAlert.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static (HttpContext context, MemoryStream body) CreateContext()
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        return (context, body);
    }

    private static async Task<JsonElement> ReadResponseAsync(MemoryStream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<JsonElement>(body);
    }

    [Fact]
    public async Task InvokeAsync_MapsNotFoundExceptionTo404WithCorrectEnvelope()
    {
        var (context, body) = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new NotFoundException("Budget", Guid.NewGuid()));

        await middleware.InvokeAsync(context);

        Assert.Equal(404, context.Response.StatusCode);
        var json = await ReadResponseAsync(body);
        Assert.Equal(404, json.GetProperty("status").GetInt32());
        Assert.Equal("Not Found", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvokeAsync_MapsValidationExceptionTo400WithFieldErrors()
    {
        var (context, body) = CreateContext();
        var failures = new[] { new ValidationFailure("Amount", "must be greater than zero") };
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ValidationException(failures));

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
        var json = await ReadResponseAsync(body);
        Assert.Equal(400, json.GetProperty("status").GetInt32());
        Assert.True(json.GetProperty("errors").TryGetProperty("Amount", out _));
    }

    [Fact]
    public async Task InvokeAsync_MapsUnhandledExceptionTo500WithoutExposingInternals()
    {
        var (context, body) = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("secret internal detail"));

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        var json = await ReadResponseAsync(body);
        Assert.Equal(500, json.GetProperty("status").GetInt32());
        Assert.DoesNotContain("secret internal detail", json.GetProperty("detail").GetString()!);
    }
}
