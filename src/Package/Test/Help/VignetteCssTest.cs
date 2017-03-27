// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.Help]
    [Collection(CollectionNames.NonParallel)]
    public class VignetteCssTest {
        //[Test]
        //public void CssTest() {
        //    var ctrs = shell.Current.Services.GetService<IContentTypeRegistryService>();
        //    var tbfs = shell.Current.Services.GetService<ITextBufferFactoryService>();
        //    var tef = shell.Current.Services.GetService<ITextEditorFactoryService>();

        //    var ct = ctrs.GetContentType(RContentTypeDefinition.ContentType);
        //    var tb = tbfs.CreateTextBuffer();
        //    var tv = tef.CreateTextView(tb, );

        //    var iwvc = Substitute.For<IInteractiveWindowVisualComponent>();
        //    iwvc.TextView.Returns(tv);

        //    var iwf = Substitute.For<IRInteractiveWorkflow>();
        //    iwf.ActiveWindow.Returns(iwvc);

        //    var iwfp = Substitute.For<IRInteractiveWorkflowProvider>();
        //    iwfp.GetOrCreate().Returns(iwf);

        //    var cfms = shell.Current.Services.GetService<IClassificationFormatMapService>();
        //    var clstrs = shell.Current.Services.GetService<IClassificationTypeRegistryService>();

        //    var vcb = new VignetteCodeColorBuilder(iwfp, cfms, clstrs);
        //    var cssText = vcb.GetCodeColorsCss();
        //}
    }
}
