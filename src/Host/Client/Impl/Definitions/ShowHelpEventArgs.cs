using System;

namespace Microsoft.R.Host.Client {
    public class ShowHelpEventArgs : EventArgs {
        public string Url { get; private set; }
        public ShowHelpEventArgs(string url) {
            Url = url;
        }
    }
}
