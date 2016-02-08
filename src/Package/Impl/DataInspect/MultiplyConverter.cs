using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class MultiplyConverter : IValueConverter {
        public double Coefficient { get; set; }

        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter != null) {
                if (parameter is double) {
                    return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * (double)parameter;
                } else if (parameter is string) {

                    double coefficient;
                    if (double.TryParse((string)parameter, out coefficient)) {
                        return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * coefficient;
                    }
                }
                Debug.Assert(false, "MultiplyConverter parameter is not convertable to double");
            }
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * Coefficient;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
