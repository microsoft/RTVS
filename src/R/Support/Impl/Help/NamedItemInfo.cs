using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help
{
    public class NamedItemInfo: INamedItemInfo
    {
        /// <summary>
        /// Item type: function, constant, package, ...
        /// </summary>
        public NamedItemType ItemType { get; internal set; }

        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Item description
        /// </summary>
        public virtual string Description { get; internal set; }

        public NamedItemInfo(): 
            this(null, NamedItemType.Function)
        {
        }
        public NamedItemInfo(string name, NamedItemType type): 
            this(name, string.Empty, type)
        {
        }

        public NamedItemInfo(string name, string description, NamedItemType type)
        {
            Name = name;
            Description = description;
            ItemType = type;
        }
    }
}
