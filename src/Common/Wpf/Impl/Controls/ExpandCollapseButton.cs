// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;

namespace Microsoft.Common.Wpf.Controls {
    public class ExpandCollapseButton : ButtonBase {
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(nameof(Expanded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpandCollapseButton));
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(nameof(Collapsed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpandCollapseButton));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ExpandCollapseButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsExpandedChanged));

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

        protected override void OnClick() {
            IsExpanded = !IsExpanded;
            base.OnClick();
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