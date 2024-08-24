using System.Globalization;

namespace ExceptionHandling_Middleware.Middlewares
{
    public class CulturalMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Console.WriteLine("---------Extension:CulturalMiddleware start");

            var culturalQuery = context.Request.Query["culture"];

            if (!string.IsNullOrWhiteSpace(culturalQuery))
            {
                var culture = new CultureInfo(culturalQuery);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            context.Items["CulturalMiddleware"] = "Result from CulturalMiddleware (IMiddleware)";

            await next(context);

            Console.WriteLine("---------Extension:CulturalMiddleware end");
        }
    }
}
