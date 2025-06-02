using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using Sqeeper.Config.Models;
using Sqeeper.Core;
using ZLogger;

namespace Sqeeper.Command
{

    public class UpdateCommand
    {
        private UpdateService _updateService;
        private ILogger<UpdateCommand> _logger;

        public UpdateCommand(UpdateService updateService, ILogger<UpdateCommand> logger)
        {
            _updateService = updateService;
            _logger = logger;
        }

        /// <summary>
        ///     Runs through the config and updates all mentioned apps.
        /// </summary>
        /// <param name="name">-n, Name of the app to update.</param>
        public async Task Update(string? name = null)
        {
            await _updateService.Update(name);
        }
    }
}