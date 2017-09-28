// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Commands {
    internal sealed class Controller : IController {
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand> {
            {GetInterpreterPathCommand.Name, new GetInterpreterPathCommand()}
        };

        private readonly IServiceContainer _services;
        public Controller(IServiceContainer services) {
            _services = services;
        }

        public static string[] Commands => _commands.Keys.ToArray();

        public object Execute(string command, params object[] args) {
            if (_commands.TryGetValue(command, out var cmd)) {
                return cmd.Execute(_services, args);
            }
            Debug.Fail($"Unknown command {command}");
            return null;
        }
    }
}
