using Microsoft.Languages.Editor.Controller.Constants;

namespace Microsoft.R.Editor.Application.Test.TestShell
{
    public partial class TestScript
    {
        /// <summary>
        /// Adds 'go to line/column' command to the command script
        /// </summary>
        /// <param name="line">Line number</param>
        /// <param name="column">Column number</param>
        public void GoTo(int line, int column)
        {
            Invoke(() => EditorWindow.CoreEditor.GoTo(line, column, 0));
        }

        /// <summary>
        /// Move care up one line
        /// </summary>
        public void MoveUp()
        {
            Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP);
        }

        /// <summary>
        /// Move caret down one line
        /// </summary>
        public void MoveDown()
        {
            Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN);
        }

        /// <summary>
        /// Move caret left by one character
        /// </summary>
        public void MoveLeft()
        {
            Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT);
        }

        /// <summary>
        /// Move caret right by one character
        /// </summary>
        public void MoveRight()
        {
            Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT);
        }
    }
}
