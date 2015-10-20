using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools
{
    internal sealed class CranMirrorTypeConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StandardValuesCollection coll = new StandardValuesCollection(CranMirrorList.MirrorNames);
            return coll;
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value as string;
        }
    }
}
