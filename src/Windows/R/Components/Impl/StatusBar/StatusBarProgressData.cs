// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Components.StatusBar {
    public sealed class StatusBarProgressData {
        public string Message { get; }
        public int Step { get; }

        public StatusBarProgressData(string message, int step) {
            Message = message;
            Step = step;
        }
    }
}
