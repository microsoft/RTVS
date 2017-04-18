// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Components.Test.Fakes.Undo {
    /// <summary>
    /// Represents an empty <see cref="IMergeTextUndoTransactionPolicy"/> implementation, which disallows merging between transactions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class NullMergeUndoTransactionPolicy : IMergeTextUndoTransactionPolicy {
        #region Private Fields

        private static NullMergeUndoTransactionPolicy _instance;

        #endregion

        #region Private Constructor

        private NullMergeUndoTransactionPolicy() {
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="NullMergeUndoTransactionPolicy"/> object.
        /// </summary>
        public static IMergeTextUndoTransactionPolicy Instance => _instance ?? (_instance = new NullMergeUndoTransactionPolicy());

        public bool TestCompatiblePolicy(IMergeTextUndoTransactionPolicy other) {
            return false;
        }

        public bool CanMerge(ITextUndoTransaction newerTransaction, ITextUndoTransaction olderTransaction) {
            return false;
        }

        public void PerformTransactionMerge(ITextUndoTransaction existingTransaction, ITextUndoTransaction newTransaction) {
            throw new InvalidOperationException("Strings.NullMergePolicyCannotMerge");
        }
    }
}
