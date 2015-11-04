using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    public sealed class TestServiceProvider
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private Dictionary<Guid, object> _guidServices = new Dictionary<Guid, object>();

        public TestServiceProvider()
        {
            _services.Add(typeof(SVsRegisterProjectTypes), new VsRegisterProjectGeneratorsMock());
            _services.Add(typeof(SVsRegisterEditors), new VsRegisterEditorsMock());
            _services.Add(typeof(IMenuCommandService), new MenuCommandServiceMock());
            _services.Add(typeof(SComponentModel), new ComponentModelMock(RPackageTestCompositionCatalog.Current));
            _services.Add(typeof(SVsTextManager), new TextManagerMock());

            _guidServices.Add(typeof(SVsImageService).GUID, new VsImageServiceMock());
            _guidServices.Add(typeof(SVsUIShell).GUID, new VsUiShellMock());
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            _services.TryGetValue(serviceType, out service);
            if(service == null) {
                _guidServices.TryGetValue(serviceType.GUID, out service);
            }
            return service;
        }
    }
}
