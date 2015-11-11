using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.QuickInfo {
    /// <summary>
    /// Interaction logic for QuickInfoWithLink.xaml
    /// </summary>
    public partial class QuickInfoWithLink : UserControl, IInteractiveQuickInfoContent {
        private Action _dismissCallback;
        private Action<object> _clickAction;
        private object _param;

        public QuickInfoWithLink(string info, Action dismissCallback)
            : this(info, null, null, null, dismissCallback) {
        }

        public QuickInfoWithLink(string info, string linkText, Action<object> clickAction, object param, Action dismissCallback) {
            InitializeComponent();

            _dismissCallback = dismissCallback;
            _clickAction = clickAction;
            _param = param;

            QuickInfo.Text = info;

            if (_clickAction != null) {
                QuickInfoLinkBlock.Visibility = Visibility.Visible;
                QuickInfoLink.IsEnabled = true;
                QuickInfoLinkText.Text = linkText;
            } else {
                QuickInfoLinkBlock.Visibility = Visibility.Collapsed;
                QuickInfoLink.IsEnabled = false;
            }
        }

        private void QuickInfoLink_Click(object sender, RoutedEventArgs e) {
            if (_dismissCallback != null) {
                _dismissCallback();
            }

            if (_clickAction != null) {
                _clickAction(_param);
            }
        }

        public bool KeepQuickInfoOpen {
            get {
                return false;
            }
        }

        public bool IsMouseOverAggregated {
            get {
                return IsMouseDirectlyOver;
            }
        }
    }
}
