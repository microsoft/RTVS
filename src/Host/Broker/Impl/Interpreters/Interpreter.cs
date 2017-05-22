// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class Interpreter {
        public InterpreterManager Manager { get; }

        public string Id { get; }

        public string Name { get; }

        public string Path { get; }

        public string BinPath { get; }

        public Version Version { get; }

        public InterpreterInfo Info => new InterpreterInfo {
            Id = Id,
            Name = Name,
            Path = Path,
            BinPath = BinPath,
            Version = Version
        };

        public IRInterpreterInfo RInterpreterInfo { get; }

        public Interpreter(InterpreterManager manager, string id, IRInterpreterInfo rInterpreterInfo) {
            Manager = manager;
            Id = id;
            Name = rInterpreterInfo.Name;
            Path = rInterpreterInfo.InstallPath;
            BinPath = rInterpreterInfo.BinPath;
            Version = rInterpreterInfo.Version;
            RInterpreterInfo = rInterpreterInfo;
        }

        public Interpreter(InterpreterManager manager, string id, string name, IRInterpreterInfo rInterpreterInfo) {
            Manager = manager;
            Id = id;
            Name = name;
            Path = rInterpreterInfo.InstallPath;
            BinPath = rInterpreterInfo.BinPath;
            Version = rInterpreterInfo.Version;
            RInterpreterInfo = rInterpreterInfo;
        }
    }
}
