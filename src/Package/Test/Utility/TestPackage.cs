using System;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    internal sealed class TestRPackage : RPackage
    {
        TestServiceProvider _serviceProvider = new TestServiceProvider();

        public void Init()
        {
            base.Initialize();
        }

        protected override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
