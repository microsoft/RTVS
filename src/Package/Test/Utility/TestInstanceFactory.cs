using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    [ExcludeFromCodeCoverage]
    internal sealed class TestInstanceFactory : IObjectInstanceFactory
    {
        public object CreateInstance(ref Guid clsid, ref Guid riid, Type objectType)
        {
            if (objectType == typeof(IVsTextLines))
            {
                return new VsTextLinesMock();
            }
            else if (objectType == typeof(IVsTextBuffer))
            {
                return new VsTextBufferMock();
            }

            Assert.Fail("Don't know how to create instance of " + objectType.FullName);
            return null;
        }
    }
}
