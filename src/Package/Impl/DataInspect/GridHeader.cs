// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [DataContract]
    public class GridHeader {
        [DataMember(Name = "headers")]
        public ReadOnlyCollection<string> Headers { get; private set; }
    }
}
