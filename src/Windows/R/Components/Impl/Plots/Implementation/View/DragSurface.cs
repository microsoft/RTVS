// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.R.Components.Plots.Implementation.View {
    internal class DragSurface {
        private Point? _lastMousedownPosition;

        public void MouseDown(MouseEventArgs e) {
            _lastMousedownPosition = e.GetPosition(null);
        }

        public void MouseMove(MouseEventArgs e) {
            if (e.MouseDevice.LeftButton == MouseButtonState.Released) {
                _lastMousedownPosition = null;
            }
        }

        public void MouseLeave(MouseEventArgs e) {
            _lastMousedownPosition = null;
        }

        public bool IsMouseMoveStartingDrag(MouseEventArgs e) {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed && _lastMousedownPosition.HasValue) {
                var distance = e.GetPosition(null) - _lastMousedownPosition.Value;
                if (Math.Abs(distance.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(distance.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    return true;
                }
            }
            return false;
        }
    }
}
