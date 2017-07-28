// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Microsoft.Common.Wpf.Extensions;

namespace Microsoft.Common.Wpf.Automation {
    public class AutomationPropertyChangedBase {
        private readonly UIElement _element;

        public AutomationPropertyChangedBase(UIElement element) {
            _element = element;
        }

        protected bool SetProperty(ref double storage, double value, AutomationProperty property) {
            if (storage.IsCloseTo(value)) {
                return false;
            }

            storage = value;
            RaisePropertyChanged(storage, value, property);

            return true;
        }

        protected bool SetProperty<T>(ref T storage, T value, AutomationProperty property) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) {
                return false;
            }

            storage = value;
            RaisePropertyChanged(storage, value, property);

            return true;
        }

        private void RaisePropertyChanged<T>(T storage, T value, AutomationProperty property)
            => UIElementAutomationPeer.FromElement(_element)?.RaisePropertyChangedEvent(property, storage, value);
    }

}
