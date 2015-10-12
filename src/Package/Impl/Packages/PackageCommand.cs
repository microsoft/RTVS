using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Commands
{
    internal abstract class PackageCommand : OleMenuCommand
    {
        protected PackageCommand(Guid group, int id) :
            base((sender, args) => new Handler().OnCommand(sender as PackageCommand),
                new CommandID(group, id))
        {
            this.BeforeQueryStatus += OnBeforeQueryStatus;
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            PackageCommand command = sender as PackageCommand;
            if (command != null)
            {
                command.SetStatus();
            }
        }

        protected virtual void SetStatus() { }
        protected virtual void Handle() { }

        private class Handler
        {
            public void OnCommand(PackageCommand command)
            {
                if (command != null)
                {
                    command.Handle();
                }
            }
        }
    }
}
