using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions
{
    public sealed class ArgumentInfo: NamedItemInfo, IArgumentInfo
    {
        /// <summary>
        /// Default argument value
        /// </summary>
        public string DefaultValue { get; internal set; }

        /// <summary>
        /// True if argument can be omitted
        /// </summary>
        public bool IsOptional { get; internal set; }

        /// <summary>
        /// True if argument is '...'
        /// </summary>
        public bool IsEllipsis { get; internal set; }

        public ArgumentInfo(string name) :
            base(name, NamedItemType.Parameter)
        {
        }

        public ArgumentInfo(string name, string description):
            base(name, description, NamedItemType.Parameter)
        {
        }
    }
}
