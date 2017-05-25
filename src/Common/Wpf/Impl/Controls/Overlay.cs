// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Common.Wpf.Controls {
    public class Overlay {
        public static UIElement GetAdornerContent(FrameworkElement frameworkElement) 
            => (UIElement)frameworkElement.GetValue(AdornerContentProperty);

        public static void SetAdornerContent(FrameworkElement frameworkElement, UIElement value) 
            => frameworkElement.SetValue(AdornerContentProperty, value);

        public static readonly DependencyProperty AdornerContentProperty =
            DependencyProperty.RegisterAttached("AdornerContent", typeof(UIElement), typeof(Overlay),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnAdornerContentChanged));

        private static void OnAdornerContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var oldValue = args.OldValue as UIElement;
            var newValue = args.NewValue as UIElement;
            if (Equals(oldValue, newValue)) {
                return;
            }

            var frameworkElement = obj as FrameworkElement;
            if (frameworkElement == null) {
                return;
            }

            // No reason to do anything before loaded 
            if (frameworkElement.IsLoaded) {
                EnsureAdornerAttached(frameworkElement, newValue);
            } else {
                frameworkElement.Loaded += FrameworkElement_Loaded;
            }
        }

        private static void EnsureAdornerAttached(FrameworkElement frameworkElement, UIElement watermark) {
            var layer = AdornerLayer.GetAdornerLayer(frameworkElement);
            var oldWatermarkAdorner = GetWatermarkAdorner(layer, frameworkElement);

            if (oldWatermarkAdorner != null) {
                layer.Remove(oldWatermarkAdorner);
                oldWatermarkAdorner.Dispose();
            }

            layer.Add(new OverlayAdorner(layer, frameworkElement, watermark));
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e) {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null) {
                return;
            }

            frameworkElement.Loaded -= FrameworkElement_Loaded;
            var watermark = GetAdornerContent(frameworkElement);
            EnsureAdornerAttached(frameworkElement, watermark);
        }

        private static OverlayAdorner GetWatermarkAdorner(AdornerLayer layer, UIElement adornedElement) => layer.GetAdorners(adornedElement)?.OfType<OverlayAdorner>().FirstOrDefault();

        private static IList<DependencyPropertyDescriptor> GetDescriptors(DependencyObject obj) => GetPropertyDescriptors(obj)
            .Cast<PropertyDescriptor>()
            .Select(DependencyPropertyDescriptor.FromProperty)
            .Where(dpd => dpd != null && ((dpd.Metadata as FrameworkPropertyMetadata)?.AffectsRender == true || dpd.DependencyProperty == UIElement.VisibilityProperty))
            .ToList();

        private static PropertyDescriptorCollection GetPropertyDescriptors(DependencyObject obj) 
            => TypeDescriptor.GetProperties(obj, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.Valid) });

        private class OverlayAdorner : Adorner, IDisposable {
            private readonly AdornerLayer _layer;
            private readonly ContentPresenter _contentPresenter;
            private readonly IList<DependencyPropertyDescriptor> _descriptors;
            private static DependencyPropertyDescriptor _isVisibleDescriptor;

            static OverlayAdorner() {
                _isVisibleDescriptor = DependencyPropertyDescriptor.FromProperty(IsVisibleProperty, typeof(AdornerLayer));
            }

            public OverlayAdorner(AdornerLayer layer, FrameworkElement adornedElement, UIElement content) : base(adornedElement) {
                _contentPresenter = new ContentPresenter {
                    Content = content
                };
                _descriptors = GetDescriptors(content);
                _layer = layer;

                foreach (var descriptor in _descriptors) {
                    descriptor.AddValueChanged(content, OnValueChanged);
                }
                _isVisibleDescriptor.AddValueChanged(_layer, OnValueChanged);

                ClipToBounds = true;
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => _contentPresenter;

            protected override Size MeasureOverride(Size constraint) {
                var frameworkElement = (FrameworkElement) AdornedElement;
                _contentPresenter.Height = frameworkElement.ActualHeight;
                _contentPresenter.Width = frameworkElement.ActualWidth;
                _contentPresenter.Measure(constraint);
                return _contentPresenter.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize) {
                if (!AdornedElement.IsVisible) {
                    return new Size(0, 0);
                }

                _contentPresenter.Arrange(new Rect(finalSize));
                return finalSize;
            }

            private void OnValueChanged(object sender, EventArgs eventArgs) {
                if (VisualParent != null) {
                    _layer.Update(AdornedElement);
                }
            }

            public void Dispose() {
                foreach (var descriptor in _descriptors) {
                    descriptor.RemoveValueChanged(AdornedElement, OnValueChanged);
                }
                _isVisibleDescriptor.RemoveValueChanged(_layer, OnValueChanged);
            }
        }
    }
}
