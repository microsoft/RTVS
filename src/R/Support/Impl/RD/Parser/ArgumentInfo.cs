
namespace Microsoft.R.Support.RD.Parser
{
    public sealed class ArgumentInfo
    {
        /// <summary>
        /// Argument name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Description of the function argument
        /// </summary>
        public string Description { get; internal set; }

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
