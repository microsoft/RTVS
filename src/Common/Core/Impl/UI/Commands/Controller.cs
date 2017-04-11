// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core.UI.Commands {
    /// <summary>
    /// Command controller (Model-View-Controller pattern)
    /// </summary>
    public class Controller : ICommandTarget, IDisposable {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<Guid, Dictionary<int, ICommand>> CommandMap { get; private set; }

        /// <summary>
        /// Chained controller is the one below this one in the controller chain.
        /// In regular scenarios it is a core editor typing/command controller.
        /// In contained language scenarios this may be a contained language
        /// controller and core editor controller is then chained to the 
        /// contained language typing/command controller.
        /// </summary>
        public virtual ICommandTarget ChainedController {
            get { return _chainedController; }
            set { _chainedController = value; }
        }

        private ICommandTarget _chainedController;

        // From outside of the base editor we should always use the ViewController only
        public Controller()
            : this(null) {
        }

        public Controller(ICommandTarget chainedController) {
            CommandMap = new Dictionary<Guid, Dictionary<int, ICommand>>();
            _chainedController = chainedController;
        }

        #region ICommandTarget
        public virtual CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            CommandResult result = CommandResult.NotSupported;
            ICommand cmd = Find(group, id);

            // Pre-process the command
            if (cmd != null) {
                result = cmd.Invoke(group, id, inputArg, ref outputArg);
            }

            if (!result.WasExecuted && ChainedController != null) {
                result = ChainedController.Invoke(group, id, inputArg, ref outputArg);

                // Post-process the command
                if (cmd != null && result.WasExecuted) {
                    cmd.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
                }
            }

            return result;
        }

        /// <summary>
        /// Non-routed status allows controller to return command status before contained 
        /// language processes it. For example, HTML controller may decide to handle 
        /// comment/uncomment command at HTML level rather than pass it down to CSS, JScript, 
        /// C#, etc. Handling status before regular controller chain allows top level controller
        /// to intercept command status.
        /// </summary>
        /// <returns>CommandStatus.NotSupported if Status request should be routed down 
        /// the regular chain. Other status stops Status request routing and returns
        /// status immediately to the caller.</returns>
        public virtual CommandStatus NonRoutedStatus(Guid group, int id, object inputArg) {
            ICommand cmd = Find(group, id);

            if (cmd != null)
                return cmd.Status(group, id);

            return CommandStatus.NotSupported;
        }

        public virtual CommandStatus Status(Guid group, int id) {
            var status = NonRoutedStatus(group, id, null);

            if (status != CommandStatus.NotSupported)
                return status;

            if (ChainedController != null)
                return ChainedController.Status(group, id);

            return CommandStatus.NotSupported;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            ICommand cmd = Find(group, id);

            if (cmd != null && ChainedController == null)
                cmd.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
        }
        #endregion

        /// <summary>
        /// Retrieves commands for a particular group
        /// </summary>
        /// <param name="group">Group id</param>
        /// <returns>Map of command id to command object</returns>
        public IDictionary<int, ICommand> GetGroupCommands(Guid group) {
            Dictionary<int, ICommand> groupCommands = null;

            if (CommandMap.TryGetValue(group, out groupCommands)) {
                return groupCommands;
            }

            return new Dictionary<int, ICommand>();
        }

        /// <summary>
        /// Adds command to the controller command table
        /// </summary>
        /// <param name="command">Command object</param>
        public void AddCommand(ICommand command) {
            if (command != null) {
                foreach (CommandId commandId in command.CommandIds) {
                    Dictionary<int, ICommand> idToCommandMap = null;

                    if (!CommandMap.TryGetValue(commandId.Group, out idToCommandMap)) {
                        idToCommandMap = new Dictionary<int, ICommand>();
                        CommandMap.Add(commandId.Group, idToCommandMap);
                    }

                    if (!idToCommandMap.ContainsKey(commandId.Id))
                        idToCommandMap.Add(commandId.Id, command);
                }
            }
        }

        /// <summary>
        /// Adds a set of commands to the controller command table
        /// </summary>
        /// <param name="commands">List of commands</param>
        public void AddCommandSet(IEnumerable<ICommand> commands) {
            if (commands != null) {
                foreach (ICommand command in commands) {
                    AddCommand(command);
                }
            }
        }

        public ICommand Find(Guid group, int id) {
            Dictionary<int, ICommand> idToCommandMap = null;

            if (!CommandMap.TryGetValue(group, out idToCommandMap))
                return null;

            ICommand cmd = null;
            idToCommandMap.TryGetValue(id, out cmd);

            return cmd;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            foreach (var kvp in CommandMap) {
                var disposable = kvp.Value as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            ChainedController = null;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
