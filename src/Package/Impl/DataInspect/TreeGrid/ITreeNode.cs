using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    /// <summary>
    /// Represent visual node for ObservableTreeNode
    /// </summary>
    public interface ITreeNode
    {
        object Content { get; set; }

        bool IsSame(ITreeNode node);

        Task<IList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken);
    }
}
