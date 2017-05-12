// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class ComputeCurrentParameterTest : FunctionIndexBasedTest {
        private readonly IREditorSettings _settings;

        public ComputeCurrentParameterTest(IServiceContainer services) :
            base(services) {
            _settings = Substitute.For<IREditorSettings>();
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter01() {
            var textBuffer = new TextBufferMock("aov(", RContentTypeDefinition.ContentType);
            var editorBuffer = textBuffer.ToEditorBuffer();
            var source = new RSignatureHelpSource(textBuffer, Services);
            var session = new SignatureHelpSessionMock(Services, textBuffer, 0);
            var textView = session.TextView as TextViewMock;
            var signatures = new List<ISignature>();

            using (var tree = new EditorTree(editorBuffer, Services)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {

                    session.TrackingPoint = new TrackingPointMock(textBuffer, 4, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
                    await FunctionIndex.GetPackageNameAsync("aov");

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    var index = GetCurrentParameterIndex(signatures[0], signatures[0].CurrentParameter);
                    index.Should().Be(0);

                    textView.Caret = new TextCaretMock(textView, 5);
                    TextBufferUtility.ApplyTextChange(textBuffer, 4, 0, 1, "a");
                    index = GetCurrentParameterIndex(signatures[0], signatures[0].CurrentParameter);
                    index.Should().Be(0);

                    textView.Caret = new TextCaretMock(textView, 6);
                    TextBufferUtility.ApplyTextChange(textBuffer, 5, 0, 1, ",");
                    tree.EnsureTreeReady();
                    index = GetCurrentParameterIndex(signatures[0], signatures[0].CurrentParameter);
                    index.Should().Be(1);

                    textView.Caret = new TextCaretMock(textView, 7);
                    TextBufferUtility.ApplyTextChange(textBuffer, 6, 0, 1, ",");
                    tree.EnsureTreeReady();
                    index = GetCurrentParameterIndex(signatures[0], signatures[0].CurrentParameter);
                    index.Should().Be(2);
                }
            }
        }

        private int GetCurrentParameterIndex(ISignature sh, IParameter parameter) {
            for (var i = 0; i < sh.Parameters.Count; i++) {
                if(sh.Parameters[i].Locus.Start == parameter.Locus.Start) {
                    return i;
                }
            }
            return -1;
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter02() {
            await FunctionIndex.GetPackageNameAsync("legend");

            _settings.PartialArgumentNameMatch.Returns(true);

            var textBuffer = new TextBufferMock("legend(bty=1, lt=3)", RContentTypeDefinition.ContentType);
            var eb = textBuffer.ToEditorBuffer();
            var source = new RSignatureHelpSource(textBuffer, Services);
            var session = new SignatureHelpSessionMock(Services, textBuffer, 0);
            var textView = session.TextView as TextViewMock;
            var signatures = new List<ISignature>();

            using (var tree = new EditorTree(eb, Services)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);
                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    var sh = ((RSignatureHelp)signatures[0]).FunctionSignatureHelp;
                    var index = sh.SignatureInfo.ComputeCurrentParameter(tree.BufferSnapshot, tree.AstRoot, 8, _settings);
                    index.Should().Be(11);

                    textView.Caret = new TextCaretMock(textView, 15);
                    index = sh.SignatureInfo.ComputeCurrentParameter(tree.BufferSnapshot, tree.AstRoot, 15, _settings);
                    index.Should().Be(6);
                }
            }
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter03() {
            await FunctionIndex.GetPackageNameAsync("legend");

            var textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            var eb = textBuffer.ToEditorBuffer();
            var source = new RSignatureHelpSource(textBuffer, Services);
            var session = new SignatureHelpSessionMock(Services, textBuffer, 0);
            var textView = session.TextView as TextViewMock;
            var signatures = new List<ISignature>();

            using (var tree = new EditorTree(eb, Services)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    var sh = ((RSignatureHelp)signatures[0]).FunctionSignatureHelp;
                    var index = sh.SignatureInfo.ComputeCurrentParameter(tree.BufferSnapshot, tree.AstRoot, 8, _settings);
                    index.Should().Be(0);
                }
            }
        }

        [Test(ThreadType = ThreadType.UI)]
        public async Task ParameterTest_ComputeCurrentParameter04() {
            await FunctionIndex.GetPackageNameAsync("legend");

            _settings.PartialArgumentNameMatch.Returns(true);

            var textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            var eb = textBuffer.ToEditorBuffer();
            var source = new RSignatureHelpSource(textBuffer, Services);
            var session = new SignatureHelpSessionMock(Services, textBuffer, 0);
            var textView = session.TextView as TextViewMock;
            var signatures = new List<ISignature>();

            using (var tree = new EditorTree(eb, Services)) {
                tree.Build();
                using (var document = new EditorDocumentMock(tree)) {
                    session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

                    tree.TakeThreadOwnerShip();
                    await source.AugmentSignatureHelpSessionAsync(session, signatures, tree.AstRoot);

                    signatures.Should().ContainSingle();

                    textView.Caret = new TextCaretMock(textView, 8);
                    var sh = ((RSignatureHelp)signatures[0]).FunctionSignatureHelp;
                    var index = sh.SignatureInfo.ComputeCurrentParameter(tree.BufferSnapshot, tree.AstRoot, 8, _settings);
                    index.Should().Be(9);
                }
            }
        }
    }
}
