using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    /// <summary>
    /// set Visibility to Hidden for false, Visibile for true
    /// </summary>
    public class BooleanToHidingVisibilityConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool) value;
            }
            else if (value is bool?)
            {
                bool? boolean = (bool?)value;
                if (boolean.HasValue)
                {
                    flag = boolean.Value;
                }
            }
            return flag ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
