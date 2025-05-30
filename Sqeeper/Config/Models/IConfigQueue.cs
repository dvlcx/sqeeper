using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sqeeper.Config.Models
{
    public interface IConfigQueue
    {
        public AppConfig Dequeue();
        public int Count { get; }
    }
}