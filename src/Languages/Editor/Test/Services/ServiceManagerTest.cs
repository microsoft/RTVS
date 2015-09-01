using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Services
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceManagerTest
    {
        #region Mock Services
        interface IService1
        {
            void DoSomething();
        }

        [ExcludeFromCodeCoverage]
        class Service1 : IService1
        {
            public void DoSomething() { }
        }

        interface IService2
        {
            void DoSomethingElse();
        }

        [ExcludeFromCodeCoverage]
        class Service2 : IService2
        {
            public void DoSomethingElse() { }
        }
        #endregion

        #region Mock Property Owner
        [ExcludeFromCodeCoverage]
        class PropertyOwner : IPropertyOwner
        {
            PropertyCollection _collection = new PropertyCollection();

            #region IPropertyOwner Members
            public PropertyCollection Properties
            {
                get { return _collection; }
            }

            #endregion
        }
        #endregion

        [TestMethod]
        public void ServiceManager_Test01()
        {
            PropertyOwner propertyOwner = new PropertyOwner();
            Service1 s1 = new Service1();
            bool added = false;
            bool removed = false;

            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, s => { added = true; });

            // Verify notifications not sent out when advising
            Assert.IsFalse(added);

            // Verify notifications not sent out after adding other service
            ServiceManager.AddService<Service1>(s1, propertyOwner);
            Assert.IsFalse(added);

            // Verify added notification sent out after adding this service
            ServiceManager.AddService<IService1>(s1, propertyOwner);
            Assert.IsTrue(added);

            added = false;
            ServiceManager.AdviseServiceRemoved<IService1>(propertyOwner, s => { removed = true; });

            // Verify notifications not sent out after removing other service
            ServiceManager.RemoveService<Service1>(propertyOwner);
            Assert.IsFalse(removed);

            // Verify removed notification sent out after adding this service
            ServiceManager.RemoveService<IService1>(propertyOwner);
            Assert.IsTrue(removed);

            // Verify we aren't still listening to advised events
            ServiceManager.AddService<IService1>(s1, propertyOwner);
            Assert.IsFalse(added);

            // Verify notification sent out when advising to existing service
            ServiceManager.AdviseServiceAdded<IService1>(propertyOwner, s => { added = true; });
            Assert.IsTrue(added);
        }

        [TestMethod]
        public void ServiceManager_Test02()
        {
            PropertyOwner propertyOwner = new PropertyOwner();
            int servicesAdded = 0;
            int servicesRemoved = 0;

            Service1 s1 = new Service1();
            Service2 s2 = new Service2();

            ServiceManager.AddService<IService1>(s1, propertyOwner);

            Assert.AreEqual(s1 as IService1, ServiceManager.GetService<IService1>(propertyOwner));
            Assert.AreEqual(s1, ServiceManager.GetService<Service1>(propertyOwner));
            Assert.IsNotNull(ServiceManager.GetService<Service1>(propertyOwner));

            ServiceManager.RemoveService<IService1>(propertyOwner);
            Assert.IsNull(ServiceManager.GetService<IService1>(propertyOwner));

            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Assert.IsNotNull(sm);

            EventHandler<ServiceManagerEventArgs> onServiceAdded = (object sender, ServiceManagerEventArgs e) =>
            {
                servicesAdded++;

                if (servicesAdded == 1)
                {
                    Assert.AreEqual(s1, e.Service);
                    Assert.AreEqual(typeof(IService1), e.ServiceType);
                }
                else if (servicesAdded == 2)
                {
                    Assert.AreEqual(s2, e.Service);
                    Assert.AreEqual(typeof(IService2), e.ServiceType);
                }
            };

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = (object sender, ServiceManagerEventArgs e) =>
            {
                servicesRemoved++;

                if (servicesRemoved == 1)
                {
                    Assert.AreEqual(typeof(IService1), e.ServiceType);
                }
                else if (servicesRemoved == 2)
                {
                    Assert.AreEqual(typeof(IService2), e.ServiceType);
                }
            };

            sm.ServiceAdded += onServiceAdded;
            sm.ServiceRemoved += onServiceRemoved;

            ServiceManager.AddService<IService1>(s1, propertyOwner);
            ServiceManager.AddService<IService2>(s2, propertyOwner);

            ServiceManager.RemoveService<IService1>(propertyOwner);
            ServiceManager.RemoveService<IService2>(propertyOwner);

            Assert.IsNull(ServiceManager.GetService<IService1>(propertyOwner));
            Assert.IsNull(ServiceManager.GetService<IService2>(propertyOwner));
        }
    }
}