using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sqeeper.Config.Models
{
    public interface IMainConfig
    {
        public AppConfig Dequeue();
        public int Count { get; }
    }
}