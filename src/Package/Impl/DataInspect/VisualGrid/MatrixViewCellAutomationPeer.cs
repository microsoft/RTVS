// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-support-for-the-dataitem-control-type
    /// </summary>
    internal sealed class MatrixViewCellAutomationPeer : MatrixViewItemAutomationPeer, ITableItemProvider, IScrollItemProvider, IVirtualizedItemProvider {
        public int Row { get; }
        public int Column { get; }
        public int RowSpan { get; }
        public int ColumnSpan { get; }

        public IRawElementProviderSimple ContainingGrid => ProviderFromPeer(Owner.AutomationPeer);

        public MatrixViewCellAutomationPeer(MatrixView matrixView, int row, int column) : base(matrixView) {
            Row = row;
            Column = column;
            RowSpan = 1;
            ColumnSpan = 1;
        }

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;
        
        public override object GetPattern(PatternInterface patternInterface) {
            switch (patternInterface) {
                case PatternInterface.ScrollItem:
                case PatternInterface.GridItem:
                case PatternInterface.TableItem:
                case PatternInterface.VirtualizedItem when VirtualizedItemPatternIdentifiers.Pattern != null && !IsRealized:
                    return this;
                default:
                    return null;
            }
        }

        protected override Rect GetBoundingRectangleCore() {
            var bounds = Points.GetBounds(Row, Column);
            return new Rect(Owner.Data.PointToScreen(bounds.TopLeft), Owner.Data.PointToScreen(bounds.BottomRight));
        }

        protected override string GetClassNameCore() => "MatrixViewCell";

        protected override bool IsKeyboardFocusableCore() => Owner.Focusable;

        protected override bool HasKeyboardFocusCore() 
            => Owner.Data.SelectedIndex.Equals((Row, Column)) && Owner.Data.HasKeyboardFocus;

        protected override string GetAutomationIdCore() => $"{Owner.AutomationPeer.GetAutomationId()}_{Row}_{Column}";

        protected override bool IsContentElementCore() => true;

        protected override void SetFocusCore() => Owner.SetCellFocus(Row, Column);

        public IRawElementProviderSimple[] GetRowHeaderItems() => null;

        public IRawElementProviderSimple[] GetColumnHeaderItems()
            => new[] { ProviderFromPeer(Owner.AutomationPeer.GetOrCreateHeader(Column)) };

        public void ScrollIntoView() => Owner.Scroller.ScrollIntoViewAsync(Row, Column).DoNotWait();

        public void Realize() => Owner.Scroller.ScrollIntoViewAsync(Row, Column).DoNotWait();

        protected override Rect GetVisibleRect() => Rect.Intersect(new Rect(new Size(Points.ViewportWidth, Points.ViewportHeight)), Points.GetBounds(Row, Column));
    }
}