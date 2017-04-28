// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace Microsoft.Languages.Editor.Outline {
    public class OutlineController : ICommandTarget {
        private readonly IOutliningManagerService _outliningManagerService;
        private readonly ITextView _textView;

        public OutlineController(ITextView textView, IOutliningManagerService outliningManagerService) {
            _textView = textView;
            _outliningManagerService = outliningManagerService;
        }

        private IOutliningManager OutliningManager {
            get { return _outliningManagerService.GetOutliningManager(_textView); }
        }

        private void ExpandAll() {
            ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;
            SnapshotSpan snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OutliningManager.ExpandAll(snapshotSpan, (collapsible => true));
        }

        private void CollapseAll() {
            ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;
            SnapshotSpan snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OutliningManager.CollapseAll(snapshotSpan, (collapsible => true));
        }

        private void StopOutlining() {
            OutliningManager.Enabled = false;
        }

        private void StartOutlining() {
            OutliningManager.Enabled = true;
        }

        private void ToggleAll() {
            if (AnyExpandableOutliningRegions) {
                ExpandAll();
            } else {
                CollapseAll();
            }
        }

        private void ToggleCurrent() {
            var caretPosition = _textView.Caret.Position.BufferPosition;
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var span = new SnapshotSpan(snapshot, new Span(caretPosition, 0));

            var regions = OutliningManager.GetAllRegions(span);

            // Find innermost one
            ICollapsible region = null;

            int regionStart = 0;
            int regionEnd = snapshot.Length;

            foreach (ICollapsible c in regions) {
                int start = c.Extent.GetStartPoint(snapshot);
                int end = c.Extent.GetEndPoint(snapshot);

                if (start >= regionStart && end < regionEnd) {
                    regionStart = start;
                    regionEnd = end;

                    region = c;
                }
            }

            if (region != null) {
                if (region.IsCollapsed) {
                    OutliningManager.Expand(region as ICollapsed);
                } else {
                    OutliningManager.TryCollapse(region);
                }
            }
        }

        private bool AnyExpandableOutliningRegions {
            get {
                // Return whether there are any collapsed regions
                if (OutliningManager.Enabled) {
                    ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;
                    SnapshotSpan snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
                    return OutliningManager.GetCollapsedRegions(snapshotSpan).Any();
                }
                return false;
            }
        }


        #region ICommandTarget
        public CommandStatus Status(Guid group, int id) {
            if (OutliningManager != null) {
                if (group == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)id) {
                        case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                            return OutliningManager.Enabled ? CommandStatus.Supported : CommandStatus.SupportedAndEnabled;

                        case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL:
                            return OutliningManager.Enabled ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;

                        case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT:
                            return OutliningManager.Enabled ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;

                        case VSConstants.VSStd2KCmdID.OUTLN_COLLAPSE_TO_DEF:
                            return CommandStatus.Invisible;
                    }
                }
            }

            return CommandStatus.NotSupported;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                switch ((VSConstants.VSStd2KCmdID)id) {
                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        StartOutlining();
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        StopOutlining();
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL:
                        ToggleAll();
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT:
                        ToggleCurrent();
                        break;

                    default:
                        return CommandResult.NotSupported;
                }

                return CommandResult.Executed;
            }

            return CommandResult.NotSupported;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }
        #endregion
    }
}
