using System.Text.RegularExpressions;

namespace Sqeeper.Core;

public static class Utils
{
    public static string? TryExtractVersion(string line)
    {
        var regex = new Regex(@"([0-9]\d*(\.[0-9]\d*)*)");
        var match = regex.Match(line);
        return match.Success ? match.Groups[1].Value : null;
    }
    
    public static string? TryExtractHref(string line)
    {
        var regex = new Regex(@"<a\s+(?:[^>]*?\s+)?href=([""'])(.*?)\1");
        var match = regex.Match(line);
        return match.Success ? match.Groups[2].Value : null;
    }

    public static bool IsNewerVersion(string currentVersion, string foundVersion)
    {
        var versionNumbers1 = currentVersion.Split('.').Select(int.Parse).ToArray();
        var versionNumbers2 = foundVersion.Split('.').Select(int.Parse).ToArray();
        if (versionNumbers1.Length != versionNumbers2.Length)
            return false;
        
        for (var i = 0; i < versionNumbers1.Length; i++)
        {
            if (versionNumbers1[i] < versionNumbers2[i])
                return true;
            else if (versionNumbers1[i] > versionNumbers2[i])
                return false;
        }
        return false;
    }

    public static string[] GetLinesWithMaxVersions(string[] lines)
    {
        var maxVersion = TryExtractVersion(lines[0]);
        for (var i = 1; i < lines.Length; i++)
        {
            var version = TryExtractVersion(lines[i]);
            if (maxVersion is null)
                maxVersion = version;
            else if (version is null)
                continue;
            else if (IsNewerVersion(maxVersion, version))
                maxVersion = version;
        }
        
        return lines.Where(x => x.Contains(maxVersion)).ToArray();
    }
}