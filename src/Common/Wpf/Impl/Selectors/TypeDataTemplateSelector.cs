using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Common.Wpf.Selectors {
    /// <summary>
    /// Searches for the data template that is available in container resources.
    /// The key can be either the type of the item or any of its implemented interfaces
    /// </summary>
    public class TypeDataTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var fe = container as FrameworkElement;
            if (fe == null || item == null) {
                return base.SelectTemplate(item, container);
            }

            var type = item.GetType();

            foreach (var key in new [] { type }.Concat(type.GetInterfaces()).Select(t => new DataTemplateKey(t))) {
                var resource = fe.TryFindResource(key) as DataTemplate;
                if (resource != null) {
                    return resource;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
