// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class MenuCommandServiceMock : IMenuCommandService {
        Dictionary<CommandID, MenuCommand> _commands = new Dictionary<CommandID, MenuCommand>();

        public DesignerVerbCollection Verbs {
            get {
                throw new NotImplementedException();
            }
        }

        public void AddCommand(MenuCommand command) {
            _commands[command.CommandID] = command;
        }

        public void AddVerb(DesignerVerb verb) {
        }

        public MenuCommand FindCommand(CommandID commandID) {
            MenuCommand command;
            _commands.TryGetValue(commandID, out command);

            return command;
        }

        public bool GlobalInvoke(CommandID commandID) {
            MenuCommand command;
            _commands.TryGetValue(commandID, out command);

            if (command != null) {
                command.Invoke();
                return true;
            }

            return false;
        }

        public void RemoveCommand(MenuCommand command) {
            _commands.Remove(command.CommandID);
        }

        public void RemoveVerb(DesignerVerb verb) {
        }

        public void ShowContextMenu(CommandID menuID, int x, int y) {
        }
    }
}
