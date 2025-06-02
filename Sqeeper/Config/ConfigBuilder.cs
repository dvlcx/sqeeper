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
            _appsConfig = SectionsChildrenOrDefault("app");
            return this;
        }
        public ConfigBuilder IncludeApp(string appName)
        {
            _appsConfig = SectionsChildrenOrDefault("app." + appName);
            return this;
        }
        
        public ConfigBuilder IncludeDefaults()
        {
            _defaults = SectionsChildrenOrDefault("default");
            return this;
        }

        public ConfigBuilder IncludeGroupDefaults()
        {
            _groupsConfig = SectionsChildrenOrDefault("group");
            return this;
        }

        public ConfigQueue Build()
        {
            try
            {
                List<AppConfig> result = [];

                if (_appsConfig is null)
                    return new ConfigQueue([]);

                foreach (var app in _appsConfig)
                {
                    var opts = app.GetChildren();

                    var group = SettingOrDefault(app, null, "group");
                    var appGroupDefs = _groupsConfig?.FirstOrDefault(x => x.Key == group)?.GetChildren();

                    //required params without default values
                    var name = app.Key;
                    var url = SettingOrDefault(app, appGroupDefs, "url");
                    var path = SettingOrDefault(app, appGroupDefs, "path");
                    if (!CheckRequired([name, url, path], [nameof(name), nameof(url), nameof(path)]))
                    {
                        _logger.ZLogError($"App \"{name}\" skipped. Not enough params.");
                        continue;
                    }

                    //required params with default values
                    var keepOld = bool.TryParse(SettingOrDefault(app,appGroupDefs, "keepOld"), out var ko) ? ko : true;
                    var isGithub = bool.TryParse(SettingOrDefault(app,appGroupDefs, "isGithub"), out var ig) ? ig : false;
                    //optional params
                    var query = SettingOrDefault(app,appGroupDefs, "query")?.Split(';');
                    var postScript = SettingOrDefault(app, appGroupDefs, "postScript");

                    result.Add(new AppConfig(name, url!, path!, query, keepOld, isGithub, postScript));
                }

                return new ConfigQueue(result.ToArray());
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private IEnumerable<IConfigurationSection>? SectionsChildrenOrDefault(string sectionName) =>
            _baseConfig.GetSection(sectionName) is var section && section.Exists() ? section.GetChildren() : null;
    
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