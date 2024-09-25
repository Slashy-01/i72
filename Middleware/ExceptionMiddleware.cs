using System.Net;
using System.Text.Json.Serialization;
using MySqlConnector;
using Newtonsoft.Json;

namespace I72_Backend.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (MySqlException ex)
        {
            _logger.LogError($"MySQL query went wrong: {ex}");
            await HandleMySqlExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await HandleGneralExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleMySqlExceptionAsync(HttpContext context, MySqlException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "SQL query error. Check your request again.",
            Details = exception.Message
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
    
    private Task HandleGneralExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Something went wrong!",
            Details = exception.Message
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
