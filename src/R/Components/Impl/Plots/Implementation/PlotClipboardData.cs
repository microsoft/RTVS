// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots.Implementation {
    [Serializable]
    internal class PlotClipboardData {
        public Guid PlotId { get; }
        public Guid DeviceId { get; }
        public bool Cut { get; }

        public const string Format = "RPlotRef";

        public PlotClipboardData(Guid deviceId, Guid plotId, bool cut) {
            DeviceId = deviceId;
            PlotId = plotId;
            Cut = cut;
        }

        // Using .NET serialization wasn't working, error unable to locate the assembly that contained the type
        // so this is using 'manual' serialization and storing a string type in the clipboard instead.
        public static PlotClipboardData Parse(string text) {
            var parts = text.Split('|');
            if (parts.Length == 3) {
                Guid deviceId;
                if (!Guid.TryParse(parts[0], out deviceId)) {
                    return null;
                }

                Guid plotId;
                if (!Guid.TryParse(parts[1], out plotId)) {
                    return null;
                }

                bool cut;
                if (!bool.TryParse(parts[2], out cut)) {
                    return null;
                }

                return new PlotClipboardData(deviceId, plotId, cut);
            }

            return null;
        }

        public override string ToString() {
            return $"{DeviceId}|{PlotId}|{Cut}";
        }
    }
}
