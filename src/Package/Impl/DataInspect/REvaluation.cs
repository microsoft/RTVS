using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.R.Controls
{
    [DataContract]
    public class REvaluation
    {
        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name = "class")]
        public string ClassName { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "type")]
        public string TypeName { get; set; }
    }
}
