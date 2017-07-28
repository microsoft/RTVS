// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal sealed class MatrixViewHeaderAutomationPeer : MatrixViewItemAutomationPeer, IInvokeProvider {
        public int Column { get; }

        public MatrixViewHeaderAutomationPeer(MatrixView matrixView, int column) : base(matrixView) {
            Column = column;
        }

        protected override AutomationControlType GetAutomationControlTypeCore() 
            => AutomationControlType.HeaderItem;

        public override object GetPattern(PatternInterface patternInterface) {
            switch (patternInterface) {
                case PatternInterface.Invoke:
                case PatternInterface.VirtualizedItem when VirtualizedItemPatternIdentifiers.Pattern != null && !IsRealized:
                    return this;
                default:
                    return null;
            }
        }

        protected override Rect GetBoundingRectangleCore() {
            var bounds = new Rect(Points.xPosition(Column), 0, Points.GetWidth(Column), Points.ColumnHeaderHeight);
            return new Rect(Owner.PointToScreen(bounds.TopLeft), Owner.PointToScreen(bounds.BottomRight));
        }

        protected override string GetClassNameCore() => "MatrixViewHeader";

        protected override bool IsKeyboardFocusableCore() => Owner.Focusable;

        protected override bool HasKeyboardFocusCore()
            => Owner.ColumnHeader.SelectedIndex.Column == Column && Owner.ColumnHeader.HasKeyboardFocus;

        protected override string GetAutomationIdCore() => $"{Owner.AutomationPeer.GetAutomationId()}_{Column}";

        // AutomationControlType.HeaderItem must return IsContentElement false.
        // See http://msdn.microsoft.com/en-us/library/ms742202.aspx
        protected override bool IsContentElementCore() => false;

        protected override void SetFocusCore() => Owner.SetHeaderFocus(Column);

        public void Invoke() {}

        protected override Rect GetVisibleRect() {
            var left = Points.xPosition(Column);
            var right = left + Points.GetWidth(Column);

            return left.GreaterOrCloseTo(Points.ViewportWidth) || right.LessThanOrClose(0) 
                ? Rect.Empty
                : new Rect(Math.Max(left, 0), 0, Math.Min(right, Points.ViewportWidth), Points.ColumnHeaderHeight);
        }
    }
}