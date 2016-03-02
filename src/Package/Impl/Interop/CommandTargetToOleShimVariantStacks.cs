// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Interop {
    public sealed class CommandTargetToOleShimVariantStacks {
        private Stack<IntPtr> _variantInStack;
        private Stack<IntPtr> _variantOutStack;
        private Stack<bool> _allocateVariants;

        private CommandTargetToOleShimVariantStacks() {
            _variantInStack = new Stack<IntPtr>();
            _variantOutStack = new Stack<IntPtr>();
            _allocateVariants = new Stack<bool>();
        }

        public static CommandTargetToOleShimVariantStacks EnsureConnected(ITextView textView) {
            CommandTargetToOleShimVariantStacks stacks = null;
            if (!textView.Properties.TryGetProperty(typeof(CommandTargetToOleShimVariantStacks), out stacks)) {
                stacks = new CommandTargetToOleShimVariantStacks();
                textView.Properties.AddProperty(typeof(CommandTargetToOleShimVariantStacks), stacks);
            }

            return stacks;
        }

        public void Push(IntPtr variantIn, IntPtr variantOut, bool allocateVariants) {
            _variantInStack.Push(variantIn);
            _variantOutStack.Push(variantOut);
            _allocateVariants.Push(allocateVariants);
        }

        public void Pop() {
            _variantInStack.Pop();
            _variantOutStack.Pop();
            _allocateVariants.Pop();
        }

        public void Peek(out IntPtr variantIn, out IntPtr variantOut, out bool allocateVariants) {
            variantIn = _variantInStack.Peek();
            variantOut = _variantOutStack.Peek();
            allocateVariants = _allocateVariants.Peek();
        }

        public bool IsEmpty {
            get {
                return _variantInStack.Count == 0;
            }
        }
    }
}
