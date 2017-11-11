// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public class LocalDockerContainer : IContainer {
        public string Id { get; }
        public string Name { get; }
        /// <summary>
        /// Possible values for Docker Container:
        /// "created", "restarting", "running", "removing", "paused", "exited", "dead"
        /// </summary>
        public string Status { get; }
        public bool IsRunning => Status.EqualsOrdinal("running");
        public bool IsShortId => Id.Length < 64;

        public IEnumerable<int> HostPorts { get; }

        internal LocalDockerContainer(JToken containerObject) {
            Id = (string)containerObject["Id"];
            Name = GetContainerName(containerObject);
            HostPorts = GetHostPorts(containerObject);
            Status = GetContainerStatus(containerObject);
        }

        private string GetContainerStatus(JToken containerObject) {
            return ((dynamic)containerObject).State.Status;
        }

        private IEnumerable<int> GetHostPorts(JToken containerObject) {
            var hostPorts = new List<int>();
            try {
                dynamic portMap = ((dynamic)containerObject).HostConfig.PortBindings["5444/tcp"];
                if (portMap != null) {
                    foreach (dynamic pm in portMap) {
                        if (int.TryParse(pm.HostPort.Value, out int port)) {
                            hostPorts.Add(port);
                        }
                    }
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                // RuntimeBinderException can occur if a container was not port mapped.
            }
            return hostPorts;
        }

        private string GetContainerName(JToken containerObj) {
            var name = (string)containerObj["Name"];
            return (name.StartsWithIgnoreCase("/") ? name.Substring(1) : name);
        }
    }
}
