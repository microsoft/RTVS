using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Help;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    class RHostClientHelpTestApp : RHostClientTestApp {
        IHelpWindowVisualComponent _component;
        public IHelpWindowVisualComponent Component {
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

        public override Task ShowHelp(string url) {
            UIThreadHelper.Instance.Invoke(() => {
                Component.Navigate(url);
            });
            return Task.CompletedTask;
        }
    }
}
