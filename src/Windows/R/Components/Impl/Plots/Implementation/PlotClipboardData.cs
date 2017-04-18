// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Json;
using Newtonsoft.Json;

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

        public static string Serialize(PlotClipboardData data) {
            return JsonConvert.SerializeObject(data);
        }

        public static PlotClipboardData Parse(string text) {
            try {
                return Json.DeserializeObject<PlotClipboardData>(text);
            } catch (JsonReaderException) {
                return null;
            }
        }
    }
}
