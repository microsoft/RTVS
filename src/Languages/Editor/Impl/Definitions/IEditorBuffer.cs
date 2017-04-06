// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor {
    /// <summary>
    /// Represents editor code (text) buffer
    /// </summary>
    public interface IEditorBuffer {
        IServiceManager Services { get; }

        PropertyDictionary Properties { get; }

        /// <summary>
        /// Current buffer snapshot
        /// </summary>
        IBufferSnapshot CurrentSnapshot { get; }

        /// <summary>
        /// Fires when text buffer content changed but before 
        /// <see cref="Changed"/>
        /// </summary>
        event EventHandler<TextChangeEventArgs> ChangedHighPriority;

        /// <summary>
        /// Fires when text buffer content changed
        /// </summary>
        event EventHandler<TextChangeEventArgs> Changed;

        /// <summary>
        /// Fires when text buffer is closing
        /// </summary>
        event EventHandler Closing;

        /// <summary>
        /// Path to the file being edited, if any.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Returns underlying platform object such as ITextBuffer in Visual Studio.
        /// May return null if there is no underlying implementation.
        /// </summary>
        T As<T>() where T: class;

        void Insert(int position, string text);
        void Replace(ITextRange range, string text);
        void Delete(ITextRange range);
    }
}
