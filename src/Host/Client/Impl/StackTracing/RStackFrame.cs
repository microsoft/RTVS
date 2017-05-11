// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.StackTracing {
    internal sealed class RStackFrame : IRStackFrame {

        public IRSession Session { get; }

        public int Index { get; }

        public string EnvironmentExpression => Invariant($"base::sys.frame({Index})");

        public string EnvironmentName { get; }

        public IRStackFrame CallingFrame { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public string Call { get; }

        public bool IsGlobal => EnvironmentName == "<environment: R_GlobalEnv>";

        internal RStackFrame(IRSession session, int index, RStackFrame callingFrame, JObject jFrame) {
            Session = session;
            Index = index;
            CallingFrame = callingFrame;

            FileName = jFrame.Value<string>("filename");
            LineNumber = jFrame.Value<int?>("line_number");
            Call = jFrame.Value<string>("call");
            EnvironmentName = jFrame.Value<string>("env_name");
        }

        public override string ToString() =>
            Invariant($"{EnvironmentName ?? Call ?? "<null>"} at {FileName ?? "<null>"}:{(LineNumber?.ToString(CultureInfo.InvariantCulture) ?? "<null>")}");
    }
}
