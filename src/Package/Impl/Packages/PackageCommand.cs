using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal abstract class PackageCommand : OleMenuCommand {
        protected PackageCommand(Guid group, int id) :
            base(OnCommand, new CommandID(group, id)) {

            BeforeQueryStatus += OnBeforeQueryStatus;
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e) {
            PackageCommand command = sender as PackageCommand;
            if (command != null) {
                command.SetStatus();
            }
        }

        internal virtual void SetStatus() { }
        internal virtual void Handle() { }

        public static void OnCommand(object sender, EventArgs args) {
            var command = sender as PackageCommand;
            if (command != null) {
                command.Handle();
            }
        }
    }
}
