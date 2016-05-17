// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Services;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public class RInteractiveWindowVisualComponent : IInteractiveWindowVisualComponent {
        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IInteractiveWindow InteractiveWindow { get; }

        public bool IsRunning => InteractiveWindow.IsRunning;
        public IWpfTextView TextView => InteractiveWindow.TextView;
        public ITextBuffer CurrentLanguageBuffer => InteractiveWindow.CurrentLanguageBuffer;

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public int TerminalWidth { get; private set; } = 80;
        public event EventHandler<TerminalWidthChangedEventArgs> TerminalWidthChanged;

        public RInteractiveWindowVisualComponent(IInteractiveWindow interactiveWindow, IVisualComponentContainer<IInteractiveWindowVisualComponent> container) {
            InteractiveWindow = interactiveWindow;
            Container = container;

            var textView = interactiveWindow.TextView;
            Controller = ServiceManagerBase.GetService<ICommandTarget>(textView);
            Control = textView.VisualElement;
            interactiveWindow.Properties.AddProperty(typeof(IInteractiveWindowVisualComponent), this);
            interactiveWindow.TextView.VisualElement.SizeChanged += VisualElement_SizeChanged;
        }

        private void VisualElement_SizeChanged(object sender, SizeChangedEventArgs e) {
            int width = (int)(TextView.VisualElement.ActualWidth / TextView.FormattedLineSource.ColumnWidth);
            // From R docs:  Valid values are 10...10000 with default normally 80.
            TerminalWidth = Math.Max(10, Math.Min(10000, width));
            TerminalWidthChanged?.Invoke(this, new TerminalWidthChangedEventArgs(TerminalWidth));
        }

        public void Dispose() {
            InteractiveWindow.TextView.VisualElement.SizeChanged -= VisualElement_SizeChanged;
            InteractiveWindow.Properties.RemoveProperty(typeof (IInteractiveWindowVisualComponent));
        }
    }
}