using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Package.DataInspect {

    internal struct LayoutInfo {
        public int FirstItemIndex { get; set; }
        public double FirstItemOffset { get; set; }
        public int ItemCountInViewport { get; set; }
    }

    internal interface SharedScrollInfo {
        LayoutInfo GetLayoutInfo(Size size);

        event EventHandler SharedScrollChanged;
    }


    internal class DynamicGridCellsPanel : VirtualizingPanel {
        private const double ItemMinWidth = 20;
        private SharedScrollInfo _sharedScrollInfo;

        #region Layout

        private bool sharedscrollinit = false;
        private void EnsurePrerequisite() {
            var children = Children;

            if (!sharedscrollinit) {
                _sharedScrollInfo = ItemsControl.GetItemsOwner(this) as SharedScrollInfo;
                if (_sharedScrollInfo == null) {
                    throw new NotSupportedException($"{typeof(DynamicGridCellsPanel)} supports only ItemsControl that implements {typeof(SharedScrollInfo)}");
                }
                _sharedScrollInfo.SharedScrollChanged += SharedScrollChanged;

                // TODO: doesn't support panel change on the fly yet.
                sharedscrollinit = true;

                VirtualizingStackPanel.AddCleanUpVirtualizedItemHandler(this, CleanUpVirtualizedItem);
            }
        }

        private static void CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e) {
            var me = (DynamicGridCellsPanel)sender;
        }

        private void SharedScrollChanged(object sender, EventArgs e) {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            var layoutInfo = _sharedScrollInfo.GetLayoutInfo(availableSize);

            int startIndex = layoutInfo.FirstItemIndex;
            int viewportCount = layoutInfo.ItemCountInViewport;

            IItemContainerGenerator generator = this.ItemContainerGenerator;
            GeneratorPosition position = generator.GeneratorPositionFromIndex(startIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (position.Offset == 0) ? position.Index : position.Index + 1;

            double height = this.ActualHeight;
            double width = 0;
            int finalCount = 0;
            using (generator.StartAt(position, GeneratorDirection.Forward, true)) {
                for (int i = 0; i < viewportCount; i++, childIndex++) {
                    bool newlyRealized;
                    DynamicGridCell child = generator.GenerateNext(out newlyRealized) as DynamicGridCell;
                    Debug.Assert(child != null);

                    if (newlyRealized) {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= InternalChildren.Count) {
                            AddInternalChild(child);
                        } else {
                            InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    } else {
                        Debug.Assert(child == InternalChildren[childIndex]);
                    }

                    double availableWidth = child.ColumnStripe.GetSizeConstraint();

                    if (newlyRealized) {
                        child.Measure(new Size(availableWidth, double.PositiveInfinity));
                    }
                    if (child.DesiredSize.Height > height) {
                        height = child.DesiredSize.Height;
                    }

                    child.ColumnStripe.LayoutSize.Max = child.DesiredSize.Width;

                    width += child.DesiredSize.Width;
                    finalCount++;

                    if (width > availableSize.Width) {
                        break;
                    }
                }
            }

            Size desired = new Size(width, height);

            if (finalCount > 0) {
                // TODO: the index might be invalid at the time when CleanUpItems runs
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => { CleanUpItems(startIndex, startIndex + finalCount - 1); }));
            }

            return desired;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            double x = 0.0;
            for (int i = 0; i < InternalChildren.Count; i++) {
                var child = InternalChildren[i] as DynamicGridCell;
                Debug.Assert(child != null);

                child.Arrange(new Rect(x, 0, child.ColumnStripe.LayoutSize.Max, finalSize.Height));
                x += child.ColumnStripe.LayoutSize.Max;
            }

            return finalSize;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated) {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--) {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated) {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        #endregion
    }
}
