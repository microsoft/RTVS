// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class EncodingTypeConverter : TypeConverter {
        private static readonly int[] _supportedCodePages = new int[] {
            708,720, 737, 775,
            850, 852, 855, 857, 860, 861, 862,
            863, 864, 865, 866, 869, 874,
            932, 936, 949, 950,
            1250, 1251, 1252, 1253,1254, 1255, 1256, 1257, 1258,
            20000, 20127, 20866, 20932, 20936, 20949, 21866,
            28591, 28592, 28593, 28594, 28595, 28596, 28597,
            28598, 28599, 28603, 28605,
            38598
        };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var encodings = _supportedCodePages.Select(cp => Encoding.GetEncoding(cp));
            var codePages = encodings.OrderBy(e => e.EncodingName).Select(e => e.CodePage).ToList();
            codePages.Insert(0, 0);
            return new StandardValuesCollection(codePages);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(int);
        }

        /// <summary>
        /// Converts encoding name to code page
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value.GetType() == typeof(string)) {
                return ConvertToCodePage(value as string);
            } else if (value.GetType() == typeof(int)) {
                return ConvertToEncodingName((int)value);
            }
            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string) || destinationType == typeof(int);
        }

        /// <summary>
        /// Converts code page number to the user-friendly encoding name
        /// </summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value.GetType() == typeof(string)) {
                return ConvertToCodePage(value as string);
            } else if (value.GetType() == typeof(int)) {
                return ConvertToEncodingName((int)value);
            }
            return null;
        }

        private int ConvertToCodePage(string encodingName) {
            if (encodingName.EqualsOrdinal(Resources.Settings_DefaultValue)) {
                return 0;
            }
            var enc = Encoding.GetEncodings().FirstOrDefault(e => e.DisplayName.EqualsOrdinal(encodingName));
            return enc != null ? enc.CodePage : 0;
        }

        private string ConvertToEncodingName(int codePage) {
            var enc = Encoding.GetEncodings().FirstOrDefault(e => e.CodePage == codePage);
            return enc != null ? enc.DisplayName : Resources.Settings_DefaultValue;
        }
    }
}
