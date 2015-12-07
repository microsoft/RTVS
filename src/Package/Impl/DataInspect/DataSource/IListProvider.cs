using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public interface IListProvider<TData> {
        int Count { get; }

        Task<IList<TData>> GetRangeAsync(Range range);
    }
}
