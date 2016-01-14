using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpHomeCommand : Command {
        public HelpHomeCommand(HelpWindowPane pane) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpHome)) {
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            ShowDefaultHelpPage();
            return CommandResult.Executed;
        }

        public static void ShowDefaultHelpPage() {
            Task.Run(async () => await ShowDefaultHelpPageAsync());
        }

        private static async Task ShowDefaultHelpPageAsync() {
            var rSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            IRSession session = rSessionProvider.Current;
            if (session != null) {
                try {
                    using (IRSessionEvaluation evaluation = await session.BeginEvaluationAsync(isMutating: false)) {
                        await evaluation.EvaluateAsync("help.start()" + Environment.NewLine);
                    }
                } catch (RException) { } catch (OperationCanceledException) { }
            }
        }
    }
}
