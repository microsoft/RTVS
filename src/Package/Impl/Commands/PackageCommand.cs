// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal abstract class PackageCommand : OleMenuCommand {
        protected PackageCommand(Guid group, int id) :
            base(OnCommand, new CommandID(group, id)) {

            BeforeQueryStatus += OnBeforeQueryStatus;
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e) {
            PackageCommand command = sender as PackageCommand;
            command?.SetStatus();
        }

        protected virtual void SetStatus() { }

        protected virtual void Handle() { }

        protected virtual void Handle(object inArg, out object outArg) {
            outArg = null;
            Handle();
        }

        public static void OnCommand(object sender, EventArgs args) {
            var command = sender as PackageCommand;
            if (command != null) {
                object inArg, outArg;

                var oleArgs = args as OleMenuCmdEventArgs;
                inArg = oleArgs?.InValue;

                command.Handle(inArg, out outArg);

                if (oleArgs != null && oleArgs.OutValue != IntPtr.Zero) {
                    Marshal.GetNativeVariantForObject(outArg, oleArgs.OutValue);
                }
            }
        }
    }
}
