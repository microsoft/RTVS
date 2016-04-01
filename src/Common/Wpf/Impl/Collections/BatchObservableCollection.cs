using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Common.Wpf.Collections {
    /// <summary>
    /// ObservableCollection that supports bulk changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchObservableCollection<T> : ObservableCollection<T> {

        public BatchObservableCollection() { }

        public BatchObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddMany(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            var raiseEvent = false;
            foreach (var i in collection) {
                Items.Add(i);
                raiseEvent = true;
            }

            if (raiseEvent) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void RemoveMany(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            var raiseEvent = false;
            foreach (var i in collection) {
                raiseEvent |= Items.Remove(i);
            }

            if (raiseEvent) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void ReplaceWith(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }
            
            var raiseEvent = Items.Count > 0;
            Items.Clear();
            foreach (var i in collection) {
                Items.Add(i);
                raiseEvent = true;
            }

            if (raiseEvent) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
