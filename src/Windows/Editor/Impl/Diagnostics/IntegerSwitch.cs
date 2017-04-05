// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Languages.Editor.Diagnostics {
    [ExcludeFromCodeCoverage]
    public class IntegerSwitch : Switch {
        private int _value;

        public IntegerSwitch(string name, string description, int value)
            : base(name, description, value.ToString(CultureInfo.CurrentCulture)) {
            _value = value;
        }

        public int SwitchValue {
            get {
                int value;
                if (Int32.TryParse(this.Value, out value)) {
                    _value = value;
                }

                return _value;
            }
        }
    }
}
