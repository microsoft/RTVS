using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Languages.Editor.Diagnostics
{
    public class IntegerSwitch : Switch
    {
        private int _value;

        public IntegerSwitch(string name, string description, int value)
            : base(name, description, value.ToString(CultureInfo.CurrentCulture))
        {
            _value = value;
        }

        public int SwitchValue
        {
            get
            {
                int value;
                if (Int32.TryParse(this.Value, out value))
                {
                    _value = value;
                }

                return _value;
            }
        }
    }
}
