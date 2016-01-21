using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace SetupCustomActions {
    public class CustomActions {
        [CustomAction]
        public static ActionResult DSProfilePromptAction(Session session) {
            session.Log("Begin DSProfilePromptAction");
            MessageBox.Show("Would you like to set Data Scientist profile?", "R Tools for Visual Studio");
            return ActionResult.Success;
        }
    }
}
