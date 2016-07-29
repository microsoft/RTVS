// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class Interpreter {
        public InterpreterManager Manager { get; }

        public string Id { get; }

        public string Path { get; }

        public string BinPath { get; }

        public Version Version { get; }

        public InterpreterInfo Info => new InterpreterInfo {
            Id = Id,
            Path = Path,
            BinPath = BinPath,
            Version = Version
        };

        public Interpreter(InterpreterManager manager, string id, string path, string binPath, Version version) {
            Manager = manager;
            Id = id;
            Path = path;
            BinPath = binPath;
            Version = version;
        }
    }
}
