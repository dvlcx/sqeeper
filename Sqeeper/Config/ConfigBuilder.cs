using Microsoft.Extensions.Configuration;
using Sqeeper.Config.Models;
using Sqeeper.Core.Links;

namespace Sqeeper.Config
{
    public class ConfigBuilder
    {
        private readonly IConfiguration _baseConfig;
        private IConfigurationSection[]? _defaults;
        private IConfigurationSection[]? _groupsConfig;
        private IConfigurationSection[]? _appsConfig;

        public ConfigBuilder(IConfiguration baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public ConfigBuilder IncludeApps()
        {
            _appsConfig = SectionOrDefault("app")?.GetChildren().ToArray();
            return this;
        }

        public ConfigBuilder IncludeApp(string appName)
        {
            var section = SectionOrDefault("app:" + appName);
            _appsConfig = section is null ? null : [section];
            return this;
        }

        public ConfigBuilder IncludeDefaults()
        {
            _defaults = SectionOrDefault("default")?.GetChildren().ToArray();
            return this;
        }

        public ConfigBuilder IncludeGroupDefaults()
        {
            _groupsConfig = SectionOrDefault("group")?.GetChildren().ToArray();
            return this;
        }

        public ConfigArray Build()
        {
            try
            {
                List<AppConfig> result = [];

                if (_appsConfig is null)
                {
                    Console.WriteLine($"No apps found in config.");
                    return new ConfigArray([]);
                }

                foreach (var app in _appsConfig)
                {
                    var appConfig = ComposeAppConfig(app);
                    if (appConfig is null)
                        continue;
                    result.Add(appConfig);
                }

                return new ConfigArray(result.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private AppConfig? ComposeAppConfig(IConfigurationSection appSection)
        {
            var opts = appSection.GetChildren().ToArray();

            var group = SettingOrDefault(opts, null, "group");
            var appGroupDefs = _groupsConfig?.FirstOrDefault(x => x.Key == group)?.GetChildren().ToArray();

            //required params without default values
            var name = appSection.Key;
            var version = SettingOrDefault(opts, appGroupDefs, "version");
            var url = SettingOrDefault(opts, appGroupDefs, "url");
            var path = SettingOrDefault(opts, appGroupDefs, "path");
            var sourceType = SettingOrDefault(opts, appGroupDefs, "sourceType");
            if (!ValidateParam(name, url, nameof(url), ValidateUrl) |
                !ValidateParam(name, path, nameof(path), ValidatePath) |
                !ValidateParam<UpdateSource>(name, sourceType, nameof(path), ValidateSourceType, out var st) |
                st != UpdateSource.GitRepository && !ValidateParam(name, version, nameof(version), ValidateVersion))
            {
                Console.WriteLine($"\"{name}\" skipped. Not enough params.");
                return null;
            }

            //required params with default values
            var keepOld = bool.TryParse(SettingOrDefault(opts, appGroupDefs, "keepOld"), out var ko) ? ko : true;

            //optional params
            var query = SettingOrDefault(opts, appGroupDefs, "query")?.Split(',') ?? [];
            var antiQuery = SettingOrDefault(opts, appGroupDefs, "antiQuery")?.Split(',') ?? [];
            var queryablePropertyName = SettingOrDefault(opts, appGroupDefs, "queryablePropertyName");
            var versionPropertyName = SettingOrDefault(opts, appGroupDefs, "versionPropertyName");
            var postScript = SettingOrDefault(opts, appGroupDefs, "postScript");

            return new AppConfig(name, version!, url!, path!, query, antiQuery, keepOld, st, queryablePropertyName, versionPropertyName, postScript);
        }

        private IConfigurationSection? SectionOrDefault(string sectionName) =>
            _baseConfig.GetSection(sectionName) is var section && section.Exists() ? section : null;

        private string? SettingOrDefault(IConfigurationSection[] appSection, IConfigurationSection[]? groupSection, string settingName) =>
            appSection.FirstOrDefault(o => o.Key == settingName)?.Value ??
            groupSection?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            _defaults?.FirstOrDefault(o => o.Key == settingName)?.Value;

        private delegate T3 Func<in T1, T2, out T3>(T1 input, out T2 output);
        private bool ValidateParam<T>(string appName, string? param, string paramName, Func<string, T, bool> validator, out T? result)
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

        private bool ValidateParam(string appName, string? param, string paramName, Func<string, bool> validator) =>
            ValidateParam<bool>(appName, param, paramName, (string input, out bool output) => (output = validator(input)), out _);

        private bool ValidateUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out _);

        private bool ValidatePath(string path) =>
            Directory.Exists(path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

        private bool ValidateVersion(string version) =>
            Utils.TryExtractVersion(version) is not null;

        private bool ValidateSourceType(string sourceTypeString, out UpdateSource sourceType) =>
            Enum.TryParse<UpdateSource>(sourceTypeString, true, out sourceType);
    }
}
