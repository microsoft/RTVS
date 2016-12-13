using System;
using System.Windows.Forms;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    public class RPackageSourceViewModel {

        public string Name { get; }

        public string Source { get; set; }

        public bool IsEnabled { get; set; }

        public RPackageSourceViewModel(string source) : this(source, source, isEnabled: true) { }

        public RPackageSourceViewModel(string source, string name) : this(source, name, isEnabled: true) { }

        public RPackageSourceViewModel(string source, string name, bool isEnabled) {
            source = source ?? throw new ArgumentNullException(nameof(source));
            name = name ?? throw new ArgumentNullException(nameof(name));

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
