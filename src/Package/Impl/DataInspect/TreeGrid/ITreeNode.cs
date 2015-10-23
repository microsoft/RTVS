using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Represent visual node for ObservableTreeNode
    /// </summary>
    public interface ITreeNode
    {
        /// <summary>
        /// Node's Content
        /// </summary>
        object Content { get; set; }

        /// <summary>
        /// Evaluate if the content can be updated to new content
        /// </summary>
        /// <param name="node">new content</param>
        /// <returns>true if can updated, false otherwise</returns>
        bool CanUpdateTo(ITreeNode node);

        /// <summary>
        /// true if this node can have children, false otherwise
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// returns children nodes
        /// </summary>
        /// <returns>Children node collection</returns>
        Task<IReadOnlyList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken);
    }
}
