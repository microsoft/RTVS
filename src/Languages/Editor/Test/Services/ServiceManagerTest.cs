// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Utilities;
using Xunit;

namespace Microsoft.Languages.Editor.Test.Services {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ServiceManagerTest {
        #region Mock Services
        interface IService1 {
            void DoSomething();
        }

        [ExcludeFromCodeCoverage]
        class Service1 : IService1 {
            public void DoSomething() { }
        }

        interface IService2 {
            void DoSomethingElse();
        }

        [ExcludeFromCodeCoverage]
        class Service2 : IService2 {
            public void DoSomethingElse() { }
        }
        #endregion

        #region Mock Property Owner
        [ExcludeFromCodeCoverage]
        class PropertyOwner : IPropertyOwner {
            PropertyCollection _collection = new PropertyCollection();

            #region IPropertyOwner Members
            public PropertyCollection Properties {
                get { return _collection; }
            }

            #endregion
        }
        #endregion

        [Test]
        [Category.Languages.Core]
        public void ServiceManager_Test01() {
            PropertyOwner propertyOwner = new PropertyOwner();
            Service1 s1 = new Service1();
            bool added = false;
            bool removed = false;

            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, null, s => { added = true; });

            // Verify notifications not sent out when advising
            added.Should().BeFalse();

            // Verify notifications not sent out after adding other service
            ServiceManager.AddService<Service1>(s1, propertyOwner, null);
            added.Should().BeFalse();

            // Verify added notification sent out after adding this service
            ServiceManager.AddService<IService1>(s1, propertyOwner, null);
            added.Should().BeTrue();

            added = false;
            ServiceManager.AdviseServiceRemoved<IService1>(propertyOwner, null, s => { removed = true; });

            // Verify notifications not sent out after removing other service
            ServiceManager.RemoveService<Service1>(propertyOwner);
            removed.Should().BeFalse();

            // Verify removed notification sent out after adding this service
            ServiceManager.RemoveService<IService1>(propertyOwner);
            removed.Should().BeTrue();

            // Verify we aren't still listening to advised events
            ServiceManager.AddService<IService1>(s1, propertyOwner, null);
            added.Should().BeFalse();

            // Verify notification sent out when advising to existing service
            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, null, s => { added = true; });
            added.Should().BeTrue();
        }

        [Test]
        [Category.Languages.Core]
        public void ServiceManager_Test02() {
            PropertyOwner propertyOwner = new PropertyOwner();
            int servicesAdded = 0;
            int servicesRemoved = 0;

            Service1 s1 = new Service1();
            Service2 s2 = new Service2();

            ServiceManager.AddService<IService1>(s1, propertyOwner, null);

            ServiceManager.GetService<IService1>(propertyOwner).Should().Be(s1);
            ServiceManager.GetService<Service1>(propertyOwner).Should().Be(s1);

            ServiceManager.RemoveService<IService1>(propertyOwner);
            ServiceManager.GetService<IService1>(propertyOwner).Should().BeNull();

            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner, null);
            sm.Should().NotBeNull();

            EventHandler<ServiceManagerEventArgs> onServiceAdded = (object sender, ServiceManagerEventArgs e) => {
                servicesAdded++;

                switch (servicesAdded) {
                    case 1:
                        e.Service.Should().Be(s1);
                        e.ServiceType.Should().Be(typeof(IService1));
                        break;
                    case 2:
                        e.Service.Should().Be(s2);
                        e.ServiceType.Should().Be(typeof(IService2));
                        break;
                }
            };

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = (object sender, ServiceManagerEventArgs e) => {
                servicesRemoved++;

                switch (servicesRemoved) {
                    case 1:
                        e.ServiceType.Should().Be(typeof(IService1));
                        break;
                    case 2:
                        e.ServiceType.Should().Be(typeof(IService2));
                        break;
                }
            };

            sm.ServiceAdded += onServiceAdded;
            sm.ServiceRemoved += onServiceRemoved;

            ServiceManager.AddService<IService1>(s1, propertyOwner, null);
            ServiceManager.AddService<IService2>(s2, propertyOwner, null);

            ServiceManager.RemoveService<IService1>(propertyOwner);
            ServiceManager.RemoveService<IService2>(propertyOwner);

            ServiceManager.GetService<IService1>(propertyOwner).Should().BeNull();
            ServiceManager.GetService<IService2>(propertyOwner).Should().BeNull();
        }
    }
}