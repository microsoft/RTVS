using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [DataContract]
    public class GridHeader {
        [DataMember(Name = "headers")]
        public ReadOnlyCollection<string> Headers { get; private set; }
    }
}
