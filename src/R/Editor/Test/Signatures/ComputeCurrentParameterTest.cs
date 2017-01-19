// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class ComputeCurrentParameterTest : FunctionIndexBasedTest {
        public ComputeCurrentParameterTest(IExportProvider exportProvider) : base(exportProvider) { }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter01() {
            ITextBuffer textBuffer = new TextBufferMock("aov(", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            using (var tree = new EditorTree(textBuffer, EditorShell)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {

                    session.TrackingPoint = new TrackingPointMock(textBuffer, 4, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
                    await PackageIndexUtility.GetFunctionInfoAsync(FunctionIndex, "aov");

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    int index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    index.Should().Be(0);

                    textView.Caret = new TextCaretMock(textView, 5);
                    TextBufferUtility.ApplyTextChange(textBuffer, 4, 0, 1, "a");
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    index.Should().Be(0);

                    textView.Caret = new TextCaretMock(textView, 6);
                    TextBufferUtility.ApplyTextChange(textBuffer, 5, 0, 1, ",");
                    tree.EnsureTreeReady();
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    index.Should().Be(1);

                    textView.Caret = new TextCaretMock(textView, 7);
                    TextBufferUtility.ApplyTextChange(textBuffer, 6, 0, 1, ",");
                    tree.EnsureTreeReady();
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    index.Should().Be(2);
                }
            }
        }

        private int GetCurrentParameterIndex(SignatureHelp sh, IParameter parameter) {
            for (int i = 0; i < sh.Parameters.Count; i++) {
                if (sh.Parameters[i] == parameter) {
                    return i;
                }
            }

            return -1;
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter02() {
            await PackageIndexUtility.GetFunctionInfoAsync(FunctionIndex, "legend");

            REditorSettings.PartialArgumentNameMatch = true;

            ITextBuffer textBuffer = new TextBufferMock("legend(bty=1, lt=3)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            using (var tree = new EditorTree(textBuffer, EditorShell)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    SignatureHelp sh = signatures[0] as SignatureHelp;
                    int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
                    index.Should().Be(11);

                    textView.Caret = new TextCaretMock(textView, 15);
                    index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 15);
                    index.Should().Be(6);
                }
            }
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter03() {
            await PackageIndexUtility.GetFunctionInfoAsync(FunctionIndex, "legend");

            REditorSettings.PartialArgumentNameMatch = false;

            ITextBuffer textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            using (var tree = new EditorTree(textBuffer, EditorShell)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    SignatureHelp sh = signatures[0] as SignatureHelp;
                    int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
                    index.Should().Be(0);
                }
            }
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter04() {
            await PackageIndexUtility.GetFunctionInfoAsync(FunctionIndex, "legend");

            REditorSettings.PartialArgumentNameMatch = true;

            ITextBuffer textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            using (var tree = new EditorTree(textBuffer, EditorShell)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    SignatureHelp sh = signatures[0] as SignatureHelp;
                    int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
                    index.Should().Be(9);
                }
            }
        }
    }
}
