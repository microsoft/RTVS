using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History.Implementation {
    public class RHistoryWindowVisualComponent : IRHistoryWindowVisualComponent {
        private readonly IVisualComponentContainer<IRHistoryWindowVisualComponent> _container;

        public const string TextViewRole = "TextViewRole";

        public RHistoryWindowVisualComponent(ITextBuffer historyTextBuffer, ITextEditorFactoryService textEditorFactory, IVisualComponentContainer<IRHistoryWindowVisualComponent> container) {
            _container = container;
            TextView = CreateTextView(historyTextBuffer, textEditorFactory);
            Control = TextView.VisualElement;
        }

        /// <summary>
        /// TODO: Remove when commandTarget can be extracted from TextView
        /// </summary>
        public void SetController(ICommandTarget commandTarget) {
            Controller = commandTarget;
        }

        public ICommandTarget Controller { get; set; }
        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container => _container;
        public IWpfTextView TextView { get; }

        public void Dispose() {

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
