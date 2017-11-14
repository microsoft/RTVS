// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class ProjectServiceMock : IProjectService {
        public IProjectCapabilitiesScope Capabilities {
            get {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<UnconfiguredProject> LoadedUnconfiguredProjects {
            get {
                throw new NotImplementedException();
            }
        }

        public IProjectServices Services {
            get {
                throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event EventHandler Changed;

        public bool IsProjectCapabilityPresent(string projectCapability) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<UnconfiguredProject> LoadProjectAsync(System.Xml.XmlReader reader, System.Collections.Immutable.IImmutableSet<string> projectCapabilities = null) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<UnconfiguredProject> LoadProjectAsync(string projectLocation, System.Collections.Immutable.IImmutableSet<string> projectCapabilities = null) {
            throw new NotImplementedException();
        }

        public Task<UnconfiguredProject> LoadProjectAsync(string projectLocation, bool delayAutoLoad, IImmutableSet<string> projectCapabilities = null) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task UnloadProjectAsync(UnconfiguredProject project) {
            throw new NotImplementedException();
        }
    }
}
