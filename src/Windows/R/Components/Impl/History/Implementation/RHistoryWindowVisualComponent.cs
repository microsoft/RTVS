// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History.Implementation {
    public class RHistoryWindowVisualComponent : IRHistoryWindowVisualComponent {
        private readonly IVisualComponentContainer<IRHistoryWindowVisualComponent> _container;
        private IRHistory _history;

        public const string TextViewRole = "TextViewRole";

        public RHistoryWindowVisualComponent(ITextBuffer historyTextBuffer, IRHistoryProvider historyProvider, ITextEditorFactoryService textEditorFactory, IVisualComponentContainer<IRHistoryWindowVisualComponent> container) {
            _container = container;
            _history = historyProvider.GetAssociatedRHistory(historyTextBuffer);

            TextView = CreateTextView(historyTextBuffer, textEditorFactory);
            TextView.Selection.SelectionChanged += TextViewSelectionChanged;

            Control = textEditorFactory.CreateTextViewHost(TextView, false).HostControl;
        }

        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container => _container;
        public IWpfTextView TextView { get; private set; }

        public void Dispose() {
            if (TextView == null) {
                return;
            }

            TextView.Selection.SelectionChanged -= TextViewSelectionChanged;
            TextView.Close();
            TextView = null;
            _history = null;
        }

        private void TextViewSelectionChanged(object sender, EventArgs e) {
            if (TextView.Selection.Start != TextView.Selection.End) {
                _history.ClearHistoryEntrySelection();
            }
        }

        private static IWpfTextView CreateTextView(ITextBuffer historyTextBuffer, ITextEditorFactoryService textEditorFactory) {
            var textView = textEditorFactory.CreateTextView(historyTextBuffer, textEditorFactory.DefaultRoles.UnionWith(textEditorFactory.CreateTextViewRoleSet(TextViewRole)));
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.VerticalScrollBarId, true);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, true);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.SelectionMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
            textView.Options.SetOptionValue(DefaultWpfViewOptions.EnableMouseWheelZoomId, false);
            textView.Options.SetOptionValue(DefaultWpfViewOptions.EnableHighlightCurrentLineId, false);
            textView.Options.SetOptionValue(DefaultTextViewOptions.AutoScrollId, true);
            textView.Options.SetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId, false);
            textView.Options.SetOptionValue(DefaultTextViewOptions.DragDropEditingId, false);
            textView.Options.SetOptionValue(DefaultTextViewOptions.UseVirtualSpaceId, false);
            textView.Caret.IsHidden = true;
            return textView;
        }
    }
}
