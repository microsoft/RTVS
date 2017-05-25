// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    internal abstract class PropertyPageViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Special text value that indicates that the string property values for the 
        /// selected configurations are not identical.
        /// </summary>
        internal static readonly string DifferentStringOptions = Resources.PropertyPageDifferentOptions;

        /// <summary>
        /// Special value that indicates that the bool property values for the selected
        /// configurations are not identical.
        /// </summary>
        internal static readonly bool? DifferentBoolOptions = null;

        public PropertyPageControl ParentControl { get; set; }

        /// <summary>
        /// Since calls to ignore events can be nested, a downstream call could change the outer 
        /// value.  To guard against this, IgnoreEvents returns true if the count is > 0 and there is no setter. 
        /// PushIgnoreEvents\PopIgnoreEvents  are used instead to control the count.
        /// </summary>
        private int _ignoreEventsNestingCount = 0;

        public bool IgnoreEvents { get { return _ignoreEventsNestingCount > 0; } }

        public void PushIgnoreEvents() {
            _ignoreEventsNestingCount++;
        }

        public void PopIgnoreEvents() {
            Debug.Assert(_ignoreEventsNestingCount > 0);
            if (_ignoreEventsNestingCount > 0) {
                _ignoreEventsNestingCount--;
            }
        }

        public abstract Task Initialize();

        public abstract Task<int> Save();

        protected virtual void OnPropertyChanged(string propertyName, bool suppressInvalidation = false) {
            // For some properties we don't want to invalidate the property page
            if (suppressInvalidation) {
                PushIgnoreEvents();
            }

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

            if (suppressInvalidation) {
                PopIgnoreEvents();
            }
        }

        protected virtual bool OnPropertyChanged<T>(ref T propertyRef, T value, bool suppressInvalidation, [CallerMemberName] string propertyName = null) {
            if (!Object.Equals(propertyRef, value)) {
                propertyRef = value;
                OnPropertyChanged(propertyName, suppressInvalidation);
                return true;
            }
            return false;
        }

        protected virtual bool OnPropertyChanged<T>(ref T propertyRef, T value, [CallerMemberName] string propertyName = null) {
            return OnPropertyChanged(ref propertyRef, value, suppressInvalidation: false, propertyName: propertyName);
        }

        /// <summary>
        /// Override to do cleanup
        /// </summary>
        public virtual void ViewModelDetached() {
        }
    }
}
