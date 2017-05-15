// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Services {
    /// <summary>
    /// Allows property owners to control the lifetimes of the properties in the collection. 
    /// </summary>
    /// <remarks>This collection is synchronized in order to allow access by multiple threads.</remarks>
    public sealed class PropertyDictionary {
        private readonly Lazy<Dictionary<object, object>> _properties = Lazy.Create(() => new Dictionary<object, object>());
        private readonly object _lock = new object();

        /// <summary>
        /// Adds a new property to the collection.
        /// </summary>
        /// <param name="key">The key by which the property can be retrieved. Must be non-null.</param>
        /// <param name="property">The property to associate with the key.</param>
        /// <exception cref="ArgumentException">An element with the same key already exists in the PropertyCollection.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public void AddProperty(object key, object property) {
            lock (_lock) {
                _properties.Value.Add(key, property);
            }
        }

        /// <summary>
        /// Removes the property associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the property to remove.</param>
        /// <returns><c>true</c> if the property was found and removed, <c>false</c> if the property was not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public void RemoveProperty(object key) {
            lock (_lock) {
                _properties.Value.Remove(key);
            }
        }

        /// <summary>
        /// Gets or creates a property of type <typeparamref name="T"/> from the property collection. If
        /// there is already a property with the specified <paramref name="key"/>, returns the existing property. Otherwise,
        /// uses <paramref name="creator"/> to create an instance of that type and add it to the collection with the specified <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="key">The key of the property to get or create.</param>
        /// <param name="factory">The delegate used to create the property (if needed).</param>
        /// <returns>The property that was requested.</returns>
        public T GetOrCreateSingletonProperty<T>(object key, Func<T> factory) where T : class {
            Check.ArgumentNull(nameof(factory), factory);

            lock (_lock) {
                if (_properties.Value.TryGetValue(key, out object property)) {
                    return property as T;
                }

                var result = factory();

                // It is possible that executing the creator function has the side-effect of 
                // adding a property with this key to the property bag. The locks only prevents 
                // access from other threads, not from re-entrant calls by the same thread. 
                // This is bad since thecreator function is getting called twice. Our best option 
                // is to discard the result created above and return the one that is already 
                // in the property bag so, at least, we are being consistent.
                if (_properties.Value.TryGetValue(key, out property)) {
                    return property as T;
                }

                _properties.Value[key] = result;
                return result;
            }
        }

        /// <summary>
        /// Gets or creates a property of type <typeparamref name="T"/> from the property collection. If
        /// there is already a property of that type, it returns the existing property. Otherwise, it
        /// uses <paramref name="creator"/> to create an instance of that type.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="creator">The delegate used to create the property (if needed).</param>
        /// <returns>An instance of the property.</returns>
        /// <remarks>The key used in the property collection will be typeof(T).</remarks>
        public T GetOrCreateSingletonProperty<T>(Func<T> creator) where T : class => GetOrCreateSingletonProperty<T>(typeof(T), creator);

        /// <summary>
        /// Gets the property associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The property value, or null if the property is not set.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> does not exist in the property collection.</exception>
        public TProperty GetProperty<TProperty>(object key) => (TProperty)this.GetProperty(key);

        /// <summary>
        /// Gets the property associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The property value, or null if the property is not set.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> does not exist in the property collection.</exception>
        public object GetProperty(object key) {
            lock (_lock) {
                if (!_properties.IsValueCreated) {
                    throw new KeyNotFoundException(nameof(key));
                }

                if (!_properties.Value.TryGetValue(key, out object item)) {
                    throw new KeyNotFoundException(nameof(key));
                }
                return item;
            }
        }

        /// <summary>
        /// Gets the property associated with the specified key.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property associated with the specified key.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="property">The retrieved property, or default(TValue) if there is
        /// no property associated with the specified key.</param>
        /// <returns><c>true</c> if the property was found, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetProperty<T>(object key, out T property) {
            lock (_lock) {
                if (_properties.IsValueCreated) {
                    if (_properties.Value.TryGetValue(key, out var item)) {
                        property = (T)item;
                        return true;
                    }
                }
            }
            property = default(T);
            return false;
        }

        /// <summary>
        /// Determines whether the property collection contains a property for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the property exists, otherwise <c>false</c>.</returns>
        public bool ContainsProperty(object key) {
            lock (_lock) {
                return _properties.IsValueCreated && _properties.Value.ContainsKey(key);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        public object this[object key] {
            get => GetProperty(key);
            set => SetProperty(key, value);
        }

        /// <summary>
        /// Returns the property collection as a read-only collection.
        /// </summary>
        /// <value>The read-only collection.</value>
        public ReadOnlyCollection<KeyValuePair<object, object>> PropertyList {
            get {
                if (!_properties.IsValueCreated) {
                    return new List<KeyValuePair<object, object>>().AsReadOnly();
                }
                var propertyList = new List<KeyValuePair<object, object>>();
                lock (_lock) {
                    foreach (var property in _properties.Value) {
                        propertyList.Add(new KeyValuePair<object, object>(property.Key, property.Value));
                    }
                }
                return propertyList.AsReadOnly();
            }
        }

        private void SetProperty(object key, object property) {
            lock (_lock) {
                _properties.Value[key] = property;
            }
        }
    }
}
