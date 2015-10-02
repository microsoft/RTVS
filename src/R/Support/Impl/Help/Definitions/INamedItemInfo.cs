namespace Microsoft.R.Support.Help.Definitions
{
    /// <summary>
    /// Represents generic item that has name and description.
    /// Primarily used in intellisense where name appear in the
    /// completion list and description is shows as a tooltip.
    /// </summary>
    public interface INamedItemInfo
    {
        /// <summary>
        /// Type of the item (function, constant)
        /// </summary>
        NamedItemType ItemType { get; }
        
        /// <summary>
        /// Item name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Item description
        /// </summary>
        string Description { get; }
    }
}
