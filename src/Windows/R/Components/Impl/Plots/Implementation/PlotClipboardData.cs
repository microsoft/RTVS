// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Common.Core.Json;
using Newtonsoft.Json;

namespace Microsoft.R.Components.Plots.Implementation {
    [Serializable]
    internal class PlotClipboardData {
        private const string Format = "RPlotRef";

        public Guid PlotId { get; }
        public Guid DeviceId { get; }
        public bool Cut { get; }

        public PlotClipboardData(Guid deviceId, Guid plotId, bool cut) {
            DeviceId = deviceId;
            PlotId = plotId;
            Cut = cut;
        }

        public static IEnumerable<PlotClipboardData> FromDataObject(IDataObject dataObject) {
            IEnumerable<PlotClipboardData> result = null;
            if (dataObject != null && dataObject.GetDataPresent(Format)) {
                var cbd = dataObject.GetData(Format) as string[];
                result = cbd?.Select(Parse);
            }
            return result ?? Enumerable.Empty<PlotClipboardData>();
        }

        public static IDataObject ToDataObject(IEnumerable<IRPlot> plots)
            => new DataObject(Format, Serialize(plots));

        public static IDataObject ToDataObject(Guid deviceId, Guid plotId)
            => new DataObject(Format, Serialize(deviceId, plotId));

        public static IEnumerable<PlotClipboardData> FromClipboard()
            => FromDataObject(Clipboard.GetDataObject());

        public static void ToClipboard(IEnumerable<IRPlot> plots, bool cut = false) {
            var data = Serialize(plots, cut);
            Clipboard.Clear();
            Clipboard.SetData(Format, data);
        }

        public static void ToClipboard(Guid deviceId, Guid plotId, bool cut = false) {
            var data = Serialize(deviceId, plotId, cut);
            Clipboard.Clear();
            Clipboard.SetData(Format, data);
        }

        public static bool IsClipboardDataAvailable()
            => Clipboard.GetDataObject()?.GetDataPresent(Format) ?? false;

        private static string[] Serialize(IEnumerable<IRPlot> plots, bool cut = false)
            => plots.Select(p => Serialize(new PlotClipboardData(p.ParentDevice.DeviceId, p.PlotId, cut))).ToArray();

        private static string[] Serialize(Guid deviceId, Guid plotId, bool cut = false)
            => new[] { Serialize(new PlotClipboardData(deviceId, plotId, cut)) };

        private static string Serialize(PlotClipboardData data) => JsonConvert.SerializeObject(data);

        private static PlotClipboardData Parse(string text) {
            try {
                return Json.DeserializeObject<PlotClipboardData>(text);
            } catch (JsonReaderException) {
                return null;
            }
        }
    }
}
