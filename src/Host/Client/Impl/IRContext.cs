namespace Microsoft.R.Host.Client
{
    /// <summary>
    /// Representation of <c>struct RCTXT</c> in R.
    /// </summary>
    public interface IRContext
    {
        RContextType CallFlag { get; }
    }
}
