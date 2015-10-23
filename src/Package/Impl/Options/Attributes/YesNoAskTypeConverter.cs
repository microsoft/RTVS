using Microsoft.Common.Core.Enums;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class YesNoAskTypeConverter : EnumTypeConverter<YesNoAsk> {
        public YesNoAskTypeConverter() : base(Resources.Yes, Resources.No, Resources.Ask) {}
    }
}
