// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Package.Definitions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    [ExcludeFromCodeCoverage]
    internal sealed class TestRPackage : IRPackage
    {
        TestServiceProvider _serviceProvider = new TestServiceProvider();

        public T FindWindowPane<T>(Type t, int id, bool create) where T : ToolWindowPane {
            return new ToolWindowPaneMock(this) as T;
        }

        public void LoadSettings() { }

        public DialogPage GetDialogPage(Type t) {
            throw new NotImplementedException();
        }

        public T GetPackageService<T>(Type t = null) where T : class {
            return _serviceProvider.GetService(t) as T;
        }

        public object GetService(Type t) {
            return _serviceProvider.GetService(t);
        }
    }
}
