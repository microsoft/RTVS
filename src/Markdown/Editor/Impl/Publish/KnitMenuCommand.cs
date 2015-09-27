using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Markdown.Editor.Document;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands
{
    internal class KnitMenuCommand : ViewCommand
    {
        public KnitMenuCommand(ITextView textView)
            : base(textView, new CommandId[]
                {
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitHtml),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitPdf),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitWord),
                }, false)
        {
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            // Save the file
            // Check if KnitR is installed
            // Run RScript
            //string rBinariesFolder = RToolsSettings.GetBinariesFolder();
            //string rScriptPath = Path.Combine(rBinariesFolder, @"rscript.exe");

            //IEditorDocument document = MdEditorDocument.FromTextBuffer(TextView.TextBuffer);
            //string inputFilePath = document.WorkspaceItem.Path;
            //string outputFilePath = Path.ChangeExtension(inputFilePath, "html");

            //inputFilePath = inputFilePath.Replace('\\', '/');
            //outputFilePath = outputFilePath.Replace('\\', '/');

            //ProcessStartInfo psi = new ProcessStartInfo();

            //psi.FileName = rScriptPath;
            //psi.WorkingDirectory = rBinariesFolder;
            //psi.Arguments = string.Format(CultureInfo.InvariantCulture,
            //    "-e \'rmarkdown::render(\"{0}\", NULL, output_file=\"{1}\")\'", inputFilePath, outputFilePath);
            //try
            //{
            //    using (Process p = Process.Start(psi))
            //    {
            //        p.WaitForExit(5000);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    string message = string.Format(CultureInfo.InvariantCulture, Resources.Error_RScriptLaunchFailed, ex.Message);
            //    EditorShell.Current.ShowErrorMessage(message);
            //}

            //if(File.Exists(outputFilePath))
            //{
            //    Process.Start(outputFilePath);
            //}

            return CommandResult.Executed;
        }
    }
}
