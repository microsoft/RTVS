using System;

namespace Microsoft.R.Host.Client {
    public class PlotEventArgs : EventArgs {
        public string FilePath { get; private set; }
        public PlotEventArgs(string filePath) {
            FilePath = filePath;
        }
    }
}
