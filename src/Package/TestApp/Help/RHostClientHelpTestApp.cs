// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.R.Components.Help;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    class RHostClientHelpTestApp : RHostClientTestApp {
        IHelpVisualComponent _component;
        public IHelpVisualComponent Component {
            get { return _component; }
            set {
                _component = value;
                _component.Browser.Navigated += Browser_Navigated;
            }
        }
        public bool Ready { get; set; }
        public Uri Uri { get; private set; }
        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            Ready = true;
            Uri = _component.Browser.Url;
        }

        public override Task ShowHelpAsync(string url, CancellationToken cancellationToken) {
            return UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Navigate(url);
            }, cancellationToken);
        }
    }
}
