using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal sealed class LocDefaultValueAttribute : DefaultValueAttribute {
        public LocDefaultValueAttribute(string resourceId) : base(string.Empty) {
            SetValue(Resources.ResourceManager.GetString(resourceId));
        }
     }
}
