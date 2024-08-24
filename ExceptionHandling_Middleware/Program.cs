#define LOCAL
#define AZURE
#define MIDDLE

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;

using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ExceptionHandling_Middleware.Middlewares;
using ExceptionHandling_Middleware.Extensions;
using Microsoft.OpenApi.Models;
using Azure.Core;
using Azure;
using DnsClient;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualBasic;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Buffers.Text;
using System.Reflection.Metadata;
using System;

// Middlewares in ASP.NET Core
// https://medium.com/codex/how-to-create-custom-middlewares-in-asp-net-core-5bb2f910da01
// https://adnanrafiq.com/blog/develop-intuitive-understanding-of-middleware-in-asp-net8/

// Install-Package Microsoft.ApplicationInsights.AspNetCore
// Install-Package Swashbuckle.AspNetCore.Annotations
// install-package Microsoft.Azure.AppConfiguration.AspNetCore                  //Azure App Configuration
// install-package Microsoft.FeatureManagement.AspNetCore                       //Azure App Configuration
// install-package Microsoft.Extensions.Configuration.AzureAppConfiguration     //Azure App Configuration

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//Register AppConfiguration and ApplicationInsights
builder.AddCustomConfiguration();

//string Sentinel = builder.Configuration["Sentinel"];
//string AppInsightsConnectionString = builder.Configuration["AppInsightsConnectionString"];
//string BackGroundColor = builder.Configuration["Settings:DemoStyles:BackGroundColor"];
string LOG_LEVEL = builder.Configuration["LOG_LEVEL"]?.ToLower() ?? "information";



// Add services to the container -------------------------------------------------------------------

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});


// Register Middleware 
builder.Services.AddScoped<CulturalMiddleware>();


var app = builder.Build();




// Configure the HTTP request pipeline -------------------------------------------------------------

var _logger = app.Logger;
_logger.LogCritical($"*** ExceptionHandling_Middleware is starting, Log Level: {LOG_LEVEL} ***");


#if LOCAL
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endif

#if AZURE
bool swaggerUI = app.Environment.IsDevelopment() ? true : true;     //on azure: IsDevelopment()=false
if (swaggerUI)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI as the default page
    });
}
#endif


app.UseHttpsRedirection();


// --------------------------------------- Middlewares ---------------------------------------------
//- Routing Middleware              : Responsible for mapping incoming requests to the appropriate endpoint based on defined routes.
//- Authentication Middleware       : Handles user authentication, ensuring secure access to your Minimal API endpoints.
//- Exception Handling Middleware   : Catches and handles exceptions, providing a centralized mechanism for managing errors in your API.
//- Logging Middleware              : Offers a way to log information about requests, responses, and other relevant details for troubleshooting and analysis.
//- CORS Middleware                 : Enables Cross - Origin Resource Sharing, allowing controlled access to your API from different domains.

#if MIDDLE

Console.WriteLine("\n");

app.Use(async (context, next) =>
{
    Console.WriteLine("---Middelware 1 start");

    context.Items["FirstMiddlewareResult"] = "This is Middelware 1 action (Minimal API)";
    await next.Invoke();

    Console.WriteLine("---Middelware 1 end");
});


app.Use(async (context, next) =>
{
    Console.WriteLine("------Middelware 2 start");

    //await context.Response.WriteAsync("This is Middelware 2 action\n");
    //--> This will create header update exception error 

    context.Items["SecondMiddlewareResult"] = "This is Middelware 2 action (Minimal API)";
    await next.Invoke();

    Console.WriteLine("------Middelware 2 end");
});

#endif
// --------------------------------------- Middlewares ---------------------------------------------



//Use Middleware
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<CulturalMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


// --------------------------------------- Endpoints  ----------------------------------------------

// Hello Word
app.MapGet("/foo", () =>
{
    return "Hello, World!";
})
.WithMetadata(new SwaggerOperationAttribute
{
    Summary = "Minimal API sample ",
    Description = "This method returns **Hello, World!.**"
});






//app.Run(async ctx =>
//{
//    await ctx.Response.WriteAsync("\n*** Welcome to Middleware Run(), This is the end of the pipeline. ***\n");
//});

// This statement require to connect "https://" to open web browser
Console.WriteLine("\n---app.Run();---\n");
app.Run();          

