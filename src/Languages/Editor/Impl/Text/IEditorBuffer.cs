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
    public interface IEditorBuffer: IPlatformSpecificObject, IPropertyHolder {
        /// <summary>
        /// Set of services attached to the text buffer
        /// </summary>
        IServiceManager Services { get; }

        /// <summary>
        /// Name of the content type. Typically language name like 'R' or 'HTML'.
        /// </summary>
        string ContentType { get; }

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
        /// Attempts to locate associated editor document.
        /// Implementation depends on the platform.
        /// </summary>
        /// <typeparam name="T">Type of the document to locate</typeparam>
        T GetEditorDocument<T>() where T : class, IEditorDocument;

        bool Insert(int position, string text);
        bool Replace(ITextRange range, string text);
        bool Delete(ITextRange range);
    }
}
