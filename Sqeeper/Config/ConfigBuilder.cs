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
            if (!ConfigVaidators.ValidateParam(name, url, nameof(url), ConfigVaidators.ValidateUrl) ||
                !ConfigVaidators.ValidateParam(name, path, nameof(path), ConfigVaidators.ValidatePath) ||
                !ConfigVaidators.ValidateParam<UpdateSource>(name, sourceType, nameof(path), ConfigVaidators.ValidateSourceType, out var st) ||
                st != UpdateSource.GitRepository && !ConfigVaidators.ValidateParam(name, version, nameof(version), ConfigVaidators.ValidateVersion))
            {
                Console.WriteLine($"\"{name}\" skipped. Not enough params.");
                return null;
            }

            //required params with default values
            var keepOld = !bool.TryParse(SettingOrDefault(opts, appGroupDefs, "keepOld"), out var ko) || ko;

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
    }
}
