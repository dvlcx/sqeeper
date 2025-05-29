namespace Sqeeper.Config
{
    public class AppConfig
    {
        public string Name { get; set; }
        public string? Type { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string[]? Query { get; set; }
        public bool KeepOld { get; set; }
        public bool IsGithub { get; set; }
        public string? PostScript { get; set; }

        public AppConfig(string name, string? type, string url, string path, string[]? query, bool keepOld, bool isGithub, string? postScript)
        {
            Name = name;
            Type = type;
            Url = url;
            Path = path;
            Query = query;
            KeepOld = keepOld;
            IsGithub = isGithub;
            PostScript = postScript;
        }
    }
}