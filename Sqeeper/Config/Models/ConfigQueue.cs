using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sqeeper.Config.Models
{
    public class ConfigQueue : IConfigQueue
    {
        public int Count { get => _appConfigs.Count; }
        private Queue<AppConfig> _appConfigs;

        public ConfigQueue(Queue<AppConfig> appConfigs)
        {
            _appConfigs = appConfigs;
        }

        public AppConfig Dequeue() =>
            _appConfigs.Dequeue();
    }
}