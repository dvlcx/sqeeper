using System.Text.RegularExpressions;

namespace Sqeeper.Config
{
    public class AppConfig
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string[] Query { get; set; }
        public bool KeepOld { get; set; }
        public bool IsGithub { get; set; }
        public string? PostScript { get; set; }

        public AppConfig(string name, string version, string url, string path, string[] query, bool keepOld, bool isGithub, string? postScript)
        {
            Name = name;
            Version = version;
            Url = url;
            Path = path;
            Query = query;
            KeepOld = keepOld;
            IsGithub = isGithub;
            PostScript = postScript;
        }
    }
}