namespace Microsoft.R.Core.AST.Arguments
{
    /// <summary>
    /// Represents missing argument like in a[,1]. 
    /// </summary>
    public sealed class MissingArgument : CommaSeparatedItem
    {
        public override string ToString()
        {
            return "{Missing}";
        }
    }
}
