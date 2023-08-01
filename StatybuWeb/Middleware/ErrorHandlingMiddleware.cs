public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Items["Exception"] = ex;

            HandleException(context, ex);

        }
    }

    private void HandleException(HttpContext context, Exception exception)
    {
        context.Response.Clear();
        context.Response.Redirect("/Home/Error");
        return;
    }
}
