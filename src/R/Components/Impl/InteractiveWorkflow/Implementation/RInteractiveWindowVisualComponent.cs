// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        public RInteractiveWindowVisualComponent(IInteractiveWindow interactiveWindow, IVisualComponentContainer<IInteractiveWindowVisualComponent> container) {
            InteractiveWindow = interactiveWindow;
            Container = container;

            var textView = interactiveWindow.TextView;
            Controller = ServiceManagerBase.GetService<ICommandTarget>(textView);
            Control = textView.VisualElement;
            interactiveWindow.Properties.AddProperty(typeof(IInteractiveWindowVisualComponent), this);
        }

        public void Dispose() {
            InteractiveWindow.Properties.RemoveProperty(typeof (IInteractiveWindowVisualComponent));
        }
    }
}