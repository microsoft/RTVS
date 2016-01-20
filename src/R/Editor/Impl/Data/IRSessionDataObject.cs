using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Editor.Data {
    public interface IRSessionDataObject {
        string Name { get; }

        string Value { get; }

        string ValueDetail { get; }

        string TypeName { get; }

        string Class { get; }

        bool HasChildren { get; }

        IReadOnlyList<int> Dimensions { get; }

        bool IsHidden { get; }

        string Expression { get; }

        Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsync();
    }
}
