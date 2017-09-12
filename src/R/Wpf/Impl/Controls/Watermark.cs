// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Microsoft.Common.Wpf.Controls.Overlay;

namespace Microsoft.R.Wpf.Controls {
    public class Watermark {
        public static string GetTextBoxHint(TextBox frameworkElement) {
            return (string)frameworkElement.GetValue(TextBoxHintProperty);
        }

        public static void SetTextBoxHint(TextBox textBox, string value) {
            textBox.SetValue(TextBoxHintProperty, value);
        }

        public static readonly DependencyProperty TextBoxHintProperty =
            DependencyProperty.RegisterAttached("TextBoxHint", typeof(string), typeof(Watermark),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnTextBoxHintChanged));

        private static void OnTextBoxHintChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var oldValue = args.OldValue as string;
            var newValue = args.NewValue as string;
            if (Equals(oldValue, newValue)) {
                return;
            }

            var textBox = obj as TextBox;
            if (textBox == null) {
                return;
            }

            var textBlock = EnsureAdorner(textBox);
            textBlock.Text = newValue;
            textBox.ToolTip = newValue;
        }

        private static TextBlock EnsureAdorner(TextBox textBox) {
            var content = GetAdornerContent(textBox);
            if (content != null) {
                return (TextBlock)content;
            } 

            var textBlock = new TextBlock {
                Foreground = (Brush)textBox.FindResource(Brushes.GrayTextBrushKey),
                Margin = new Thickness(2,0,0,0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = string.IsNullOrEmpty(textBox.Text) && textBox.IsVisible ? Visibility.Visible : Visibility.Collapsed
            };

            void SetTextBlockVisibility<TArgs>(object sender, TArgs args) {
                textBlock.Visibility = string.IsNullOrEmpty(textBox.Text) && textBox.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            } 

            textBox.TextChanged += SetTextBlockVisibility;
            textBox.IsVisibleChanged += SetTextBlockVisibility;

            SetAdornerContent(textBox, textBlock);

            return textBlock;
        }
    }
}
