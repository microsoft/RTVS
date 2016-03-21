using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.R.Components.PackageManager.Implementation.Xaml {
    /// <summary>
    /// Interaction logic for InfiniteScrollList.xaml
    /// </summary>
    public partial class InfiniteScrollList : UserControl {
        // Indicates wether check boxes are enabled on packages
        private bool _checkBoxesEnabled;

        public bool CheckBoxesEnabled {
            get { return _checkBoxesEnabled; }
            set {
                _checkBoxesEnabled = value;

                if (!_checkBoxesEnabled) {
                    // the current tab is not "updates", so the container
                    // should become invisible.
                    UpdateButtonContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        public InfiniteScrollList() {
            InitializeComponent();
            CheckBoxesEnabled = false;
        }

        private void CheckBoxSelectAllPackages_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void CheckBoxSelectAllPackages_Unchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_Loaded(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }

    //public class InfiniteScrollListItemStyleSelector : StyleSelector {
    //    private Style PackageItemStyle { get; set; }
    //    private Style LoadingStatusIndicatorStyle { get; set; }

    //    private void Init(ItemsControl infiniteScrollList) {
    //        if (PackageItemStyle == null && LoadingStatusIndicatorStyle == null) {
    //            PackageItemStyle = (Style)infiniteScrollList.FindResource("packageItemStyle");
    //            LoadingStatusIndicatorStyle = (Style)infiniteScrollList.FindResource("loadingStatusIndicatorStyle");

    //            if (!StandaloneSwitch.IsRunningStandalone && PackageItemStyle.Setters.Count == 0) {
    //                var setter = new Setter(Control.TemplateProperty, infiniteScrollList.FindResource("ListBoxItemTemplate"));
    //                PackageItemStyle.Setters.Add(setter);
    //            }
    //        }
    //    }

    //    public override Style SelectStyle(object item, DependencyObject container) {
    //        Init(ItemsControl.ItemsControlFromItemContainer(container));

    //        if (item is LoadingStatusIndicator) {
    //            return LoadingStatusIndicatorStyle;
    //        }

    //        return PackageItemStyle;
    //    }
    //}
}
