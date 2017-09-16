// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

#define WAIT_FOR_DEBUGGER

using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Common.Core;

namespace Microsoft.R.LanguageServer.Server {
    internal static class Program {
        public static void Main(string[] args) {
            var debugMode = CheckDebugMode(args);
            using (Session.Create()) {
                var connection = new VsCodeConnection();
                connection.Connect(Session.Current.Services, debugMode);
            }
        }

        private static bool CheckDebugMode(string[] args) {
            var debugMode = args.Any(a => a.EqualsOrdinal("--debug"));
            if (debugMode) {
#if WAIT_FOR_DEBUGGER
                while (!Debugger.IsAttached) {
                    Thread.Sleep(1000);
                }
#endif
            }
            return debugMode;
        }
    }
}