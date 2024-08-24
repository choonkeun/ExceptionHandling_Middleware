namespace ExceptionHandling_Middleware.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine($"---------LoggingMiddleware: Request: {context.Request.Method} {context.Request.Path}");

            context.Items["LoggingMiddleware"] = "Result from LoggingMiddleware (RequestDelegate)";

            await _next(context);

            Console.WriteLine($"---------LoggingMiddleware: Response: {context.Response.StatusCode}");
        }

    }
}
