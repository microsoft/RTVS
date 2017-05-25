using System;
using System.Windows.Forms;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    public class RPackageSourceViewModel {

        public string Name { get; }

        public string Source { get; set; }

        public bool IsEnabled { get; set; }

        public RPackageSourceViewModel(string source) : this(source, source, isEnabled: true) { }

        public RPackageSourceViewModel(string source, string name) : this(source, name, isEnabled: true) { }

        public RPackageSourceViewModel(string source, string name, bool isEnabled) {
            Check.ArgumentNull(nameof(source), source);
            Check.ArgumentNull(nameof(name), name);

            Name = name;
            Source = source;
            IsEnabled = isEnabled;
        }

        public RPackageSourceViewModel Clone() {
            return new RPackageSourceViewModel(Source, Name, IsEnabled);
        }

        public void CopyToClipboard() {
            Clipboard.Clear();
            Clipboard.SetText(Source);
        }
    }
}
