using Microsoft.Common.Core.Enums;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class YesNoTypeConverter : EnumTypeConverter<YesNo> {
        public YesNoTypeConverter() : base(Resources.Yes, Resources.No) {}
    }
}