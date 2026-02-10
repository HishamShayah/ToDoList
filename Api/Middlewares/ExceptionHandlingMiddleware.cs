using Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            int statusCode;
            string message;

            switch (ex)
            {
                case KeyNotFoundException:
                    statusCode = 404;
                    message = ex.Message;
                    break;

                case ArgumentException:
                case InvalidOperationException:
                    statusCode = 400;
                    message = ex.Message;
                    break;

                default:
                    statusCode = 500;
                    message = "An unexpected error occurred.";
                    break;
            }

            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = _env.IsDevelopment()
                ? new ErrorResponse
                {
                    Status = statusCode,
                    Message = message,
                    StackTrace = ex.StackTrace
                }
                : new ErrorResponse
                {
                    Status = statusCode,
                    Message = "An unexpected error occurred."
                };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
