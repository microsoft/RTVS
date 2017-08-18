using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.Common.Wpf.Collections {
    /// <summary>
    /// ObservableCollection that supports bulk changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchObservableCollection<T> : ObservableCollection<T> {
        private readonly CountdownDisposable _batchUpdate;

        public BatchObservableCollection() {
            _batchUpdate = new CountdownDisposable(OnBatchUpdateCompleted);
        }

        public BatchObservableCollection(IEnumerable<T> collection) : base(collection) {
            _batchUpdate = new CountdownDisposable(OnBatchUpdateCompleted);
        }

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
                OnBatchUpdateCompleted();
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
                OnBatchUpdateCompleted();
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
                OnBatchUpdateCompleted();
            }
        }

        public IDisposable StartBatchUpdate() => _batchUpdate.Increment();

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (_batchUpdate.Count == 0) { 
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (_batchUpdate.Count == 0) {
                base.OnPropertyChanged(e);
            }
        }

        private void OnBatchUpdateCompleted() {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
