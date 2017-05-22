// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    internal static class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(this IRSession rSession, string expression, GridRange? gridRange, ISortOrder sortOrder = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            string rows = gridRange?.Rows.ToRString();
            string columns = gridRange?.Columns.ToRString();
            string rowSelector = (sortOrder != null && !sortOrder.IsEmpty) ? sortOrder.GetRowSelector() : "";
            string expr = Invariant($"rtvs:::grid_data({expression}, {rows}, {columns}, {rowSelector})");

            try {
                return await rSession.EvaluateAsync<GridData>(expr, REvaluationKind.Normal);
            } catch (RException ex) {
                var message = Invariant($"Grid data evaluation failed:{Environment.NewLine}{ex.Message}");
                VsAppShell.Current.Log().Write(LogVerbosity.Normal, MessageCategory.Error, message);
                return null;
            }
        }
    }
}
