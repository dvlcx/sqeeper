namespace Sqeeper.Config
{
    public class AppConfig
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string[] Query { get; set; }
        public string[] AntiQuery { get; set; }
        public bool KeepOld { get; set; }
        public UpdateSource SourceType { get; set; }
        public string? PostScript { get; set; }

        public AppConfig(string name, string version, string url, string path, string[] query, string[] antiQuery, bool keepOld, UpdateSource sourceType, string? postScript)
        {
            Name = name;
            Version = version;
            Url = url;
            Path = path;
            Query = query;
            AntiQuery = antiQuery;
            KeepOld = keepOld;
            SourceType = sourceType;
            PostScript = postScript;
        }
    }
    
    public enum UpdateSource
    {
        GitHubRelease,
        GitLabRelease,
        GitRepository,
        DirectoryIndex
    }
}