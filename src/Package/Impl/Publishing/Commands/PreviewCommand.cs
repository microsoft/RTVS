// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.Document;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Actions.Logging;
using Microsoft.R.Actions.Script;
using Microsoft.R.Components.Controller;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal abstract class PreviewCommand : ViewCommand {
        private RCommand _lastCommand;
        private string _outputFilePath;
        private Dictionary<MarkdownFlavor, IMarkdownFlavorPublishHandler> _flavorHandlers = new Dictionary<MarkdownFlavor, IMarkdownFlavorPublishHandler>();

        public PreviewCommand(ITextView textView, int id)
            : base(textView, new CommandId[] { new CommandId(MdPackageCommandId.MdCmdSetGuid, id) }, false) {
            IEnumerable<Lazy<IMarkdownFlavorPublishHandler>> handlers = VsAppShell.Current.ExportProvider.GetExports<IMarkdownFlavorPublishHandler>();
            foreach (var h in handlers) {
                _flavorHandlers[h.Value.Flavor] = h.Value;
            }
        }

        protected abstract string FileExtension { get; }

        protected abstract PublishFormat Format { get; }

        public override CommandStatus Status(Guid group, int id) {
            if (!IsFormatSupported()) {
                return CommandStatus.Invisible;
            }

            if (_lastCommand == null || _lastCommand.Task.IsCompleted) {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TaskAvailable()) {
                return CommandResult.Disabled;
            }

            IMarkdownFlavorPublishHandler flavorHandler = GetFlavorHandler(TextView.TextBuffer);
            if (flavorHandler != null) {
                if (!InstallPackages.IsInstalled(flavorHandler.RequiredPackageName, 5000, RToolsSettings.Current.RBasePath)) {
                    VsAppShell.Current.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_PackageMissing, flavorHandler.RequiredPackageName));
                    return CommandResult.Disabled;
                }

                // Save the file
                ITextDocument textDocument;
                if (TextView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
                    if (textDocument.IsDirty) {
                        textDocument.Save();
                    }
                }

                IEditorDocument document = MdEditorDocument.FromTextBuffer(TextView.TextBuffer);
                string inputFilePath = document.WorkspaceItem.Path;
                var buffer = new StringBuilder(NativeMethods.MAX_PATH);
                NativeMethods.GetShortPathName(inputFilePath, buffer, NativeMethods.MAX_PATH);

                inputFilePath = buffer.ToString();
                _outputFilePath = Path.ChangeExtension(inputFilePath, FileExtension);

                try {
                    File.Delete(_outputFilePath);
                } catch (IOException ex) {
                    PublishLog.Current.WriteFormatAsync(MessageCategory.Error, Resources.Error_CannotDeleteFile, _outputFilePath, ex.Message);
                    return CommandResult.Executed;
                }

                inputFilePath = inputFilePath.Replace('\\', '/');
                string outputFilePath = _outputFilePath.Replace('\\', '/');

                string arguments = flavorHandler.GetCommandLine(inputFilePath, outputFilePath, Format);

                _lastCommand = RCommand.ExecuteRExpressionAsync(arguments, PublishLog.Current, RToolsSettings.Current.RBasePath);
                _lastCommand.Task.ContinueWith((Task t) => LaunchViewer(t));
            }
            return CommandResult.Executed;
        }

        private void LaunchViewer(Task task) {
            if (!string.IsNullOrEmpty(_outputFilePath)) {
                if (File.Exists(_outputFilePath)) {
                    Process.Start(_outputFilePath);
                } else {
                    PublishLog.Current.WriteLineAsync(MessageCategory.Error, Resources.Error_MarkdownConversionFailed);
                }
            }
        }

        private bool TaskAvailable() {
            if (_lastCommand == null) {
                return true;
            }

            switch (_lastCommand.Task.Status) {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                case TaskStatus.RanToCompletion:
                    return true;
            }

            return false;
        }

        private IMarkdownFlavorPublishHandler GetFlavorHandler(ITextBuffer textBuffer) {
            MarkdownFlavor flavor = MdFlavor.FromTextBuffer(textBuffer);
            IMarkdownFlavorPublishHandler value = null;

            if (_flavorHandlers.TryGetValue(flavor, out value)) {
                return value;
            }

            return null; // new MdPublishHandler();
        }

        private bool IsFormatSupported() {
            IMarkdownFlavorPublishHandler flavorHandler = GetFlavorHandler(TextView.TextBuffer);
            if (flavorHandler != null) {
                if (flavorHandler.FormatSupported(Format)) {
                    return true;
                }
            }

            return false;
        }
    }
}
