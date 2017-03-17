// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal abstract class PreviewCommand : ViewCommand {
        private Task _lastCommandTask;
        private string _outputFilePath;
        private readonly Dictionary<MarkdownFlavor, IMarkdownFlavorPublishHandler> _flavorHandlers = new Dictionary<MarkdownFlavor, IMarkdownFlavorPublishHandler>();
        protected readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IProcessServices _pss;
        private readonly IFileSystem _fs;

        protected ICoreShell Shell { get; }

        public PreviewCommand(ITextView textView, int id,
            IRInteractiveWorkflowProvider workflowProvider,
            ICoreShell shell,
            IProcessServices pss,
            IFileSystem fs)
            : base(textView, new CommandId[] { new CommandId(MdPackageCommandId.MdCmdSetGuid, id) }, false) {
            _workflowProvider = workflowProvider;
            Shell = shell;
            _pss = pss;
            _fs = fs;

            IEnumerable<Lazy<IMarkdownFlavorPublishHandler>> handlers = shell.ExportProvider.GetExports<IMarkdownFlavorPublishHandler>();
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

            if (_lastCommandTask == null || _lastCommandTask.IsCompleted) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TaskAvailable()) {
                return CommandResult.Disabled;
            }

            IMarkdownFlavorPublishHandler flavorHandler = GetFlavorHandler(TextView.TextBuffer);
            if (flavorHandler == null) {
                return CommandResult.Disabled;
            }

            var workflow = _workflowProvider.GetOrCreate();
            _lastCommandTask = Task.Run(async () => {
                // Get list of installed packages and verify that all the required ones are installed
                var packages = await workflow.Packages.GetInstalledPackagesAsync();
                if (packages.Any(p => p.Package.EqualsIgnoreCase(flavorHandler.RequiredPackageName))) {
                    // Text buffer operations should be performed in UI thread
                    await Shell.SwitchToMainThreadAsync();
                    if (await CheckPrerequisitesAsync()) {
                        var textBuffer = SaveFile();
                        if (textBuffer != null) {
                            var inputFilePath = textBuffer.GetFilePath();
                            _outputFilePath = Path.ChangeExtension(inputFilePath, FileExtension);

                            try {
                                _fs.DeleteFile(_outputFilePath);
                            } catch (IOException ex) {
                                Shell.ShowErrorMessage(ex.Message);
                                return;
                            }

                            var session = workflow.RSession;
                            await flavorHandler.PublishAsync(session, Shell, _fs, inputFilePath, _outputFilePath, Format, textBuffer.GetEncoding()).ContinueWith(t => LaunchViewer());
                        }
                    }
                } else {
                    await Shell.ShowErrorMessageAsync(Resources.Error_PackageMissing.FormatInvariant(flavorHandler.RequiredPackageName));
                }
            });

            return CommandResult.Executed;
        }

        private ITextBuffer SaveFile() {
            var document = EditorExtensions.FindInProjectedBuffers<MdEditorDocument>(TextView.TextBuffer, MdContentTypeDefinition.ContentType);
            var tb = document.TextBuffer;
            if (!tb.CanBeSavedInCurrentEncoding()) {
                if (MessageButtons.No == Shell.ShowMessage(Resources.Warning_SaveInUtf8, MessageButtons.YesNo)) {
                    return null;
                }
                tb.Save(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            } else {
                tb.Save();
            }
            return tb;
        }

        protected virtual async Task<bool> CheckPrerequisitesAsync() {
            if (!await CheckExistsOnPathAsync("pandoc.exe")) {
                var session = _workflowProvider.GetOrCreate().RSession;
                var message = session.IsRemote ? Resources.Error_PandocMissingRemote : Resources.Error_PandocMissingLocal;
                await Shell.ShowErrorMessageAsync(message);
                _pss.Start("http://pandoc.org/installing.html");
                return false;
            }
            return true;
        }

        private void LaunchViewer() {
            if (!string.IsNullOrEmpty(_outputFilePath)) {
                if (_fs.FileExists(_outputFilePath)) {
                    LaunchViewer(_outputFilePath);
                }
            }
            _lastCommandTask = null;
        }

        protected virtual void LaunchViewer(string fileName) {
            _pss.Start(_outputFilePath);
        }

        private bool TaskAvailable() {
            if (_lastCommandTask == null) {
                return true;
            }

            switch (_lastCommandTask.Status) {
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

        protected Task<bool> CheckExistsOnPathAsync(string fileName) {
            var session = _workflowProvider.GetOrCreate().RSession;
            return session.EvaluateAsync<bool>(Invariant($"rtvs:::exists_on_path('{fileName}')"), REvaluationKind.Normal);
        }
    }
}
