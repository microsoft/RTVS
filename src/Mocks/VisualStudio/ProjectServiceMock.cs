using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class ProjectServiceMock : ProjectService {
        public IEnumerable<UnconfiguredProject> LoadedUnconfiguredProjects {
            get {
                throw new NotImplementedException();
            }
        }

        public IImmutableSet<string> ServiceCapabilities {
            get {
                throw new NotImplementedException();
            }
        }

        public IProjectServices Services {
            get {
                throw new NotImplementedException();
            }
        }

        public IComparable Version {
            get {
                throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event EventHandler Changed;

        public bool IsProjectCapabilityPresent(string projectCapability) {
            throw new NotImplementedException();
        }

        public Task<UnconfiguredProject> LoadProjectAsync(System.Xml.XmlReader reader, System.Collections.Immutable.IImmutableSet<string> projectCapabilities = null) {
            throw new NotImplementedException();
        }

        public Task<UnconfiguredProject> LoadProjectAsync(string projectLocation, System.Collections.Immutable.IImmutableSet<string> projectCapabilities = null) {
            throw new NotImplementedException();
        }

        public Task UnloadProjectAsync(UnconfiguredProject project) {
            throw new NotImplementedException();
        }
    }
}
