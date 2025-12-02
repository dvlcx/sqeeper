using Sqeeper.Config.Models;
using Sqeeper.Core.Links;

namespace Sqeeper.Config;

public static class ConfigVaidators
{
    public delegate T3 Func<in T1, T2, out T3>(T1 input, out T2 output);
    public static bool ValidateParam<T>(string appName, string? param, string paramName, Func<string, T, bool> validator, out T? result)
    {
        result = default;

        if (param is null)
        {
            Console.WriteLine($"\"{appName}\" {paramName} is missing.");
            return false;
        }
        if (!validator(param, out result))
        {
            Console.WriteLine($"\"{appName}\" {paramName} is invalid.");
            return false;
        }

        return true;
    }

    public static bool ValidateParam(string appName, string? param, string paramName, Func<string, bool> validator) =>
        ValidateParam<bool>(appName, param, paramName, (string input, out bool output) => (output = validator(input)), out _);

    public  static bool ValidateUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _);

    public  static bool ValidatePath(string path) =>
        Directory.Exists(path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

    public  static bool ValidateVersion(string version) =>
        Utils.TryExtractVersion(version) is not null;

    public  static bool ValidateSourceType(string sourceTypeString, out UpdateSource sourceType) =>
        Enum.TryParse<UpdateSource>(sourceTypeString, true, out sourceType);
}