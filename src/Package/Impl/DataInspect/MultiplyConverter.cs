using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public class MultiplyConverter : IValueConverter
    {
        public double Coefficient { get; set; }

        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string)
            {
                double coefficient = double.Parse((string)parameter);
                return System.Convert.ToDouble(value) * coefficient;
            }
            return System.Convert.ToDouble(value) * Coefficient;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
