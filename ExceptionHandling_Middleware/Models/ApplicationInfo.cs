using System.Diagnostics.CodeAnalysis;

namespace ExceptionHandling_Middleware.Models;

//[SuppressMessage("Compiler", "CS1591:Missing XML comment for publicly visible type or member")]
public class ApplicationInfo
{
    public string ApplicationName { get; set; }
    public string DisplayName { get; set; }
    public string Title { get; set; }
    public bool EnableSwagger { get; set; }
    public AzureAppInsights AzureAppInsights { get; set; }
    public AzureAppConfig AzureAppConfig { get; set; }
}

public class AzureAppInsights
{
    public string AppInsightsConnectionString { get; set; }
    public string LogLevelAzureAppInsight { get; set; }
}

public class AzureAppConfig
{
    public string AppConfigConnectionString { get; set; }
    public List<SelectFilter> SelectFilters { get; set; }
}

public struct SelectFilter
{
    public string Key { get; set; }
    public string Label { get; set; }
}


public class AzureConnectionStrings
{
    public string AzureAppConfig { get; set; }
    public string AzureAppInsights { get; set; }
}

