using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SettingsTest : UnitTestBase
    {
        [TestMethod]
        public void Settings_TestDefaults()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            Assert.AreEqual(false, REditorSettings.CommitOnSpace);
            Assert.AreEqual(true, REditorSettings.CompletionEnabled);
            Assert.AreEqual(true, REditorSettings.FormatOnPaste);
            Assert.AreEqual(4, REditorSettings.IndentSize);
            Assert.AreEqual(IndentStyle.Smart, REditorSettings.IndentStyle);
            Assert.AreEqual(IndentType.Spaces, REditorSettings.IndentType);
            Assert.AreEqual(4, REditorSettings.TabSize);
            Assert.AreEqual(true, REditorSettings.ValidationEnabled);
            Assert.AreEqual(true, REditorSettings.SignatureHelpEnabled);
            Assert.AreEqual(false, REditorSettings.ShowTclFunctions);
            Assert.AreEqual(false, REditorSettings.ShowInternalFunctions);

            Assert.AreEqual(4, REditorSettings.FormatOptions.IndentSize);
            Assert.AreEqual(4, REditorSettings.FormatOptions.TabSize);
            Assert.AreEqual(IndentType.Spaces, REditorSettings.FormatOptions.IndentType);
            Assert.AreEqual(true, REditorSettings.FormatOptions.SpaceAfterComma);
            Assert.AreEqual(true, REditorSettings.FormatOptions.SpaceAfterKeyword);
            Assert.AreEqual(false, REditorSettings.FormatOptions.BracesOnNewLine);
        }
    }
}
