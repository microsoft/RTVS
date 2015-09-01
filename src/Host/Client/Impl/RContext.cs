namespace Microsoft.R.Host.Client
{
    internal class RContext : IRContext
    {
        public RContext(RContextType callFlag)
        {
            CallFlag = callFlag;
        }

        public RContextType CallFlag { get; }
    }
}