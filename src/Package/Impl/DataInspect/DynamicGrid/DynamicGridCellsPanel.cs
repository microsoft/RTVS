using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Package.DataInspect {

    public struct LayoutInfo {
        public int FirstItemIndex { get; set; }
        public double FirstItemOffset { get; set; }
        public int ItemCountInViewport { get; set; }
    }

    public interface IScrollInfoGiver {
        LayoutInfo GetLayoutInfo(Size size);

        event EventHandler SharedScrollChanged;
    }

    internal class DynamicGridCellsPanel : VirtualizingPanel {
        private IScrollInfoGiver _sharedScrollInfo;
        internal IScrollInfoGiver SharedScroll {
            get {
                if (_sharedScrollInfo == null) {
                    _sharedScrollInfo = ItemsControl.GetItemsOwner(this) as IScrollInfoGiver;
                    if (_sharedScrollInfo == null) {
                        throw new NotSupportedException($"{typeof(DynamicGridCellsPanel)} supports only ItemsControl that implements {typeof(IScrollInfoGiver)}");
                    }
                    _sharedScrollInfo.SharedScrollChanged += SharedScrollChanged;
                }

                return _sharedScrollInfo;
            }
        }

        #region Layout

        private void SharedScrollChanged(object sender, EventArgs e) {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize) {
            // work around to make sure ItemContainerGenerator non-null
            var children = Children;

            var layoutInfo = SharedScroll.GetLayoutInfo(availableSize);

            int startIndex = layoutInfo.FirstItemIndex;
            int viewportCount = layoutInfo.ItemCountInViewport;

            IItemContainerGenerator generator = this.ItemContainerGenerator;
            GeneratorPosition position = generator.GeneratorPositionFromIndex(startIndex);

            // if realized, position.Offset ==0, use just position's index
            // otherwise, add one to insert after it.
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
                        if (childIndex >= InternalChildren.Count) {
                            AddInternalChild(child);
                        } else {
                            InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    } else {
                        Debug.Assert(child == InternalChildren[childIndex]);
                    }

                    if (newlyRealized) {
                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                    if (child.DesiredSize.Height > height) {
                        height = child.DesiredSize.Height;
                    }

                    child.ColumnWidth.Max = child.DesiredSize.Width;

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

                child.Arrange(new Rect(x, 0, child.ColumnWidth.Max, finalSize.Height));
                x += child.ColumnWidth.Max;
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
