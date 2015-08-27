using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Scripts
{
    [TestClass]
    public class IntellisenseTest
    {
        //[TestMethod]
        public void R_Intellisense()
        {
            var script = new RTestScript();

            try
            {
                script.Type("<ht{DOWN}{TAB}");
                script.DoIdle(300);
                script.Type(" di{DOWN}=r");
                script.DoIdle(300);
                script.Type("{DOWN}{DOWN}{TAB}{RIGHT}>");
                script.DoIdle(1000);
                script.Type("<br/>");
                script.DoIdle(1000);
                script.Type("<a>");
                script.DoIdle(1000);

                string expected = "<html dir=\"rtl\"><br/><a></a></html>";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                script.Close();
            }
        }
    }
}
