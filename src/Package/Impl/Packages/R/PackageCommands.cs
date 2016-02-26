using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Debugger.Commands;
using Microsoft.VisualStudio.R.Package.Documentation;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Data;
using Microsoft.VisualStudio.R.Package.Repl.Debugger;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.RPackages.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class PackageCommands {
        public static IEnumerable<MenuCommand> GetCommands(ExportProvider exportProvider) {
            var rSessionProvider = exportProvider.GetExportedValue<IRSessionProvider>();
            var projectServiceAccessor = exportProvider.GetExportedValue<IProjectServiceAccessor>();
            var plotHistoryProvider = exportProvider.GetExportedValue<IPlotHistoryProvider>();
            var plotHistory = plotHistoryProvider.GetPlotHistory(rSessionProvider.GetInteractiveWindowRSession());
            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(IVsDebugger));
            var textViewTracker = exportProvider.GetExportedValue<IActiveWpfTextViewTracker>();

            return new List<MenuCommand> {
                new GoToOptionsCommand(),
                new GoToEditorOptionsCommand(),
                new ImportRSettingsCommand(),

                new SendSmileCommand(),
                new SendFrownCommand(),

                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRtvsDocumentation, DocumentationUrls.RtvsDocumentation),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRtvsSamples, DocumentationUrls.RtvsSamples),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsIntroToR, DocumentationUrls.CranIntro),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsTaskViews, DocumentationUrls.CranViews),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsDataImportExport, DocumentationUrls.CranData),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsWritingRExtensions, DocumentationUrls.CranExtensions),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCheckForUpdates, DocumentationUrls.CheckForRtvsUpdates),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdMicrosoftRProducts, DocumentationUrls.MicrosoftRProducts),

                new LoadWorkspaceCommand(rSessionProvider, projectServiceAccessor),
                new SaveWorkspaceCommand(rSessionProvider, projectServiceAccessor),

                new SourceRScriptCommand(VsAppShell.Current.CompositionService),

                new AttachDebuggerCommand(rSessionProvider),
                new AttachToRInteractiveCommand(rSessionProvider),
                new StopDebuggingCommand(rSessionProvider),
                new ContinueDebuggingCommand(rSessionProvider),
                new StepOverCommand(rSessionProvider),
                new StepOutCommand(rSessionProvider),
                new StepIntoCommand(rSessionProvider),

                new InterruptRCommand(rSessionProvider, debugger),
                new ResetReplCommand(),

                new ImportDataSetTextFileCommand(),
                new ImportDataSetUrlCommand(),

                new InstallPackagesCommand(),
                new CheckForPackageUpdatesCommand(),

                // Window commands
                new ShowPlotWindowsCommand(),
                new ShowRInteractiveWindowsCommand(),
                new ShowVariableWindowCommand(),
                new ShowHelpWindowCommand(),
                new ShowHelpOnCurrentCommand(rSessionProvider, textViewTracker),
                new ShowHistoryWindowCommand(),

                // Plot commands
                new ExportPlotAsImageCommand(plotHistory),
                new ExportPlotAsPdfCommand(plotHistory),
                new CopyPlotAsBitmapCommand(plotHistory),
                new CopyPlotAsMetafileCommand(plotHistory),
                new HistoryNextPlotCommand(plotHistory),
                new HistoryPreviousPlotCommand(plotHistory),
                new ClearPlotsCommand(plotHistory),
                new RemovePlotCommand(plotHistory),
            };
        }
    }
}
