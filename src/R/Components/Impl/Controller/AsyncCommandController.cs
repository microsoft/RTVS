// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;

namespace Microsoft.R.Components.Controller {
    public class AsyncCommandController : ICommandTarget {
        public Dictionary<Guid, Dictionary<int, IAsyncCommand>> CommandMap { get; } = new Dictionary<Guid, Dictionary<int, IAsyncCommand>>();

        public AsyncCommandController() {
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            CommandResult result = CommandResult.NotSupported;
            var cmd = Find(group, id);

            // Pre-process the command
            if (cmd != null) {
                cmd.InvokeAsync().DoNotWait();
                result = CommandResult.Executed;
            }

            return result;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }

        public CommandStatus Status(Guid group, int id) {
            IAsyncCommand cmd = Find(group, id);

            if (cmd != null)
                return cmd.Status;

            return CommandStatus.NotSupported;
        }

        /// <summary>
        /// Adds command to the controller command table
        /// </summary>
        /// <param name="command">Command object</param>
        public void AddCommand(Guid group, int id, IAsyncCommand command) {
            Dictionary<int, IAsyncCommand> idToCommandMap = null;

            if (!CommandMap.TryGetValue(group, out idToCommandMap)) {
                idToCommandMap = new Dictionary<int, IAsyncCommand>();
                CommandMap.Add(group, idToCommandMap);
            }

            if (!idToCommandMap.ContainsKey(id)) {
                idToCommandMap.Add(id, command);
            }
        }

        public IAsyncCommand Find(Guid group, int id) {
            Dictionary<int, IAsyncCommand> idToCommandMap = null;

            if (!CommandMap.TryGetValue(group, out idToCommandMap))
                return null;

            IAsyncCommand cmd = null;
            idToCommandMap.TryGetValue(id, out cmd);

            return cmd;
        }
    }
}
