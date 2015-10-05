using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Actions.Logging;
using Microsoft.R.Actions.Script;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    internal abstract class PreviewCommandBase : ViewCommand
    {
        private RCommand _lastCommand;
        private string _outputFilePath;

        public PreviewCommandBase(ITextView textView)
            : base(textView, new CommandId[]
                {
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdPreviewHtml),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdPreviewPdf),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdPreviewWord),
                }, false)
        {
        }

        protected abstract string RequiredPackageName { get; }
        protected abstract IEditorDocument GetDocument(ITextBuffer textBuffer);
        protected abstract string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat);

        public override CommandStatus Status(Guid group, int id)
        {
            if (_lastCommand == null || _lastCommand.Task.IsCompleted)
            {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (!TaskAvailable())
            {
                return CommandResult.Disabled;
            }

            if (!InstallPackages.IsInstalled(RequiredPackageName, 5000))
            {
                EditorShell.Current.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_PackageMissing, RequiredPackageName));
                return CommandResult.Disabled;
            }

            // Save the file
            ITextDocument textDocument;
            if (TextView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument))
            {
                if (textDocument.IsDirty)
                {
                    textDocument.Save();
                }
            }

            IEditorDocument document = GetDocument(TextView.TextBuffer);
            string inputFilePath = document.WorkspaceItem.Path;
            _outputFilePath = Path.ChangeExtension(inputFilePath, "html");

            try
            {
                File.Delete(_outputFilePath);
            }
            catch (IOException ex)
            {
                PublishLog.Current.WriteFormatAsync(MessageCategory.Error, Resources.Error_CannotDeleteFile, _outputFilePath, ex.Message);
                return CommandResult.Executed;
            }

            inputFilePath = inputFilePath.Replace('\\', '/');
            string outputFilePath = _outputFilePath.Replace('\\', '/');
            PublishFormat format = GetPublishFormat(id);

            string arguments = GetCommandLine(inputFilePath, outputFilePath, PublishFormat.Html);

            _lastCommand = RCommand.ExecuteRExpressionAsync(arguments, PublishLog.Current);
            _lastCommand.Task.ContinueWith((Task t) => LaunchViewer(t));

            return CommandResult.Executed;
        }

        private void LaunchViewer(Task task)
        {
            if (!string.IsNullOrEmpty(_outputFilePath))
            {
                if (File.Exists(_outputFilePath))
                {
                    Process.Start(_outputFilePath);
                }
                else
                {
                    PublishLog.Current.WriteLineAsync(MessageCategory.Error, Resources.Error_MarkdownConversionFailed);
                }
            }
        }

        private bool TaskAvailable()
        {
            if (_lastCommand == null)
            {
                return true;
            }

            switch (_lastCommand.Task.Status)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                case TaskStatus.RanToCompletion:
                    return true;
            }

            return false;
        }

        private PublishFormat GetPublishFormat(int id)
        {
            switch (id)
            {
                case MdPackageCommandId.icmdPreviewPdf:
                    return PublishFormat.Pdf;

                case MdPackageCommandId.icmdPreviewWord:
                    return PublishFormat.Word;
            }
            return PublishFormat.Html;
        }
    }
}
