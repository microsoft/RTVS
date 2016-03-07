// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsExpansionSessionMock : IVsExpansionSession {
        public int EndCurrentExpansion(int fLeaveCaret) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public int GoToNextExpansionField(int fCommitIfLast) {
            throw new NotImplementedException();
        }

        public int GoToPreviousExpansionField() {
            throw new NotImplementedException();
        }

        public int SetEndSpan(TextSpan ts) {
            throw new NotImplementedException();
        }

        public int SetFieldDefault(string bstrFieldName, string bstrNewValue) {
            throw new NotImplementedException();
        }
    }
}
