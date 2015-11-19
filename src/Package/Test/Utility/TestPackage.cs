using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    [ExcludeFromCodeCoverage]
    internal sealed class TestRPackage : RPackage
    {
        TestServiceProvider _serviceProvider = new TestServiceProvider();

        public void Init()
        {
            base.Initialize();
        }

        public void Close()
        {
            base.Dispose(true);
        }

        protected override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
