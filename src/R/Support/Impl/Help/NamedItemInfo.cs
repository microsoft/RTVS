using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help {
    public class NamedItemInfo : INamedItemInfo {
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

        /// <summary>
        /// If item name is an alias this field provides
        /// actual item name
        /// </summary>
        public string ActualName { get; internal set; }

        public NamedItemInfo() :
            this(null, NamedItemType.Function) {
        }
        public NamedItemInfo(string name, NamedItemType type) :
            this(name, string.Empty, type) {
        }

        public NamedItemInfo(string name, string description, NamedItemType type) :
            this(name, name, description, type) {
        }

        public NamedItemInfo(string name, string actualName, string description, NamedItemType type) {
            Name = name;
            ActualName = actualName;
            Description = description;
            ItemType = type;
        }
    }
}
