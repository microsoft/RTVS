// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// grid data provider to control
    /// </summary>
    internal sealed class GridDataProvider : IGridProvider<string> {
        private readonly IRSessionDataObject _dataObject;
        private readonly GridDataSource _dataSource;

        public GridDataProvider(IRSession session, IRSessionDataObject dataObject) {
            _dataObject = dataObject;
            _dataSource = new GridDataSource(session);

            RowCount = dataObject.Dimensions[0];
            ColumnCount = dataObject.Dimensions.Count >= 2 ? dataObject.Dimensions[1] : 1;
            CanSort = true;

            // Lists cannot be sorted, except when the list is a dataframe.
            if (dataObject.TypeName == "list") {
                var er = dataObject.DebugEvaluation as IRValueInfo;
                CanSort = er?.Classes.Contains("data.frame") == true;
            }
        }

        public long ColumnCount { get; }

        public long RowCount { get; }

        public bool CanSort { get; }

        public Task<IGridData<string>> GetAsync(GridRange gridRange, ISortOrder sortOrder = null)
            => _dataSource.GetGridDataAsync(_dataObject.Expression, gridRange, sortOrder);
    }
}
