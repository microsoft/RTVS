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
        public virtual string Description { get; internal set; }

        public NamedItemInfo(): 
            this(null)
        {
        }
        public NamedItemInfo(string name): 
            this(name, string.Empty)
        {
        }

        public NamedItemInfo(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
