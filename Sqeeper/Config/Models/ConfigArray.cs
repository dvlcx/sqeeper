namespace Sqeeper.Config.Models
{
    public class ConfigArray : IConfigArray
    {
        public int Length { get => _appConfigs.Length; }
        private readonly AppConfig[] _appConfigs;

        public ConfigArray(AppConfig[] appConfigs)
        {
            _appConfigs = appConfigs;
        }

        public AppConfig Get(int index) =>
            _appConfigs[index];
    }
}
