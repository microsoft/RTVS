// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    internal class Container<T> : ContentControl, IDisposable, IVisualComponentContainer<T> where T : IVisualComponent {
        private readonly Action _onDispose;

        public Container(Action onDispose) {
            _onDispose = onDispose;
        }

        public T Component { get; set; }

        public string CaptionText { get; set; }
        public string StatusText { get; set; }

        public bool IsOnScreen => Visibility == Visibility.Visible;

        public void Hide() {
            Visibility = Visibility.Hidden;
        }

        public void Show(bool focus, bool immediate) {
            Visibility = Visibility.Visible;
        }

        public void ShowContextMenu(CommandID commandId, Point position) {
        }

        public void UpdateCommandStatus(bool immediate) {
        }

        public void Dispose() {
            _onDispose();
        }
    }
}