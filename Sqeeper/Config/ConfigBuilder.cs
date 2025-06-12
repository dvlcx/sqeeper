using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqeeper.Config.Models;
using ZLogger;

namespace Sqeeper.Config
{
    public class ConfigBuilder
    {
        private readonly ILogger<ConfigBuilder> _logger;
        private readonly IConfiguration _baseConfig;
        private IConfigurationSection[]? _defaults;
        private IConfigurationSection[]? _groupsConfig;
        private IConfigurationSection[]? _appsConfig;

        public ConfigBuilder(IConfiguration baseConfig, ILogger<ConfigBuilder> logger)
        {
            _baseConfig = baseConfig;
            _logger = logger;
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
            var opts = appSection.GetChildren().ToArray();

            var group = SettingOrDefault(opts, null, "group");
            var appGroupDefs = _groupsConfig?.FirstOrDefault(x => x.Key == group)?.GetChildren().ToArray();

            //required params without default values
            var name = appSection.Key;
            var version = SettingOrDefault(opts, appGroupDefs, "version");
            var url = SettingOrDefault(opts, appGroupDefs, "url");
            ValidateParam(name, ref url, nameof(url), ValidateUrl);
            var path = SettingOrDefault(opts, appGroupDefs, "path");
            ValidateParam(name, ref path, nameof(path), ValidatePath);
            if (!CheckRequired([name, version, url, path], [nameof(name), nameof(version), nameof(url), nameof(path)]))
            {
                _logger.ZLogError($"\"{name}\" skipped. Not enough params.");
                return null;
            }

            //required params with default values
            var keepOld = bool.TryParse(SettingOrDefault(opts, appGroupDefs, "keepOld"), out var ko) ? ko : true;
            var isGithub = bool.TryParse(SettingOrDefault(opts, appGroupDefs, "isGithub"), out var ig) ? ig : false;
            
            //optional params
            var query = SettingOrDefault(opts, appGroupDefs, "query")?.Split(',') ?? [];
            var postScript = SettingOrDefault(opts, appGroupDefs, "postScript");

            return new AppConfig(name, version!, url!, path!, query, keepOld, isGithub, postScript);
        }

        private IConfigurationSection? SectionOrDefault(string sectionName) =>
            _baseConfig.GetSection(sectionName) is var section && section.Exists() ? section : null;
    
        private string? SettingOrDefault(IConfigurationSection[] appSection, IConfigurationSection[]? groupSection, string settingName) =>
            appSection.FirstOrDefault(o => o.Key == settingName)?.Value ??
            groupSection?.FirstOrDefault(o => o.Key == settingName)?.Value ??
            _defaults?.FirstOrDefault(o => o.Key == settingName)?.Value;

        private void ValidateParam(string appName, ref string? param, string paramName, Func<string, bool> validator)
        {
            if (param is not null && !validator(param))
            {
                _logger.ZLogError($"\"{appName}\" {paramName} is invalid.");
                param = null;
            }
        }
        
        private bool ValidateUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out _);

        private bool ValidatePath(string path) =>
            Directory.Exists(path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

        private bool CheckRequired(string?[] parameters, string[] parameterNames)
        {
            bool result = true;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i] is null)
                {
                    _logger.ZLogError($"App \"{parameters[0]}\" has required parameter \"{parameterNames[i]}\" missing/invalid.");
                    result = false;
                }
            return result;
        }
    }
}