// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsExpansionSessionMock : IVsExpansionSession {

        public int ExpansionFieldIndex { get; private set; }
        
        public int EndCurrentExpansion(int fLeaveCaret) {
            return VSConstants.S_OK;
        }

        public int GetDeclarationNode(string bstrNode, out global::MSXML.IXMLDOMNode pNode) {
            throw new NotImplementedException();
        }

        public int GetEndSpan(TextSpan[] pts) {
            throw new NotImplementedException();
        }

        public int GetFieldSpan(string bstrField, TextSpan[] ptsSpan) {
            throw new NotImplementedException();
        }

        public int GetFieldValue(string bstrFieldName, out string pbstrValue) {
            throw new NotImplementedException();
        }

        public int GetHeaderNode(string bstrNode, out global::MSXML.IXMLDOMNode pNode) {
            throw new NotImplementedException();
        }

        public int GetSnippetNode(string bstrNode, out global::MSXML.IXMLDOMNode pNode) {
            throw new NotImplementedException();
        }

        public int GetSnippetSpan(TextSpan[] pts) {
            pts[0] = new TextSpan();
            return VSConstants.S_OK;
        }

        public int GoToNextExpansionField(int fCommitIfLast) {
            ExpansionFieldIndex++;
            return VSConstants.S_OK;
        }

        public int GoToPreviousExpansionField() {
            ExpansionFieldIndex--;
            return VSConstants.S_OK;
        }

        public int SetEndSpan(TextSpan ts) {
            throw new NotImplementedException();
        }

        public int SetFieldDefault(string bstrFieldName, string bstrNewValue) {
            throw new NotImplementedException();
        }
    }
}
