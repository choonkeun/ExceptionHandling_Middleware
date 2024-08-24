
using Microsoft.Extensions.Options;
using ExceptionHandling_Middleware.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Reflection.Emit;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace ExceptionHandling_Middleware.Extensions
{

    public static class CommonExtension
    {
        //Project: <PropertyGroup><NoWarn>$(NoWarn);1591</NoWarn></PropertyGroup>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Compiler", "CS1591")]
        public static void AddCustomConfiguration(this WebApplicationBuilder builder)
        {
            bool isYahoo = Boolean.TryParse(builder.Configuration["isYahoo"] ?? "false", out bool parsedValue) && parsedValue;
            var appInfo = builder.Configuration.GetSection(nameof(ApplicationInfo)).Get<ApplicationInfo>();

            // bind azure app configuration model
            if (!string.IsNullOrEmpty(appInfo?.AzureAppConfig?.AppConfigConnectionString))
                AddAzureAppConfiguration(builder, appInfo, isYahoo);

            // bind azure applicationInsights
            if (!string.IsNullOrEmpty(appInfo?.AzureAppInsights?.AppInsightsConnectionString))
                AddAzureApplicationInsights(builder, appInfo, isYahoo);

            // Add custom logger if you configure. otherwise, default logger will be used
            //SerilogExtension.AddSerilogExtension(builder.Configuration);
            //builder.Host.UseSerilog(Log.Logger);

            // bind configuration model
            builder.Services.Configure<ApplicationInfo>(builder.Configuration.GetSection(nameof(ApplicationInfo)));

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                // using System.Reflection;
                var xmlFilename = $"{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CS1061")]
        private static void AddAzureAppConfiguration(WebApplicationBuilder builder, ApplicationInfo appInfo, bool isYahoo)
        {

            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                var cnt = options.Connect(builder.Configuration.GetConfigValue(isYahoo, appInfo.AzureAppConfig.AppConfigConnectionString));
                foreach (var selectItem in appInfo.AzureAppConfig.SelectFilters)
                {
                    cnt.Select(selectItem.Key, selectItem.Label);
                }
                cnt.ConfigureKeyVault(kv =>
                {
                    //kv.SetCredential(new DefaultAzureCredential());
                });

                options.ConfigureRefresh(refresh =>
                {
                    refresh.Register("Sentinel", refreshAll: true);               //case sensitive
                    refresh.SetCacheExpiration(TimeSpan.FromSeconds(10));       //default: 30 seconds
                });

                options.UseFeatureFlags(options =>
                {
                    options.CacheExpirationInterval = TimeSpan.FromSeconds(10);  //default: 30 seconds
                    options.Select(KeyFilter.Any);
                });

            });

            //confirm Azure AppConfiguration Key value
            //string LOG_LEVEL = builder.Configuration["LOG_LEVEL"];
        }

        internal static string GetConfigValue(this IConfiguration configuration, bool isYahoo, string? value)
        {
            //var appInfo = configuration.GetSection("ConnectionStrings");
            //string connectionnString = appInfo["VS:AzureAppInsights"];

            string KeyPrefix = isYahoo ? "Yahoo:" : "VS:";
            string key = "ConnectionStrings:" + (isYahoo ? "Yahoo" : "VS");
            var section = configuration.GetSection(key).Get<AzureConnectionStrings>();

            //data will be retireved from providers including Azure.
            if (!string.IsNullOrEmpty(value) && value.StartsWith(@"~"))
            {
                return section.AzureAppConfig;
            }
            else
            {
                return value;
            }

        }

        private static void AddAzureApplicationInsights(WebApplicationBuilder builder, ApplicationInfo appInfo, bool isYahoo)
        {
            //var applicationInsightsString = builder.Configuration["AppInsightsConnectionString"];
            var applicationInsightsString = builder.Configuration.GetInsightsValue(isYahoo, appInfo?.AzureAppInsights?.AppInsightsConnectionString);

            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = applicationInsightsString,
                EnableActiveTelemetryConfigurationSetup = true,
            });
            var Logging = builder.Configuration.GetSection("Logging");      //appSettings.json

            //azure appConfiguration value override appsettings.json
            var logLevelEnv = builder.Configuration["LOG_LEVEL"]?.ToLower() ?? "information";
            builder.Logging.SetMinimumLevel(Enum.Parse<LogLevel>(logLevelEnv, true));

            //set logLevel
            LogLevel logLevel = Enum.Parse<LogLevel>(logLevelEnv, true);
            builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);

        }

        internal static string GetInsightsValue(this IConfiguration configuration, bool isYahoo, string? value)
        {
            string KeyPrefix = isYahoo ? "Yahoo:" : "VS:";
            string key = "ConnectionStrings:" + (isYahoo ? "Yahoo" : "VS");
            var section = configuration.GetSection(key).Get<AzureConnectionStrings>();

            //data will be retireved from providers including Azure.
            if (!string.IsNullOrEmpty(value) && value.StartsWith(@"~"))
            {
                return section.AzureAppInsights;
            }
            else
            {
                return value;
            }

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CS1061")]
        public static void UseCustomConfiguration(this WebApplication app)
        {
            //enable CORS
            app.UseCors(c => c.SetIsOriginAllowed(_ => true)
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials());

            var appInfo = app.Services.GetRequiredService<IOptions<ApplicationInfo>>().Value;
            // Configure the HTTP request pipeline.
            if (appInfo.EnableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

        }

    }


}
