// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Start {
    public class Program : ProgramBase {
        public static void Main(string[] args) => MainEntry<UnixStartup>(args);
    }
}