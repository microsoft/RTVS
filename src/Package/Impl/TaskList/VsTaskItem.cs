// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    internal sealed class VsTaskItem : ErrorTask {
        private readonly IEditorTaskListItem _item;
        private readonly IEditorTaskListItemSource _source;
        private readonly IServiceContainer _services;

        public VsTaskItem(IEditorTaskListItem item, IEditorTaskListItemSource source, IServiceContainer services) {
            _item = item;
            _source = source;
            _services = services;

            Update();
        }

        public bool Update() {
            var oldLine = base.Line;
            var oldColumn = base.Column;
            var oldText = base.Text;
            var oldDocument = base.Document;
            var oldErrorCategory = base.ErrorCategory;
            var oldHierarchy = base.HierarchyItem;

            // VS wants 0-based line and column, but IWebTaskListItemSource utilize 1-based
            // line and column for WebMatrix compatability. So we need to subtract 1 here.
            Line = _item.Line - 1;
            Column = _item.Column - 1;
            Text = _item.Description;
            Document = _item.FileName;

            switch (_item.TaskType) {
                case TaskType.Warning:
                    ErrorCategory = TaskErrorCategory.Warning;
                    break;

                case TaskType.Informational:
                    ErrorCategory = TaskErrorCategory.Message;
                    break;

                default:
                    ErrorCategory = TaskErrorCategory.Error;
                    break;
            }

            var textBuffer = _source.EditorBuffer as ITextBuffer;
            HierarchyItem = textBuffer?.GetHierarchy();

            return oldLine != Line ||
                   oldColumn != Column ||
                   oldText != Text ||
                   oldDocument != Document ||
                   oldErrorCategory != ErrorCategory ||
                   oldHierarchy != HierarchyItem;
        }

        public int HasHelp(out int pfHasHelp) {
            pfHasHelp = (String.IsNullOrEmpty(_item.HelpKeyword) ? 0 : 1);
            return 0;
        }
        protected override void OnHelp(EventArgs e) {
            // Implement help directly as the base Task class tries to use the IHelpService service
            //   which can't be found (at least on the service provider of our owner)
            if (!String.IsNullOrEmpty(_item.HelpKeyword)) {
                //IVsHelp helpService = Globals.GlobalServiceProvider.GetService(typeof(IVsHelp)) as IVsHelp;
                //helpService.DisplayTopicFromF1Keyword(_item.HelpKeyword);
            }
        }

        public int ImageListIndex(out int pIndex) {
            pIndex = (int)_vstaskbitmap.BMP_SQUIGGLE;
            return 0;
        }

        public int IsReadOnly(VSTASKFIELD field, out int pfReadOnly) {
            pfReadOnly = 1; // TRUE
            return 0;
        }

        protected override void OnNavigate(EventArgs e) {
            if (_source.EditorBuffer != null && _item.Line > 0 && _item.Column > 0) {
                var textView = TextViewConnectionListener.GetFirstViewForBuffer(_source.EditorBuffer as ITextBuffer);
                if (textView != null) {
                    var snapshot = textView.TextBuffer.CurrentSnapshot;
                    try {
                        int start = snapshot.GetLineFromLineNumber(_item.Line - 1).Start + _item.Column - 1;
                        int end = start + _item.Length;

                        var endLine = snapshot.GetLineFromPosition(end);

                        var textManager = _services.GetService<IVsTextManager>(typeof(SVsTextManager));
                        var textLines = textView.TextBuffer.GetBufferAdapter<IVsTextLines>(_services);

                        textManager.NavigateToLineAndColumn(textLines, VSConstants.LOGVIEWID_TextView,
                            _item.Line - 1, _item.Column - 1,
                            endLine.LineNumber, end - endLine.Start);
                    } catch (Exception) { }
                }
            }
        }
    }
}
