// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Components.View;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    internal abstract class ContainerFactoryBase<T> : IDisposable where T : IVisualComponent {
        private readonly Dictionary<int, Container<T>> _containers = new Dictionary<int, Container<T>>();

        protected Container<T> GetOrCreate(int instanceId, Func<Container<T>, T> factory) {
            return UIThreadHelper.Instance.Invoke(() => {
                Container<T> container;
                if (_containers.TryGetValue(instanceId, out container)) {
                    return container;
                }

                container = new Container<T>(() => DisposeContainer(instanceId));
                var component = factory(container);
                container.Component = component;
                _containers.Add(instanceId, container);
                return container;
            });
        }

        public virtual void Dispose() {
            UIThreadHelper.Instance.Invoke(() => {
                var containers = _containers.Values.ToList();
                foreach (var container in containers) {
                    container.Dispose();
                }
            });            
        }

        private void DisposeContainer(int id) {
            UIThreadHelper.Instance.Invoke(() => {
                Container<T> container;
                if (!_containers.TryGetValue(id, out container)) {
                    return;
                }

                _containers.Remove(id);
                var component = container.Component;
                container.Component = default(T);
                component?.Dispose();
                container.Dispose();
            });
        }
    }
}