namespace Microsoft.R.Support.Help.Definitions {
    public interface IArgumentInfo : INamedItemInfo {
        /// <summary>
        /// Default argument value
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// True if argument can be omitted
        /// </summary>
        bool IsOptional { get; }

        /// <summary>
        /// Trie if argument is the '...' argument
        /// </summary>
        bool IsEllipsis { get; }
    }
}
