// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class MatrixViewHeaderFocus : Control {
        private readonly MatrixView _matrixView;
        private VisualGridScroller Scroller => _matrixView.Scroller;

        public long Column { get; set; }
        public string Value { get; set; }

        public MatrixViewHeaderFocus(MatrixView matrixView) {
            _matrixView = matrixView;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }

        //protected override AutomationPeer OnCreateAutomationPeer()
        //    => ((MatrixViewAutomationPeer)UIElementAutomationPeer.CreatePeerForElement(_matrixView)).GetOrCreateHeader(Column.ReduceToInt());

        protected override void OnKeyDown(KeyEventArgs e) {
            if (HandleKeyDown(e.Key)) {
                e.Handled = true;
            } else {
                base.OnKeyDown(e);
            }
        }

        private bool HandleKeyDown(Key key) {
            switch (key) {
                case Key.Right:
                    Scroller?.EnqueueCommand(GridUpdateType.HeaderFocusRight, 1L);
                    return true;
                case Key.Left:
                    Scroller?.EnqueueCommand(GridUpdateType.HeaderFocusLeft, 1L);
                    return true;
                default:
                    return false;
            }
        }
    }
}