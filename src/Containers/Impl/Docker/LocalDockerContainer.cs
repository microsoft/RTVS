// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public class LocalDockerContainer : IContainer {
        private static readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private static readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);

        public string Id { get; }
        public string Name { get; }

        public bool IsShortId => Id.Length < 64;

        public IEnumerable<int> HostPorts { get; }

        internal LocalDockerContainer(JToken containerObject) {
            Id = (string)containerObject["Id"];
            Name = GetContainerName(containerObject);
            HostPorts = GetHostPorts(containerObject);
        }

        private IEnumerable<int> GetHostPorts(dynamic containerObject) {
            var hostPorts = new List<int>();
            try {
                dynamic portMap = containerObject.NetworkSettings.Ports["5444/tcp"];
                foreach (dynamic pm in portMap) {
                    if (int.TryParse(pm.HostPort.Value, out int port)) {
                        hostPorts.Add(port);
                    }
                }
            } catch (Exception ex) when (ex.IsCriticalException()) {
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
