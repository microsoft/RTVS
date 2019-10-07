// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// #define WAIT_FOR_DEBUGGER

using System;

namespace Microsoft.R.Host.Broker.Start {
    public sealed class Program : ProgramBase {
        public static void Main(string[] args) {
#if WAIT_FOR_DEBUGGER
            var start = DateTime.Now;
            while (!System.Diagnostics.Debugger.IsAttached) {
                System.Threading.Thread.Sleep(1000);
                if ((DateTime.Now - start).TotalMilliseconds > 30000) {
                    break;
                }
            }
#endif
            MainEntry<WindowsStartup>(args);
        }
        
    }
}
