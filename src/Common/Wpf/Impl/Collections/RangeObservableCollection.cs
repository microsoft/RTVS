using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Common.Wpf.Collections {
    /// <summary>
    /// ObservableCollection that supports bulk changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeObservableCollection<T> : ObservableCollection<T> {

        public RangeObservableCollection() { }

        public RangeObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var i in collection) {
                Items.Add(i);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var i in collection) {
                Items.Remove(i);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ReplaceWith(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            Items.Clear();
            foreach (var i in collection) {
                Items.Add(i);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
