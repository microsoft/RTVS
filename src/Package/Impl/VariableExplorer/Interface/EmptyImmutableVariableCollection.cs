using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// Provides a singleton instance of an empty collection.
    /// </summary>
    public sealed class EmptyImmutableVariableCollection : IImmutableVariableCollection {
        /// <summary>
        /// An instance of an empty collection.
        /// </summary>
        public static readonly IImmutableVariableCollection Instance = new EmptyImmutableVariableCollection();

        private EmptyImmutableVariableCollection() { }

        int IImmutableVariableCollection.Count {
            get { return 0; }
        }

        Task<IVariable> IImmutableVariableCollection.GetAsync(int index, CancellationToken cancellationToken) {
            throw new IndexOutOfRangeException();
        }

        Task<ICollection<IVariable>> IImmutableVariableCollection.GetManyAsync(
            int firstIndex,
            int count,
            CancellationToken cancellationToken
        ) {
            throw new IndexOutOfRangeException();
        }
    }
}
