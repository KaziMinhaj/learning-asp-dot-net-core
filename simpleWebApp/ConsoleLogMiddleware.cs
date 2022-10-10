namespace simpleWebApp
{
    public class ConsoleLogMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Console.WriteLine("Before from console log middleware");
            await next(context);
            Console.WriteLine("After from console log middleware");
        }
    }
}
