using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.R.Package.DataInspect
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

        [DataMember(Name = "length")]
        public int Length { get; set; }

        [DataMember(Name = "children")]
        public REvaluationRange Children { get; set; }
    }

    [DataContract]
    public class REvaluationRange
    {
        [DataMember(Name ="total")]
        public int Total { get; set; }

        [DataMember(Name ="begin")]
        public int Begin { get; set; }

        [DataMember(Name = "end")]
        public int End { get; set; }

        [DataMember(Name = "variables")]
        public List<REvaluation> Evals { get; set; }
    }
}
