// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
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
        private readonly ICoreShell _shell;

        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IInteractiveWindow InteractiveWindow { get; }

        public bool IsRunning => InteractiveWindow.IsRunning;
        public IWpfTextView TextView => InteractiveWindow.TextView;
        public ITextBuffer CurrentLanguageBuffer => InteractiveWindow.CurrentLanguageBuffer;

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public RInteractiveWindowVisualComponent(IInteractiveWindow interactiveWindow, IVisualComponentContainer<IInteractiveWindowVisualComponent> container, IRSessionProvider sessionProvider, ICoreShell shell) {
            InteractiveWindow = interactiveWindow;
            Container = container;

            _sessionProvider = sessionProvider;
            _shell = shell;
            sessionProvider.BrokerStateChanged += OnBrokerChanged;

            var textView = interactiveWindow.TextView;
            Controller = ServiceManagerBase.GetService<ICommandTarget>(textView);
            Control = textView.VisualElement;
            interactiveWindow.Properties.AddProperty(typeof(IInteractiveWindowVisualComponent), this);

            UpdateWindowTitle(_sessionProvider.IsConnected);
        }

        public void Dispose() {
            InteractiveWindow.Properties.RemoveProperty(typeof (IInteractiveWindowVisualComponent));
            _sessionProvider.BrokerStateChanged -= OnBrokerChanged;
        }

        private void OnBrokerChanged(object sender, BrokerStateChangedEventArgs e) {
            _shell.DispatchOnUIThread(() => UpdateWindowTitle(e.IsConnected));
        }

        private void UpdateWindowTitle(bool isConnected) {
            if (isConnected) {
                var broker = _sessionProvider.Broker;
                string text;
                if(broker.IsRemote) {
                    var verified = broker.IsVerified ? Resources.SecureConnection : Resources.UntrustedConnection;
                    var machineUrl = broker.Uri.ToString().TrimTrailingSlash();
                    text = Invariant($"{Resources.ReplWindowName} - {broker.Name} ({machineUrl}) : {verified}");
                } else {
                    text = Invariant($"{Resources.ReplWindowName} - {broker.Name}");
                }
                Container.CaptionText = text;
            } else {
                Container.CaptionText = Invariant($"{Resources.ReplWindowName} - {Resources.ConnectionManager_Disconnected}");
            }
        }
    }
}