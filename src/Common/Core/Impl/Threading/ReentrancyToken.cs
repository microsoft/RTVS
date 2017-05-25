namespace Microsoft.Common.Core.Threading {
    public struct ReentrancyToken {
        private readonly object _source;

        private ReentrancyToken(object source) {
            _source = source;
        }

        internal static IReentrancyTokenFactory<TSource> CreateFactory<TSource>() where TSource : class => new Factory<TSource>();

        private class Factory<TSource> : IReentrancyTokenFactory<TSource> where TSource : class {
            public TSource GetSource(ReentrancyToken token) => token._source as TSource;

            public ReentrancyToken Create(TSource source) => new ReentrancyToken(source);
        }
    }
}