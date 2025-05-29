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
        private IEnumerable<IConfigurationSection>? _typesConfig = default;
        private IEnumerable<IConfigurationSection>? _appsConfig = default;

        public ConfigBuilder(IConfiguration baseConfig, ILogger<ConfigBuilder> logger)
        {
            _baseConfig = baseConfig;
            _logger = logger;
        }

        public ConfigBuilder IncludeApps()
        {
            _appsConfig = SectionsChildrenOrDefault("apps");
            return this;
        }

        public ConfigBuilder IncludeDefaults()
        {
            _defaults = SectionsChildrenOrDefault("default");
            return this;   
        }

        public ConfigBuilder IncludeTypeDefaults()
        {
            _defaults = SectionsChildrenOrDefault("types");
            return this;
        }

        public List<AppConfig> Build()
        {
            try
            {
                List<AppConfig> result = [];

                if (_appsConfig is null)
                    return result;

                foreach (var app in _appsConfig)
                {
                    var opts = app.GetChildren();

                    // filter param. not required
                    var type = SettingOrDefault(null, "type");
                    var appTypeDefs = _typesConfig?.FirstOrDefault(x => x.Key == type)?.GetChildren();

                    //required params without default values
                    var name = app.Key;
                    var url = SettingOrDefault(appTypeDefs, "url");
                    var path = SettingOrDefault(appTypeDefs, "path");
                    if (!CheckRequired([name, url, path], [nameof(name), nameof(url), nameof(path)]))
                    {
                        _logger.LogError($"App \"{name}\" skipped. Not enough params.");
                        continue;
                    }

                    //required params with default values
                    var keepOld = bool.TryParse(SettingOrDefault(appTypeDefs, "keepOld"), out var ko) ? ko : true;
                    var isGithub = bool.TryParse(SettingOrDefault(appTypeDefs, "isGithub"), out var ig) ? ig : false;

                    //optional params
                    var query = SettingOrDefault(appTypeDefs, "query")?.Split(';');
                    var postScript = SettingOrDefault(appTypeDefs, "postScript");

                    result.Add(new AppConfig(name, type, url!, path!, query, keepOld, isGithub, postScript));
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
    
        private string? SettingOrDefault(IEnumerable<IConfigurationSection>? typeSection, string settingName) =>
            _appsConfig?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            typeSection?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            _defaults?.FirstOrDefault(o => o.Key == settingName)?.Value;

        private bool CheckRequired(string?[] parameters, string[] parameterNames)
        {
            bool result = false;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i] is null)
                {
                    _logger.LogError($"App \"{parameters[0]}\" has required parameter \"{parameterNames[i]}\" missing.");
                    result = true;
                }
            return result;
        }
    }
}