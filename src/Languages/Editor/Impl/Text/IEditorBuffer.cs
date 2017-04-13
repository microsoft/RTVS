// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Document;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Represents editor code (text) buffer
    /// </summary>
    public interface IEditorBuffer {
        IServiceManager Services { get; }

        PropertyDictionary Properties { get; }

        /// <summary>
        /// Current buffer snapshot
        /// </summary>
        IEditorBufferSnapshot CurrentSnapshot { get; }

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

        /// <summary>
        /// Attempts to locate associated editor document.
        /// Implementation depends on the platform.
        /// </summary>
        /// <typeparam name="T">Type of the document to locate</typeparam>
        T GetEditorDocument<T>() where T : class, IEditorDocument;

        void Insert(int position, string text);
        void Replace(ITextRange range, string text);
        void Delete(ITextRange range);
    }
}
