// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SProcMap: IEnumerable<string>  {
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

        public string this[string name] {
            get => _map[name];
            set => _map[name] = value;
        }

        public IEnumerable<string> Scripts => _map.Values;

        public int Count => _map.Count;
        public IEnumerator<string> GetEnumerator()  => _map.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()  => _map.Keys.GetEnumerator();
    }
}
