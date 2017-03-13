// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IProjectServiceAccessor))]
    public sealed class ProjectServiceAccessorMock : IProjectServiceAccessor {
        private Lazy<IProjectService> _projectServiceMock = new Lazy<IProjectService>(() => new ProjectServiceMock());
        public IProjectService GetProjectService(ProjectServiceThreadingModel threadingModel = ProjectServiceThreadingModel.Multithreaded) {
            return _projectServiceMock.Value;
        }
    }
}
