// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public class ContainerImage {
        public string Id { get; }
        public string Name { get; }
        public string Tag { get; }

        public ContainerImage(string id, string name, string tag) {
            Id = id;
            Name = name;
            Tag = tag;
        }

        public void Deconstruct(out string id, out string name, out string tag) {
            id = Id;
            name = Name;
            tag = Tag;
        }
    }
}
