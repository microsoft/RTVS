// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class EncodingTypeConverter : TypeConverter {
        private static readonly int[] _unicodePages = new int[] {
            Encoding.UTF7.CodePage ,
            Encoding.UTF8.CodePage,
            Encoding.Unicode.CodePage,
            Encoding.UTF32.CodePage,
            Encoding.BigEndianUnicode.CodePage,
            12001 // Big Endian UTF32
        };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var codePageList = new List<int>() { 0 };
            var codePages = Encoding.GetEncodings()
                                    .OrderBy(e => e.DisplayName)
                                    .Select(e => e.CodePage)
                                    .Except(_unicodePages);
            codePageList.AddRange(codePages);
            return new StandardValuesCollection(codePageList);
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
            } else if(value.GetType() == typeof(int)) {
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
            if(encodingName.EqualsOrdinal(Resources.Settings_OsDefaultEncoding)) {
                return 0;
            }
            var enc = Encoding.GetEncodings().FirstOrDefault(e => e.DisplayName.EqualsOrdinal(encodingName));
            return enc != null ? enc.CodePage : 0;
        }

        private string ConvertToEncodingName(int codePage) {
            var enc = Encoding.GetEncodings().FirstOrDefault(e => e.CodePage == codePage);
            return enc != null ? enc.DisplayName : Resources.Settings_OsDefaultEncoding;
        }
    }
}
