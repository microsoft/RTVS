// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public class LocalDockerImage : IContainerImage {
        public string Id { get; }

        public string Name { get; }

        public string Tag { get; }

        internal LocalDockerImage(JToken imageObject) {
            var obj = (dynamic)imageObject;
            Id = GetId((string)obj.Id);
            var nametags = (JArray)obj.RepoTags;
            if (nametags.Any()) {
                var nametag = (string)obj.RepoTags[0];
                Name = GetName(nametag);
                Tag = GetTag(nametag);
            }
        }

        private string GetId(string id) {
            int idx = id.IndexOf(':');
            return idx < 0 ? id : id.Substring(idx + 1);
        }

        private string GetName(string nametag) {
            string[] split = nametag.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            return split[0];
        }

        private string GetTag(string nametag) {
            string[] split = nametag.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            return split.Length == 2 ? split[1] : string.Empty;
        }
    }
}
