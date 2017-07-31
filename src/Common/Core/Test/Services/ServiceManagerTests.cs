// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Services {
    public class ServiceManagerTests {
        private readonly ServiceManager _serviceManager;

        public ServiceManagerTests() {
            _serviceManager = new ServiceManager();
        }

        [Test]
        public void GetAccessToNonCreatedService_Recursion() {
            _serviceManager
                .AddService<C3>()
                .AddService<C4>();

            Action a = () => _serviceManager.GetService<I1>();
            a.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void DoubleAdd()
        {
            var service = new C1();
            _serviceManager.AddService(service);

            Action a = () => _serviceManager.AddService(new C1());
            a.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ServiceOfTypeLazyObject() {
            _serviceManager
                .AddService(new Lazy<object>())
                .AddService<object>();

            _serviceManager.GetService<Lazy<object>>().Should().NotBeNull();
            _serviceManager.GetService<object>().Should().NotBeNull();
        }

        [Test]
        public void AddRemove() {
            var s = new C1();
            _serviceManager.AddService(s);
            _serviceManager.RemoveService(s);
            _serviceManager.GetService<I1>().Should().BeNull();

            I1 i1 = s;
            _serviceManager.AddService(i1);
            _serviceManager.RemoveService(i1);
            _serviceManager.GetService<I1>().Should().BeNull();
        }

        [Test]
        public void AddRemoveDerived() {
            var d = new Derived(_serviceManager);
            _serviceManager.GetService<Derived>().Should().NotBeNull();
            _serviceManager.GetService<Base>().Should().NotBeNull();
            _serviceManager.GetService<I1>().Should().NotBeNull();
            _serviceManager.GetService<I2>().Should().NotBeNull();

            d.Dispose();
            _serviceManager.GetService<Derived>().Should().BeNull();
            _serviceManager.GetService<Base>().Should().BeNull();
            _serviceManager.GetService<I1>().Should().BeNull();
            _serviceManager.GetService<I2>().Should().BeNull();
        }

        [Test]
        public void AddRemoveLazy03() {
            _serviceManager.AddService<Derived>();
            _serviceManager.RemoveService(_serviceManager.GetService<Base>());

            _serviceManager.GetService<I1>().Should().BeNull();
            _serviceManager.GetService<I2>().Should().BeNull();
        }

        [Test]
        public void AddByInterface01() {
            _serviceManager.AddService<I2, Derived>();
            _serviceManager.GetService<I1>().Should().BeNull();
            _serviceManager.GetService<I2>().Should().NotBeNull();
        }

        [Test]
        public void AddByInterface02() {
            _serviceManager.AddService<I2, Derived>();
            _serviceManager.GetService<I2>().Should().NotBeNull();
            _serviceManager.GetService<I1>().Should().BeNull();
        }

        [Test]
        public void Disposed() {
            var instance = new C1();
            _serviceManager.AddService(instance);
            _serviceManager.Dispose();

            Action a = () => _serviceManager.AddService(new C1());
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.AddService(new C2());
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.AddService<C2>();
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.GetService<I1>();
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => _serviceManager.GetService<I2>();
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => _serviceManager.RemoveService(instance);
            a.ShouldNotThrow<ObjectDisposedException>();
        }

        private interface I1 { }
        private interface I2 { }

        private class C1 : I1 { }
        private class C2 : I2 { }

        private class C3 : I1 {
            public C3(IServiceContainer s) {
                s.GetService<I2>();
            }
        }
        private class C4 : I2 {
            public C4(IServiceContainer s) {
                s.GetService<I1>();
            }
        }

        private class Base : I1, IDisposable {
            private readonly IServiceManager _sm;
            protected Base(): this(null) { }

            protected Base(IServiceManager sm) {
                _sm = sm;
                _sm?.AddService(this);
            }
            public void Dispose() {
                _sm?.RemoveService(this);
            }
        }
        private class Derived : Base, I2 {
            public Derived() { }
            public Derived(IServiceManager sm) : base(sm) { }
        }
    }
}
