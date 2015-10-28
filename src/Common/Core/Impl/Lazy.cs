using System;
using System.Threading;

namespace Microsoft.Common.Core {
    public static class Lazy {
        public static Lazy<T> Create<T>(Func<T> valueFactory) {
            return new Lazy<T>(valueFactory);
        }

        public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe) {
            return new Lazy<T>(valueFactory, isThreadSafe);
        }

        public static Lazy<T> Create<T>(Func<T> valueFactory, LazyThreadSafetyMode mode) {
            return new Lazy<T>(valueFactory, mode);
        }
    }
}
