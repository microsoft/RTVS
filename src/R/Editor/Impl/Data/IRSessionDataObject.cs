using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Editor.Data {
    public interface IRSessionDataObject {
        /// <summary>
        /// Index returned from the evaluation provider. 
        /// DebugEvaluationResult returns in ascending order
        /// </summary>
        int Index { get; }

        string Name { get; }

        string Value { get; }

        string ValueDetail { get; }

        string TypeName { get; }

        string Class { get; }

        bool HasChildren { get; }

        IReadOnlyList<int> Dimensions { get; }

        bool IsHidden { get; }

        string Expression { get; }

        bool CanShowDetail { get; }

        Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsync();
    }
}
