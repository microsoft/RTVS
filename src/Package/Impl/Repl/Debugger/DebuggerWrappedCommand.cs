using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal abstract class DebuggerWrappedCommand: DebuggerCommand {
        private Guid _shellGroup;
        private uint _shellCmdId;

        public DebuggerWrappedCommand(IRSessionProvider rSessionProvider, int cmdId, Guid shellGroup, int shellCmdId, DebuggerCommandVisibility visibility)
            : base(rSessionProvider, cmdId, visibility) {
            _shellGroup = shellGroup;
            _shellCmdId = (uint)shellCmdId;
        }

        internal override void Handle() {
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            object o = null;
            shell.PostExecCommand(ref _shellGroup, _shellCmdId, 0, ref o);
        }
    }
}
