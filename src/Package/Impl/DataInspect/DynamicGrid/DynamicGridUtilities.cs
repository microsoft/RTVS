using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridUtilities {
        public static Size DecreaseSize(Size size, double thickness) {
            double width = size.Width - thickness;
            width = Math.Max(0.0, width);

            double height = size.Height - thickness;
            height = Math.Max(0.0, height);

            return new Size(width, height);
        }
    }
}
