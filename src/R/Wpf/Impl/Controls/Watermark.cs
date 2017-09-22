// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Microsoft.Common.Wpf.Controls.Overlay;

namespace Microsoft.R.Wpf.Controls {
    public class Watermark {
        public static readonly DependencyProperty TextBoxHintProperty =
            DependencyProperty.RegisterAttached("TextBoxHint", typeof(string), typeof(Watermark),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnHintChanged));

        public static readonly DependencyProperty PasswordBoxHintProperty =
            DependencyProperty.RegisterAttached("PasswordBoxHint", typeof(string), typeof(Watermark),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnHintChanged));

        public static string GetTextBoxHint(TextBox textBox) => (string)textBox.GetValue(TextBoxHintProperty);

        public static void SetTextBoxHint(TextBox textBox, string value) => textBox.SetValue(TextBoxHintProperty, value);

        public static string GetPasswordBoxHint(PasswordBox passwordBox) => (string)passwordBox.GetValue(PasswordBoxHintProperty);

        public static void SetPasswordBoxHint(PasswordBox passwordBox, string value) => passwordBox.SetValue(PasswordBoxHintProperty, value);

        private static void OnHintChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var newValue = args.NewValue as string;
            if (Equals(args.OldValue, newValue)) {
                return;
            }

            if (!(obj is FrameworkElement frameworkElement)) {
                return;
            }

            var textBlock = EnsureAdorner(frameworkElement);
            if (textBlock == null) {
                return;
            }

            textBlock.Text = newValue;
            if (frameworkElement.ToolTip == null || string.Empty.Equals(frameworkElement.ToolTip)) {
                frameworkElement.ToolTip = newValue;
            }
        }

        private static TextBlock EnsureAdorner(FrameworkElement frameworkElement) {
            switch (frameworkElement) {
                case TextBox textBox:
                    return EnsureAdorner(textBox);
                case PasswordBox passwordBox:
                    return EnsureAdorner(passwordBox);
                default:
                    return null;
            }
        }

        private static TextBlock EnsureAdorner(TextBox textBox) {
            var content = GetAdornerContent(textBox);
            if (content != null) {
                return (TextBlock)content;
            } 

            var textBlock = CreateTextBlock(textBox);
            SetTextBlockVisibility();
            textBox.TextChanged += Handler;
            textBox.IsVisibleChanged += Handler;

            SetAdornerContent(textBox, textBlock);

            return textBlock;

            void SetTextBlockVisibility() => textBlock.Visibility = string.IsNullOrEmpty(textBox.Text) && textBox.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            void Handler<TArgs>(object sender, TArgs args) => SetTextBlockVisibility();
        }

        private static TextBlock EnsureAdorner(PasswordBox passwordBox) {
            var content = GetAdornerContent(passwordBox);
            if (content != null) {
                return (TextBlock)content;
            }

            var textBlock = CreateTextBlock(passwordBox);
            SetTextBlockVisibility();
            passwordBox.PasswordChanged += Handler;
            passwordBox.IsVisibleChanged += Handler;

            SetAdornerContent(passwordBox, textBlock);

            return textBlock;

            void SetTextBlockVisibility() => textBlock.Visibility = passwordBox.SecurePassword.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            void Handler<TArgs>(object sender, TArgs args) => SetTextBlockVisibility();
        }

        private static TextBlock CreateTextBlock(FrameworkElement frameworkElement) {
            return new TextBlock {
                Foreground = (Brush)frameworkElement.FindResource(Brushes.GrayTextBrushKey),
                Margin = new Thickness(2, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }
}
