using Microsoft.Common.Core.Enums;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class HelpBrowserTypeConverter : EnumTypeConverter<HelpBrowserType> {
        public HelpBrowserTypeConverter() : base(Resources.HelpBrowser_Automatic, Resources.HelpBrowser_External) {}
    }
}
