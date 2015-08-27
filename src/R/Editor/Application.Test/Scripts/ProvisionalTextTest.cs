using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Scripts
{
    [TestClass]
    public class HtmlProvisionalTextTest
    {
        //[TestMethod]
        public void R_ProvisionalText1()
        {
            var script = new RTestScript();

            try
            {
                script.Type("{ENTER}{UP}");
                script.Type("<style id=foo\" class=bar\">{ENTER}");
                script.DoIdle(300);
                script.Type("{DOWN}{DOWN}");
                script.DoIdle(500);
                script.Type("<script{ESC} src=foo\" class=bar\">{ENTER}");
                script.DoIdle(1000);

                string expected = "<style id=\"foo\" class=\"bar\">\r\n\r\n</style>\r\n<script src=\"foo\" class=\"bar\">\r\n\r\n</script>\r\n";
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
