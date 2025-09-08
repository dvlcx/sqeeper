namespace Sqeeper.Config.Models;

public class ILinkConfig
{
    public string Url { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string[] Query { get; set; }
    public string[] AntiQuery { get; set; }
    public UpdateSource SourceType { get; set; }
    public string? QueryablePropertyName { get; set; } // RELEASE MODE ONLY. Starts from platform specific prop
    public string? VersionPropertyName { get; set; } // RELEASE MODE ONLY. Starts from JSON ROOT
}