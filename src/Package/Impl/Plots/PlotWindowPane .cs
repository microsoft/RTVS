using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots
{
    [Guid(WindowGuid)]
    internal class PlotWindowPane : ToolWindowPane
    {
        internal const string WindowGuid = "970AD71C-2B08-4093-8EA9-10840BC726A3";


        public PlotWindowPane()
        {
            Caption = Resources.PlotWindowCaption;

            ContentProvider = new PlotContentProvider();
            Content = new XamlPresenter(ContentProvider);

            InitializePresenter();

            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);

            Controller c = new Controller();
            c.AddCommandSet(GetCommands());

            this.ToolBarCommandTarget = new CommandTargetToOleShim(null, c);
        }

        public IPlotContentProvider ContentProvider { get; }

        private IEnumerable<ICommand> GetCommands()
        {
            List<ICommand> commands = new List<ICommand>();

            commands.Add(new OpenPlotCommand(this));
            commands.Add(new SavePlotCommand(this));
            commands.Add(new ExportPlotCommand(this));
            commands.Add(new FixPlotCommand(this));
            commands.Add(new CopyPlotCommand(this));
            commands.Add(new PrintPlotCommand(this));
            commands.Add(new ZoomInPlotCommand(this));
            commands.Add(new ZoomOutPlotCommand(this));

            return commands;
        }

        private void InitializePresenter()
        {
            ContentProvider.LoadXaml(
                "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                "Foreground=\"DarkGray\" " +
                "TextAlignment=\"Center\" " +
                "VerticalAlignment=\"Center\" " +
                "HorizontalAlignment=\"Center\" " +
                "TextWrapping=\"Wrap\">" +
                Resources.EmptyPlotWindowWatermark +
                "</TextBlock>");
        }

        public void OpenPlot()
        {
            string fileName = GetFileName();
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    ContentProvider.LoadFile(fileName);
                }
                catch(Exception ex)
                {
                    EditorShell.Current.ShowErrorMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.CannotOpenPlotFile, ex.Message));
                }
            }
        }

        // TODO: factor out to utility. Copied code from PTVS, Dialogs.cs
        private string GetFileName()
        {
            return FileUtilities.BrowseForFileOpen(
                IntPtr.Zero,
                Resources.PlotFileFilter,
                // TODO: open in current project folder if one is active
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\",
                Resources.OpenPlotDialogTitle);
        }
    }
}
