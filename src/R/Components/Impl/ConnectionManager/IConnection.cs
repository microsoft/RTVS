// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnection {
        Uri Id { get; }
        string Name { get; }
        string Path { get; }
        bool IsRemote { get; }
        DateTime TimeStamp { get; }
        string RCommandLineArguments { get; }
    }
}