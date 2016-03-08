// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.TaskList.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    class VsTaskItem : ErrorTask {
        private IEditorTaskListItem _item;
        private IEditorTaskListItemSource _source;

        public VsTaskItem(IEditorTaskListItem item, IEditorTaskListItemSource source) {
            _item = item;
            _source = source;

            Update();
        }

        public bool Update() {
            int oldLine = base.Line;
            int oldColumn = base.Column;
            string oldText = base.Text;
            string oldDocument = base.Document;
            TaskErrorCategory oldErrorCategory = base.ErrorCategory;
            IVsHierarchy oldHierarchy = base.HierarchyItem;

            // VS wants 0-based line and column, but IWebTaskListItemSource utilize 1-based
            // line and column for WebMatrix compatability. So we need to subtract 1 here.
            base.Line = _item.Line - 1;
            base.Column = _item.Column - 1;
            base.Text = _item.Description;
            base.Document = _item.FileName;

            switch (_item.TaskType) {
                case TaskType.Warning:
                    base.ErrorCategory = TaskErrorCategory.Warning;
                    break;

                case TaskType.Informational:
                    base.ErrorCategory = TaskErrorCategory.Message;
                    break;

                default:
                    base.ErrorCategory = TaskErrorCategory.Error;
                    break;
            }

            IVsHierarchy hierarchy = _source.TextBuffer.GetHierarchy();
            base.HierarchyItem = hierarchy;

            return oldLine != base.Line ||
                   oldColumn != base.Column ||
                   oldText != base.Text ||
                   oldDocument != base.Document ||
                   oldErrorCategory != base.ErrorCategory ||
                   oldHierarchy != base.HierarchyItem;
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
            if (_source.TextBuffer != null) {
                var textManager = VsAppShell.Current.GetGlobalService<IVsTextManager>(typeof(SVsTextManager));
                var textLines = _source.TextBuffer.GetBufferAdapter<IVsTextLines>();

                if (_item.Line > 0 && _item.Column > 0) {
                    var snapshot = _source.TextBuffer.CurrentSnapshot;

                    try {
                        int start = snapshot.GetLineFromLineNumber(_item.Line - 1).Start + _item.Column - 1;
                        int end = start + _item.Length;

                        var endLine = snapshot.GetLineFromPosition(end);

                        textManager.NavigateToLineAndColumn(textLines, VSConstants.LOGVIEWID_TextView,
                            _item.Line - 1, _item.Column - 1,
                            endLine.LineNumber, end - endLine.Start);
                    } catch (Exception) { }
                }
            }
        }
    }
}
