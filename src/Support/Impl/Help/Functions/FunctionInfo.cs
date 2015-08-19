using System.Collections.Generic;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions
{
    public sealed class FunctionInfo : NamedItemInfo, IFunctionInfo
    {
        #region INamedItemInfo
        public override string Description
        {
            get { return FunctionIndex.GetFunctionDescription(this.Name); }
        }
        #endregion

        #region IFunctionInfo
        /// <summary>
        /// Other function name variants
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; }

        /// <summary>
        /// Function signatures
        /// </summary>
        public IReadOnlyList<ISignatureInfo> Signatures { get; internal set; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string ReturnValue { get; internal set; }

        /// <summary>
        /// Indicates that function is internal (has 'internal' 
        /// in its list of keywords)
        /// </summary>
        public bool IsInternal { get; internal set; }
        #endregion

        public FunctionInfo(string name, string description) :
            base(name, description)
        {
        }

        public FunctionInfo(string name) :
            base(name)
        {
        }
    }
}
