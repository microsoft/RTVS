// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Helpers {
    internal sealed class ListEnumerator<T> : IEnumerator<T> {
        private IEnumerator enumerator;

        public ListEnumerator(IEnumerator enumerator) {
            this.enumerator = enumerator;
        }

        public T Current {
            get {
                return (T)this.enumerator.Current;
            }
        }

        object IEnumerator.Current {
            get {
                return this.enumerator.Current;
            }
        }

        public void Dispose() { }

        public bool MoveNext() {
            return this.enumerator.MoveNext();
        }

        public void Reset() {
            this.enumerator.Reset();
        }
    }
}
