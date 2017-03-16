// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.Help]
    [Collection(CollectionNames.NonParallel)]
    public class VignetteCssTest {
        //[Test]
        //public void CssTest() {
        //    var ctrs = EditorShell.Current.GlobalServices.GetService<IContentTypeRegistryService>();
        //    var tbfs = EditorShell.Current.GlobalServices.GetService<ITextBufferFactoryService>();
        //    var tef = EditorShell.Current.GlobalServices.GetService<ITextEditorFactoryService>();

        //    var ct = ctrs.GetContentType(RContentTypeDefinition.ContentType);
        //    var tb = tbfs.CreateTextBuffer();
        //    var tv = tef.CreateTextView(tb, );

        //    var iwvc = Substitute.For<IInteractiveWindowVisualComponent>();
        //    iwvc.TextView.Returns(tv);

        //    var iwf = Substitute.For<IRInteractiveWorkflow>();
        //    iwf.ActiveWindow.Returns(iwvc);

        //    var iwfp = Substitute.For<IRInteractiveWorkflowProvider>();
        //    iwfp.GetOrCreate().Returns(iwf);

        //    var cfms = EditorShell.Current.GlobalServices.GetService<IClassificationFormatMapService>();
        //    var clstrs = EditorShell.Current.GlobalServices.GetService<IClassificationTypeRegistryService>();

        //    var vcb = new VignetteCodeColorBuilder(iwfp, cfms, clstrs);
        //    var cssText = vcb.GetCodeColorsCss();
        //}
    }
}
