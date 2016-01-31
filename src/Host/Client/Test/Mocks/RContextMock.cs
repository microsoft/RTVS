namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RContextMock : IRContext {
        public RContextType CallFlag { get; set; } = RContextType.TopLevel;
    }
}
