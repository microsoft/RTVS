using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.History {
    internal sealed class ShowHistoryWindowCommand : ShowToolWindowCommand<HistoryWindowPane> {
        public ShowHistoryWindowCommand() 
            : base(RPackageCommandId.icmdShowHistoryWindow) {}
    }
}
