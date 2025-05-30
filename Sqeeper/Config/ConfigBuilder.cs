using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public ConfigBuilder IncludeDefaults()
        {
            _defaults = SectionsChildrenOrDefault("default");
            return this;   
        }

        public ConfigBuilder IncludeGroupDefaults()
        {
            _defaults = SectionsChildrenOrDefault("group");
            return this;
        }

        //there is 
        public Queue<AppConfig> Build()
        {
            try
            {
                Queue<AppConfig> result = [];

                if (_appsConfig is null)
                    return result;

                foreach (var app in _appsConfig)
                {
                    var opts = app.GetChildren();

                    var group = SettingOrDefault(null, "group");
                    var appGroupDefs = _groupsConfig?.FirstOrDefault(x => x.Key == group)?.GetChildren();

                    //required params without default values
                    var name = app.Key;
                    var url = SettingOrDefault(appGroupDefs, "url");
                    var path = SettingOrDefault(appGroupDefs, "path");
                    if (!CheckRequired([name, url, path], [nameof(name), nameof(url), nameof(path)]))
                    {
                        _logger.ZLogError($"App \"{name}\" skipped. Not enough params.");
                        continue;
                    }

                    //required params with default values
                    var keepOld = bool.TryParse(SettingOrDefault(appGroupDefs, "keepOld"), out var ko) ? ko : true;
                    var isGithub = bool.TryParse(SettingOrDefault(appGroupDefs, "isGithub"), out var ig) ? ig : false;
                    //optional params
                    var query = SettingOrDefault(appGroupDefs, "query")?.Split(';');
                    var postScript = SettingOrDefault(appGroupDefs, "postScript");

                    result.Enqueue(new AppConfig(name, url!, path!, query, keepOld, isGithub, postScript));
                }

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private IEnumerable<IConfigurationSection>? SectionsChildrenOrDefault(string sectionName) =>
            _defaults = _baseConfig.GetSection(sectionName) is var section && section.Exists() ? section.GetChildren() : null;
    
        private string? SettingOrDefault(IEnumerable<IConfigurationSection>? groupSection, string settingName) =>
            _appsConfig?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            groupSection?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            _defaults?.FirstOrDefault(o => o.Key == settingName)?.Value;

        private bool CheckRequired(string?[] parameters, string[] parameterNames)
        {
            bool result = false;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i] is null)
                {
                    _logger.ZLogError($"App \"{parameters[0]}\" has required parameter \"{parameterNames[i]}\" missing.");
                    result = true;
                }
            return result;
        }
    }
}