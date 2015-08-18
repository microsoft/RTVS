using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help
{
    public class NamedItemInfo: INamedItemInfo
    {
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Item description
        /// </summary>
        public string Description { get; internal set; }
    }
}
