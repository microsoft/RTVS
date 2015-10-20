using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.R.Support.Utility;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools
{
    internal sealed class RVersionTypeConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> values = new List<string>();

            values.Add(Resources.Settings_RVersion_Latest);
            values.AddRange(RInstallation.GetInstalledEngineVersionsFromRegistry());

            StandardValuesCollection coll = new StandardValuesCollection(values);
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
