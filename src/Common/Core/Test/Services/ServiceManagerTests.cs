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
                .AddService<I1>((s) => {
                    var i2 = s.GetService<I2>();
                    return new C1();
                })
                .AddService<I2>((s) => {
                    var i1 = s.GetService<I1>();
                    return new C2();
                });

            Action a = () => _serviceManager.GetService<I1>();
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
        public void DoubleAdd() {
            _serviceManager.AddService<I1>(new C1());
            
            Action a = () => _serviceManager.AddService<I1>(new C1());
            a.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void AddRemove() {
            var s = new C1();
            _serviceManager.AddService(s);
            _serviceManager.RemoveService(s);
            _serviceManager.GetService<I1>().Should().BeNull();

            _serviceManager.AddService(s as I1);
            _serviceManager.RemoveService<I1>();
            _serviceManager.GetService<I1>().Should().BeNull();
        }

        [Test]
        public void Disposed() {
            var instance = new C1();
            _serviceManager.AddService(instance);
            _serviceManager.Dispose();

            Action a = () => _serviceManager.AddService<I1>(new C1());
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.AddService<I2>(new C2());
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.AddService<I2>((s) => new C2());
            a.ShouldThrow<ObjectDisposedException>();

            a = () => _serviceManager.GetService<I1>();
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => _serviceManager.GetService<I2>();
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => { var l = _serviceManager.GetServices<I1>().ToList(); };
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => { var l = _serviceManager.GetServices<I2>().ToList(); };
            a.ShouldNotThrow<ObjectDisposedException>();

            a = () => _serviceManager.RemoveService(instance);
            a.ShouldNotThrow<ObjectDisposedException>();
        }

        private interface I1 {}
        private interface I2 {}

        private class C1 : I1 {}
        private class C2 : I2 {}
    }
}
