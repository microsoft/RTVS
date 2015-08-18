
namespace Microsoft.R.Support.Help.Functions
{
    public sealed class ArgumentInfo: NamedItemInfo
    {
        /// <summary>
        /// Default argument value
        /// </summary>
        public string DefaultValue { get; internal set; }

        /// <summary>
        /// True if argument can be omitted
        /// </summary>
        public bool IsOptional { get; internal set; }
    }
}
