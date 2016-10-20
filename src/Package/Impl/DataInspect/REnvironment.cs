// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironment : IREnvironment {
        public static readonly REnvironment Error =
            new REnvironment(Resources.VariableExplorer_ErrorEnvironment, null, REnvironmentKind.Error);

        public REnvironment(IRStackFrame frame)
            : this(frame.CallingFrame.Call, frame.EnvironmentExpression, REnvironmentKind.Function) { 
        }

        public REnvironment(string name)
            : this(name, Invariant($"as.environment({name.ToRStringLiteral()})"), GetKind(name)) { 
        }

        private static REnvironmentKind GetKind(string name) {
            if (name == ".GlobalEnv") {
                return REnvironmentKind.Global;
            } else if (name.StartsWithIgnoreCase("package:")) {
                return REnvironmentKind.Package;
            } else {
                return REnvironmentKind.Unknown;
            }
        }

        private REnvironment(string name, string environmentExpression, REnvironmentKind kind) {
            Name = name;
            EnvironmentExpression = environmentExpression;
            Kind = kind;
        }

        public string Name { get; }

        public string EnvironmentExpression { get; }

        public REnvironmentKind Kind { get; }
    }
}
