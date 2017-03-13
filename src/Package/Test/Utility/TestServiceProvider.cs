// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Test.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class TestServiceProvider : OLE.Interop.IServiceProvider, System.IServiceProvider {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public TestServiceProvider() {
            _services.Add(typeof(SVsRegisterProjectTypes), new VsRegisterProjectGeneratorsMock());
            _services.Add(typeof(SVsRegisterEditors), VsRegisterEditorsMock.Create());
            _services.Add(typeof(IMenuCommandService), new MenuCommandServiceMock());
            _services.Add(typeof(SComponentModel), new ComponentModelMock(VsTestCompositionCatalog.Current));
            _services.Add(typeof(SVsTextManager), new TextManagerMock());
            _services.Add(typeof(SVsImageService), VsImageServiceMock.Create());
            _services.Add(typeof(SVsUIShell), new VsUiShellMock());
            _services.Add(typeof(SOleComponentManager), OleComponentManagerMock.Create());
            _services.Add(typeof(SVsSettingsManager), VsSettingsManagerMock.Create());
        }

        public object GetService(Type serviceType) {
            object service = null;
            _services.TryGetValue(serviceType, out service);
            if (service == null) {
                Guid g = serviceType.GUID;
                if (g != Guid.Empty) {
                    Type t = _services.Keys.FirstOrDefault(x => x.GUID == g);
                    if (t != null) {
                        return _services[t];
                    }
                }
            }
            return service;
        }

        #region OLE.Interop.IServiceProvider
        public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject) {
            // OLE service retrieval should not be normally used
            throw new NotImplementedException();
        }
        #endregion
    }
}
