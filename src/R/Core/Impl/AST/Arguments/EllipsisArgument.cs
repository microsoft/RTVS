namespace Microsoft.R.Core.AST.Arguments
{
    /// <summary>
    /// Represents '...' argument. Normally it is the last 
    /// one in the function definition.
    /// </summary>
    public sealed class EllipsisArgument : CommaSeparatedItem
    {
        public override string ToString()
        {
            return "...";
        }
    }
}
