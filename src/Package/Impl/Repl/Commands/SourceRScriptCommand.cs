// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SourceRScriptCommand : PackageCommand {
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly bool _echo;

        private static readonly string[] _contentTypes = new string[] {
            RContentTypeDefinition.ContentType,
            MdProjectionContentTypeDefinition.ContentType
        };

        public SourceRScriptCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker, bool echo)
            : base(RGuidList.RCmdSetGuid, echo ? RPackageCommandId.icmdSourceRScriptWithEcho : RPackageCommandId.icmdSourceRScript) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
            _echo = echo;
        }

        private ITextView GetActiveTextView() {
            foreach (var c in _contentTypes) {
                var tv = _activeTextViewTracker.GetLastActiveTextView(c);
                if (tv != null) {
                    return tv;
                }
            }
            return null;
        }

        private string GetFilePath() {
            ITextView textView = GetActiveTextView();
            return textView?.GetFilePath();
        }

        protected override void SetStatus() {
            Visible = _interactiveWorkflow.ActiveWindow != null && _interactiveWorkflow.ActiveWindow.Container.IsOnScreen;
            ITextView textView = GetActiveTextView();
            var contentType = textView?.TextBuffer?.ContentType?.TypeName;
            Enabled = contentType != null &&
                      (contentType.EqualsIgnoreCase(RContentTypeDefinition.ContentType) ||
                      contentType.EqualsIgnoreCase(MdProjectionContentTypeDefinition.ContentType));
        }

        protected override void Handle() {
            ITextView textView = GetActiveTextView();
            var contentType = textView?.TextBuffer?.ContentType?.TypeName;
            if (contentType != null) {
                if (contentType.EqualsIgnoreCase(RContentTypeDefinition.ContentType)) {
                    HandleR();
                } else if (contentType.EqualsIgnoreCase(MdProjectionContentTypeDefinition.ContentType)) {
                    HandleMarkdown();
                }
            }
        }

        private void HandleR() {
            string filePath = GetFilePath();
            if (filePath != null) {
                // Save file before sourcing
                ITextView textView = GetActiveTextView();
                if (textView != null) {
                    textView.SaveFile();
                    _interactiveWorkflow.Operations.SourceFile(filePath, _echo);
                }
            }
        }

        private void HandleMarkdown() {
            ITextView textView = GetActiveTextView();
            if (textView != null) {
                var document = REditorDocument.FindInProjectedBuffers(textView.TextBuffer);
                if (document != null) {
                    var filePath = textView.TextBuffer.GetTextDocument()?.FilePath;
                    if (filePath != null) {
                        filePath += ".r";
                        filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath));
                        try {
                            string rContent = FilterR(document.TextBuffer.CurrentSnapshot.GetText());
                            using (var sw = new StreamWriter(filePath)) {
                                sw.Write(rContent);
                            }
                            _interactiveWorkflow.Operations.SourceFile(filePath, _echo);
                        } catch (IOException) { } catch (AccessViolationException) { }
                    }
                }
            }
        }

        private string FilterR(string content) {
            // Change decoration like '{r, x = FALSE, ...} into
            // legal R like 'x = FALSE; y = 1.0;  Allow brace nesting.
            while (true) {
                var start = content.IndexOfIgnoreCase("{r");
                if (start < 0) {
                    break;
                }

                var bc = new BraceCounter<char>('{', '}');
                var end = start;
                bc.CountBrace(content[end]);
                while (bc.Count > 0 && end < content.Length) {
                    end++;
                    bc.CountBrace(content[end]);
                }
                if (end < content.Length && end > start) {
                    content = content.Replace("{r", string.Empty, start, 2);
                    end -= 2;
                    content = content.Replace("}", string.Empty, end, 1);
                    content = content.Replace(",", ";", start, end - start);
                }
            }
            return content;
        }
    }
}
