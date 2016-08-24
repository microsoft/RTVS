// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots.Implementation {
    [Serializable]
    internal class PlotClipboardData {
        public Guid PlotId { get; }
        public Guid DeviceId { get; }
        public int? ProcessId { get; }
        public bool Cut { get; }

        public const string Format = "RPlotRef";

        public PlotClipboardData(Guid deviceId, Guid plotId, int? processId, bool cut) {
            DeviceId = deviceId;
            PlotId = plotId;
            ProcessId = processId;
            Cut = cut;
        }

        // Using .NET serialization wasn't working, error unable to locate the assembly that contained the type
        // so this is using 'manual' serialization and storing a string type in the clipboard instead.
        public static PlotClipboardData Parse(string text) {
            var parts = text.Split('|');
            if (parts.Length == 4) {
                Guid deviceId;
                if (!Guid.TryParse(parts[0], out deviceId)) {
                    return null;
                }

                Guid plotId;
                if (!Guid.TryParse(parts[1], out plotId)) {
                    return null;
                }

                int? processId = null;
                if (parts[2].Length > 0) {
                    int id;
                    if (!int.TryParse(parts[2], out id)) {
                        return null;
                    }
                    processId = id;
                }

                bool cut;
                if (!bool.TryParse(parts[3], out cut)) {
                    return null;
                }

                return new PlotClipboardData(deviceId, plotId, processId, cut);
            }

            return null;
        }

        public override string ToString() {
            var process = ProcessId.HasValue ? ProcessId.Value.ToString() : "";
            return $"{DeviceId}|{PlotId}|{process}|{Cut}";
        }
    }
}
