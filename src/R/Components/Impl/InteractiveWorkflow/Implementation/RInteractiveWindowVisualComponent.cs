// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Services;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public class RInteractiveWindowVisualComponent : IInteractiveWindowVisualComponent {
        private readonly IRSessionProvider _sessionProvider;

        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IInteractiveWindow InteractiveWindow { get; }

        public bool IsRunning => InteractiveWindow.IsRunning;
        public IWpfTextView TextView => InteractiveWindow.TextView;
        public ITextBuffer CurrentLanguageBuffer => InteractiveWindow.CurrentLanguageBuffer;

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public RInteractiveWindowVisualComponent(IInteractiveWindow interactiveWindow, IVisualComponentContainer<IInteractiveWindowVisualComponent> container, IRSessionProvider sessionProvider) {
            InteractiveWindow = interactiveWindow;
            Container = container;

            _sessionProvider = sessionProvider;
            sessionProvider.BrokerChanged += OnBrokerChanged;

            var textView = interactiveWindow.TextView;
            Controller = ServiceManagerBase.GetService<ICommandTarget>(textView);
            Control = textView.VisualElement;
            interactiveWindow.Properties.AddProperty(typeof(IInteractiveWindowVisualComponent), this);

            UpdateWindowTitle();
        }

        public void Dispose() {
            InteractiveWindow.Properties.RemoveProperty(typeof (IInteractiveWindowVisualComponent));
            _sessionProvider.BrokerChanged -= OnBrokerChanged;
        }

        private void OnBrokerChanged(object sender, EventArgs e) {
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle() {
            var broker = _sessionProvider.Broker;
            string title;
            if (broker != null) {
                title = broker.IsRemote
                    ? Invariant($"{Resources.ReplWindowName} - {broker.Name} ({broker.Uri.ToString().TrimTrailingSlash()})")
                    : Invariant($"{Resources.ReplWindowName} - {broker.Name}");
            } else {
                title = Resources.Disconnected;
            }
            Container.CaptionText = title;
        }
    }
}