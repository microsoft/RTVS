using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// <see cref="IGrid{T}"/> provider
    /// </summary>
    /// <typeparam name="TData">the type of grid item</typeparam>
    public interface IGridProvider<TData> {
        int RowCount { get; }

        int ColumnCount { get; }

        Task<IGrid<TData>> GetRangeAsync(GridRange gridRange);
    }
}
