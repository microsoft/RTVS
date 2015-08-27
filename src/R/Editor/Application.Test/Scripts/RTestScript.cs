using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Scripts
{
    public sealed class RTestScript : TestScript
    {
        #region Constructors

        public RTestScript() :
            base(RContentTypeDefinition.ContentType)
        {
        }

        /// <summary>
        /// Create script with editor window prepopulated with a given content
        /// </summary>
        public RTestScript(string text) :
            base(text, RContentTypeDefinition.ContentType)
        {
        }

        /// <summary>
        /// Create script with editor window prepopulated from a disk file
        /// </summary>
        public RTestScript(TestContext context, string fileName) :
            base(context, fileName)
        {
        }
        #endregion
    }
}
