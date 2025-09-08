namespace Sqeeper.Config.Models
{
    public class AppConfig : ILinkConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool KeepOld { get; set; }
        public string? PostScript { get; set; }

        public AppConfig(string name, string version, string url, string path, string[] query, string[] antiQuery,
            bool keepOld, UpdateSource sourceType, string? queryablePropertyName, string? versionPropertyName, string? postScript)
        {
            Name = name;
            Version = version;
            Url = url;
            Path = path;
            Query = query;
            AntiQuery = antiQuery;
            KeepOld = keepOld;
            SourceType = sourceType;
            QueryablePropertyName = queryablePropertyName;
            VersionPropertyName = versionPropertyName;
            PostScript = postScript;
        }
    }
    
    public enum UpdateSource
    {
        GitHubRelease,
        GitLabRelease,
        CodebergRelease,
        GitRepository,
        DirectoryIndex
    }
}