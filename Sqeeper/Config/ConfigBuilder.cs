using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqeeper.Config.Models;
using ZLogger;

namespace Sqeeper.Config
{
    public class ConfigBuilder
    {
        private ILogger<ConfigBuilder> _logger;
        private IConfiguration _baseConfig;
        private IEnumerable<IConfigurationSection>? _defaults = default;
        private IEnumerable<IConfigurationSection>? _groupsConfig = default;
        private IEnumerable<IConfigurationSection>? _appsConfig = default;

        public ConfigBuilder(IConfiguration baseConfig, ILogger<ConfigBuilder> logger)
        {
            _baseConfig = baseConfig;
            _logger = logger;
        }

        public ConfigBuilder IncludeApps()
        {
            _appsConfig = SectionOrDefault("app")?.GetChildren();
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
            _defaults = SectionOrDefault("default")?.GetChildren();
            return this;
        }

        public ConfigBuilder IncludeGroupDefaults()
        {
            _groupsConfig = SectionOrDefault("group")?.GetChildren();
            return this;
        }

        public ConfigArray Build()
        {
            try
            {
                List<AppConfig> result = [];

                if (_appsConfig is null)
                {
                    _logger.ZLogError($"No apps found in config.");
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
            catch (Exception e)
            {
                throw;
            }
        }

        private AppConfig? ComposeAppConfig(IConfigurationSection appSection)
        {
            var opts = appSection.GetChildren();

            var group = SettingOrDefault(appSection, null, "group");
            var appGroupDefs = _groupsConfig?.FirstOrDefault(x => x.Key == group)?.GetChildren();

            //required params without default values
            var name = appSection.Key;
            var url = SettingOrDefault(appSection, appGroupDefs, "url");
            var path = SettingOrDefault(appSection, appGroupDefs, "path");
            if (!CheckRequired([name, url, path], [nameof(name), nameof(url), nameof(path)]))
            {
                _logger.ZLogError($"App \"{name}\" skipped. Not enough params.");
                return null;
            }

            //required params with default values
            var keepOld = bool.TryParse(SettingOrDefault(appSection,appGroupDefs, "keepOld"), out var ko) ? ko : true;
            var isGithub = bool.TryParse(SettingOrDefault(appSection,appGroupDefs, "isGithub"), out var ig) ? ig : false;
            //optional params
            var query = SettingOrDefault(appSection,appGroupDefs, "query")?.Split(';');
            var postScript = SettingOrDefault(appSection, appGroupDefs, "postScript");

            return new AppConfig(name, url!, path!, query, keepOld, isGithub, postScript);
        }

        private IConfigurationSection? SectionOrDefault(string sectionName) =>
            _baseConfig.GetSection(sectionName) is var section && section.Exists() ? section : null;
    
        private string? SettingOrDefault(IConfigurationSection appSection, IEnumerable<IConfigurationSection>? groupSection, string settingName) =>
            appSection.GetChildren().FirstOrDefault(o => o.Key == settingName)?.Value ??
            groupSection?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            _defaults?.FirstOrDefault(o => o.Key == settingName)?.Value;

        private bool CheckRequired(string?[] parameters, string[] parameterNames)
        {
            bool result = true;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i] is null)
                {
                    _logger.ZLogError($"App \"{parameters[0]}\" has required parameter \"{parameterNames[i]}\" missing.");
                    result = false;
                }
            return result;
        }
    }
}