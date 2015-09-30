using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    public sealed class TestServiceProvider
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public TestServiceProvider()
        {
            _services.Add(typeof(SVsRegisterProjectTypes), new VsRegisterProjectGeneratorsMock());
            _services.Add(typeof(SVsRegisterEditors), new VsRegisterEditorsMock());
            _services.Add(typeof(IMenuCommandService), new MenuCommandServiceMock());
            _services.Add(typeof(SComponentModel), new ComponentModelMock());
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            _services.TryGetValue(serviceType, out service);

            return service;
        }
    }
}
