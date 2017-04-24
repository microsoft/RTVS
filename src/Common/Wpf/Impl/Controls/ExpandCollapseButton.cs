// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Common.Wpf.Controls {
    public class ExpandCollapseButton : ButtonBase {
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(nameof(Expanded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpandCollapseButton));
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(nameof(Collapsed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpandCollapseButton));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ExpandCollapseButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsExpandedChanged));
        public static readonly DependencyProperty ExpandCollapseModeProperty = DependencyProperty.Register(nameof(ExpandCollapseMode), typeof(ExpandCollapseMode), typeof(ExpandCollapseButton),
                new FrameworkPropertyMetadata(ExpandCollapseMode.Click, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public event RoutedEventHandler Expanded {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }

        public event RoutedEventHandler Collapsed {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }

        public bool IsExpanded {
            get { return (bool) GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public ExpandCollapseMode ExpandCollapseMode {
            get { return (ExpandCollapseMode) GetValue(ExpandCollapseModeProperty); }
            set { SetValue(ExpandCollapseModeProperty, value); }
        }

        protected override void OnClick() {
            if (ExpandCollapseMode == ExpandCollapseMode.Click) {
                IsExpanded = !IsExpanded;
            }
            base.OnClick();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (ExpandCollapseMode == ExpandCollapseMode.LeftRightArrows) {
                if (e.Key == Key.Left) {
                    IsExpanded = false;
                } else if (e.Key == Key.Right) {
                    IsExpanded = true;
                }
            }
            base.OnKeyDown(e);
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ExpandCollapseButton button = (ExpandCollapseButton)d;
            button.OnIsExpandedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnIsExpandedChanged(bool oldValue, bool newValue) {
            if (oldValue == newValue) {
                return;
            }

            var peer = UIElementAutomationPeer.FromElement(this) as ExpandCollapseAutomationPeer;
            peer?.RaiseExpandCollapseAutomationEvent(oldValue, newValue);

            RaiseEvent(new RoutedEventArgs(newValue? ExpandedEvent : CollapsedEvent));
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new ExpandCollapseAutomationPeer(this);
    }
}