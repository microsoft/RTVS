// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Undo {
    /// <summary>
    ///  Attach this to any ITextUndoTransaction.MergePolicy to allow two adjacent
    ///  undo actions to merge together. They'll only merge if the action name
    ///  is the same, if the old one supports "merge next", and if the new one
    ///  supports "merge previous".
    /// </summary>
    public class MergeUndoActionPolicy : IMergeTextUndoTransactionPolicy {
        private string _actionName;
        private bool _mergePrevious;
        private bool _mergeNext;
        private bool _addedTextChangePrimitives;

        public MergeUndoActionPolicy(
            string actionName,
            bool mergePrevious,
            bool mergeNext,
            bool addedTextChangePrimitives) // is the owner using AddBefore/AddAfterTextBufferChangePrimitive?
        {
            _actionName = actionName;
            _mergePrevious = mergePrevious;
            _mergeNext = mergeNext;
            _addedTextChangePrimitives = addedTextChangePrimitives;
        }

        public bool CanMerge(ITextUndoTransaction newTransaction, ITextUndoTransaction oldTransaction) {
            MergeUndoActionPolicy oldPolicy = oldTransaction.MergePolicy as MergeUndoActionPolicy;
            MergeUndoActionPolicy newPolicy = newTransaction.MergePolicy as MergeUndoActionPolicy;

            if (oldPolicy != null && oldPolicy._mergeNext &&
                newPolicy != null && newPolicy._mergePrevious &&
                oldPolicy._actionName == newPolicy._actionName) {
                // If one of the transactions is empty, than it is safe to merge
                if (newTransaction.UndoPrimitives.Count == 0 ||
                    oldTransaction.UndoPrimitives.Count == 0) {
                    return true;
                }

                // Make sure that we only merge consecutive edits
                ITextUndoPrimitive newPrimitive = newTransaction.UndoPrimitives[0];
                ITextUndoPrimitive oldPrimitive = oldTransaction.UndoPrimitives[oldTransaction.UndoPrimitives.Count - 1];

                return newPrimitive.CanMerge(oldPrimitive);
            }

            return false;
        }

        public void PerformTransactionMerge(ITextUndoTransaction oldTransaction, ITextUndoTransaction newTransaction) {
            MergeUndoActionPolicy oldPolicy = (MergeUndoActionPolicy)oldTransaction.MergePolicy;
            MergeUndoActionPolicy newPolicy = (MergeUndoActionPolicy)newTransaction.MergePolicy;

            // Remove trailing AfterTextBufferChangeUndoPrimitive from previous transaction and skip copying
            // initial BeforeTextBufferChangeUndoPrimitive from newTransaction, as they are unnecessary.

            if (oldTransaction.UndoPrimitives.Count > 0 && oldPolicy._addedTextChangePrimitives) {
                oldTransaction.UndoPrimitives.RemoveAt(oldTransaction.UndoPrimitives.Count - 1);
            }

            int copyStartIndex = newPolicy._addedTextChangePrimitives ? 1 : 0;

            for (int i = copyStartIndex; i < newTransaction.UndoPrimitives.Count; i++) {
                oldTransaction.UndoPrimitives.Add(newTransaction.UndoPrimitives[i]);
            }
        }

        public bool TestCompatiblePolicy(IMergeTextUndoTransactionPolicy other) {
            return GetType() == other.GetType();
        }
    }
}
