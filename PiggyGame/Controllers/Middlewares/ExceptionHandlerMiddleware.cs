using PiggyGame.Common.Exceptions;

namespace PiggyGame.Controllers.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    
    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var messages = new List<string>();

            switch (exception)
            {
                case UnauthorizedException:
                    context.Response.StatusCode = 401;
                    messages.Add(exception.Message);
                    break;
                case EntityWasNotFoundException:
                    context.Response.StatusCode = 404;
                    messages.Add(exception.Message);
                    break;
                case ArgumentException:
                    context.Response.StatusCode = 400;
                    messages.Add(exception.Message);
                    break;
                case InvalidOperationException:
                    context.Response.StatusCode = 400;
                    messages.Add(exception.Message);
                    break;
                default:
                    context.Response.StatusCode = 500;
                    messages.Add(exception.Message);
                    
                    _logger.LogError(exception, exception.Message);
                    
                    break;
            }

            var response = new
            {
                Errors = messages.ToArray()
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}