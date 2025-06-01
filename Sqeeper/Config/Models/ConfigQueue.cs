using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sqeeper.Config.Models
{
    public class ConfigQueue : IConfigArray
    {
        public int Length { get => _appConfigs.Length; }
        private AppConfig[] _appConfigs;

        public ConfigQueue(AppConfig[] appConfigs)
        {
            _appConfigs = appConfigs;
        }

        public AppConfig Get(int index) =>
            _appConfigs[index];
    }
}