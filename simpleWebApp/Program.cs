//https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0

using simpleWebApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register as a service 
builder.Services.AddSingleton<ConsoleLogMiddleware>();

var app = builder.Build();

// in-line middleware 
app.Use(async (context, next) =>
{
    Console.WriteLine("before from program.cs");
    await next();
    Console.WriteLine("after from program.cs");
});

/* 
 * any time we want to branch two request we can we map
 * completly breaks off the original middleware pipelines
 * any thing after that not going to be invoked
 */

app.Map("/map", HandleMapTest);

//if the query contains localhost:1234/?branch=main then response would be Branch used = main
app.MapWhen(context => context.Request.Query.ContainsKey("branch"), HandleBranch);

// there is another middleware called useWhen. unlike mapWhen it does not short circuit then middlware pipeline
app.UseWhen(context => context.Request.Query.ContainsKey("branch"),
    appBuilder => HandleBranchAndRejoin(appBuilder));

app.UseMiddleware<ConsoleLogMiddleware>();

app.Run(async context =>
{
    await context.Response.WriteAsync("Hello from non-Map delegate. <p>");
});

app.Run();

// map handler
static void HandleMapTest(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        await context.Response.WriteAsync("Map Test 1");
    });
}

// map when handler
static void HandleBranch(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        var branchVer = context.Request.Query["branch"];
        await context.Response.WriteAsync($"Branch used = {branchVer}");
    });
}

// use when handler 
void HandleBranchAndRejoin(IApplicationBuilder app)
{
    var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

    app.Use(async (context, next) =>
    {
        var branchVer = context.Request.Query["branch"];
        logger.LogInformation("Branch used = {branchVer}", branchVer);

        // Do work that doesn't write to the Response.
        await next();
        // Do other work that doesn't write to the Response.
    });
}