// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class ShowContextMenuCommand : ViewCommand {
        private Guid _cmdSetGuid;
        private Guid _packageGuid;
        private int _menuId;
        private IMenuCommandService _menuService;
        private bool _triedGetMenuService;

        public ShowContextMenuCommand(ITextView textView, Guid packageGuid, Guid cmdSetGuid, int menuId)
            : base(textView, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU), false) {

            _cmdSetGuid = cmdSetGuid;
            _packageGuid = packageGuid;
            _menuId = menuId;
        }

        public override CommandStatus Status(Guid group, int id) {
            return MenuCommandService != null ? CommandStatus.SupportedAndEnabled : CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (MenuCommandService != null) {
                POINTS[] position = (POINTS[])inputArg;
                CommandID menuCommand = new CommandID(_cmdSetGuid, (int)_menuId);
                MenuCommandService.ShowContextMenu(menuCommand, position[0].x, position[0].y);

                return CommandResult.Executed;
            }

            return CommandResult.NotSupported;
        }

        // IMenuCommandService is null in weird scenarios, such as Open With <non-html editor>, or diff view
        private IMenuCommandService MenuCommandService {
            get {
                if (_menuService == null && !_triedGetMenuService) {
                    _triedGetMenuService = true;

                    IVsShell shell = VsAppShell.Current.GlobalServices.GetService<IVsShell>();
                    IVsPackage package;
                    shell.LoadPackage(ref _packageGuid, out package);
                    if (package != null) {
                        var services = package as IServiceProvider;
                        _menuService = services.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
                    }

                    Debug.Assert(_menuService != null);
                }

                return _menuService;
            }
        }
    }
}
