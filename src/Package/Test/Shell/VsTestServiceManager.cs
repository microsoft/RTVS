// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    internal sealed class VsTestServiceManager: TestServiceManager {
        private readonly TestServiceProvider _sp;

        public VsTestServiceManager(ExportProvider exportProvider) : base(exportProvider) {
            _sp = new TestServiceProvider();
        }

        public override T GetService<T>(Type type = null) {
            var service = _sp.GetService(type ?? typeof(T)) as T;
            return service ?? base.GetService<T>(type);
        }
    }
}
