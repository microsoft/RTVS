// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Help;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    class RHostClientHelpTestApp : RHostClientTestApp {
        private IHelpVisualComponent _component;

        public IHelpVisualComponent Component {
            get { return _component; }
            set {
                _component = value;
                _component.Browser.Navigating += Browser_Navigating;
                _component.Browser.Navigated += Browser_Navigated;
            }
        }

        public ManualResetEventSlim Ready { get; }
        public Uri Uri { get; private set; }

        public RHostClientHelpTestApp() {
            Ready = new ManualResetEventSlim();
        }

        public void Reset() {
            Ready.Reset();
            Uri = null;
        }

        public override async Task ShowHelpAsync(string url, CancellationToken cancellationToken) {
            Reset();
            await UIThreadHelper.Instance.MainThread.SwitchToAsync(cancellationToken);
            Component.Navigate(url);
        }

        public async Task WaitForReadyAndRenderedAsync(Action<int> idleAction, string testName) {
            var start = DateTime.Now;
            while (Uri == null) {
                await UIThreadHelper.Instance.InvokeAsync(() => idleAction(200));
                Ready.Wait(500);
                await UIThreadHelper.Instance.InvokeAsync(() => idleAction(200));

                if ((DateTime.Now - start).TotalMilliseconds > 10000) {
                    throw new TimeoutException(nameof(testName));
                }
            }
        }

        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Browser.DocumentCompleted += OnDocumentCompleted;
                Uri = _component.Browser.Url;
            }).DoNotWait();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            Ready.Reset();
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Browser.DocumentCompleted -= OnDocumentCompleted;
                Ready.Set();
            }).DoNotWait();
        }
    }
}
