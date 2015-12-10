using System.Windows;

namespace Microsoft.VisualStudio.R.Package.Wpf {
    public static class WpfHelper {
        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        /// <remarks>
        /// Original code is from DataGridHelper
        /// </remarks>
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;

            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as FrameworkElement;
            }

            return null;
        }
    }
}

