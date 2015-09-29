namespace Microsoft.R.Host.Client
{
    internal class RContext : IRContext
    {
        protected bool Equals(RContext other)
        {
            return other != null && CallFlag == other.CallFlag;
        }

        public RContext(RContextType callFlag)
        {
            CallFlag = callFlag;
        }

        public RContextType CallFlag { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as RContext);
        }

        public override int GetHashCode()
        {
            return (int)CallFlag;
        }
    }
}