// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.R.Components.Help;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    class RHostClientHelpTestApp : RHostClientTestApp {
        private IHelpVisualComponent _component;
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public IHelpVisualComponent Component {
            get { return _component; }
            set {
                _component = value;
                _component.Browser.Navigating += Browser_Navigating;
                _component.Browser.Navigated += Browser_Navigated;
            }
        }

        public Task<bool> Ready => _tcs.Task;
        public Uri Uri { get; private set; }

        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            UIThreadHelper.Instance.Invoke(() => {
                Component.Browser.DocumentCompleted += OnDocumentCompleted;
                Uri = _component.Browser.Url;
            });
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            ResetReadyState();
        }

        public void ResetReadyState() {
            _tcs = new TaskCompletionSource<bool>();
        }

        private void SetReadyState() {
            _tcs.TrySetResult(true);
        }

        public override Task ShowHelpAsync(string url, CancellationToken cancellationToken) {
            ResetReadyState();
            return UIThreadHelper.Instance.InvokeAsync(() => Component.Navigate(url), cancellationToken);
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Browser.DocumentCompleted -= OnDocumentCompleted;
                SetReadyState();
            }).DoNotWait();
        }
    }
}
