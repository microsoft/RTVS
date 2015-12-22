using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Services;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Utilities;
using Xunit;

namespace Microsoft.Languages.Editor.Tests.Services {
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

        [Fact]
        [Trait("Languages.Editor", "")]
        public void ServiceManager_Test01() {
            PropertyOwner propertyOwner = new PropertyOwner();
            Service1 s1 = new Service1();
            bool added = false;
            bool removed = false;

            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, s => { added = true; });

            // Verify notifications not sent out when advising
            Assert.False(added);

            // Verify notifications not sent out after adding other service
            ServiceManager.AddService<Service1>(s1, propertyOwner);
            Assert.False(added);

            // Verify added notification sent out after adding this service
            ServiceManager.AddService<IService1>(s1, propertyOwner);
            Assert.True(added);

            added = false;
            ServiceManager.AdviseServiceRemoved<IService1>(propertyOwner, s => { removed = true; });

            // Verify notifications not sent out after removing other service
            ServiceManager.RemoveService<Service1>(propertyOwner);
            Assert.False(removed);

            // Verify removed notification sent out after adding this service
            ServiceManager.RemoveService<IService1>(propertyOwner);
            Assert.True(removed);

            // Verify we aren't still listening to advised events
            ServiceManager.AddService<IService1>(s1, propertyOwner);
            Assert.False(added);

            // Verify notification sent out when advising to existing service
            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, s => { added = true; });
            Assert.True(added);
        }

        [Fact]
        [Trait("Languages.Editor", "")]
        public void ServiceManager_Test02() {
            PropertyOwner propertyOwner = new PropertyOwner();
            int servicesAdded = 0;
            int servicesRemoved = 0;

            Service1 s1 = new Service1();
            Service2 s2 = new Service2();

            ServiceManager.AddService<IService1>(s1, propertyOwner);

            Assert.Equal(s1 as IService1, ServiceManager.GetService<IService1>(propertyOwner));
            Assert.Equal(s1, ServiceManager.GetService<Service1>(propertyOwner));
            Assert.NotNull(ServiceManager.GetService<Service1>(propertyOwner));

            ServiceManager.RemoveService<IService1>(propertyOwner);
            Assert.Null(ServiceManager.GetService<IService1>(propertyOwner));

            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Assert.NotNull(sm);

            EventHandler<ServiceManagerEventArgs> onServiceAdded = (object sender, ServiceManagerEventArgs e) => {
                servicesAdded++;

                if (servicesAdded == 1) {
                    Assert.Equal(s1, e.Service);
                    Assert.Equal(typeof(IService1), e.ServiceType);
                } else if (servicesAdded == 2) {
                    Assert.Equal(s2, e.Service);
                    Assert.Equal(typeof(IService2), e.ServiceType);
                }
            };

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = (object sender, ServiceManagerEventArgs e) => {
                servicesRemoved++;

                if (servicesRemoved == 1) {
                    Assert.Equal(typeof(IService1), e.ServiceType);
                } else if (servicesRemoved == 2) {
                    Assert.Equal(typeof(IService2), e.ServiceType);
                }
            };

            sm.ServiceAdded += onServiceAdded;
            sm.ServiceRemoved += onServiceRemoved;

            ServiceManager.AddService<IService1>(s1, propertyOwner);
            ServiceManager.AddService<IService2>(s2, propertyOwner);

            ServiceManager.RemoveService<IService1>(propertyOwner);
            ServiceManager.RemoveService<IService2>(propertyOwner);

            Assert.Null(ServiceManager.GetService<IService1>(propertyOwner));
            Assert.Null(ServiceManager.GetService<IService2>(propertyOwner));
        }
    }
}