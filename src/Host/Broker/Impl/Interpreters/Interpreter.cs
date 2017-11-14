// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Platform.Interpreters;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class Interpreter {
        public string Id { get; }

        public string Name { get; }

        public string InstallPath => RInterpreterInfo.InstallPath;

        public string BinPath => RInterpreterInfo.BinPath;

        public string LibPath => RInterpreterInfo.LibPath;

        public Version Version => RInterpreterInfo.Version;

        public IRInterpreterInfo RInterpreterInfo { get; }

        public Interpreter(string id, IRInterpreterInfo rInterpreterInfo) :
            this(id, rInterpreterInfo.Name, rInterpreterInfo) { }

        public Interpreter(string id, string name, IRInterpreterInfo rInterpreterInfo) {
            Id = id;
            Name = name;
            RInterpreterInfo = rInterpreterInfo;
        }
    }
}
