// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class AdornmentLayerMock : IAdornmentLayer {
        public ReadOnlyCollection<IAdornmentLayerElement> Elements {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsEmpty {
            get {
                throw new NotImplementedException();
            }
        }

        public double Opacity {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public IWpfTextView TextView {
            get {
                throw new NotImplementedException();
            }
        }

        public bool AddAdornment(SnapshotSpan visualSpan, object tag, UIElement adornment) {
            throw new NotImplementedException();
        }

        public bool AddAdornment(AdornmentPositioningBehavior behavior, SnapshotSpan? visualSpan, object tag, UIElement adornment, AdornmentRemovedCallback removedCallback) {
            throw new NotImplementedException();
        }

        public void RemoveAdornment(UIElement adornment) {
            throw new NotImplementedException();
        }

        public void RemoveAdornmentsByTag(object tag) {
            throw new NotImplementedException();
        }

        public void RemoveAdornmentsByVisualSpan(SnapshotSpan visualSpan) {
            throw new NotImplementedException();
        }

        public void RemoveAllAdornments() {
            throw new NotImplementedException();
        }

        public void RemoveMatchingAdornments(Predicate<IAdornmentLayerElement> match) {
            throw new NotImplementedException();
        }

        public void RemoveMatchingAdornments(SnapshotSpan visualSpan, Predicate<IAdornmentLayerElement> match) {
            throw new NotImplementedException();
        }
    }
}
