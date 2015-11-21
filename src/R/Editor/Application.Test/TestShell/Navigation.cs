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
        /// Move care up a number of lines
        /// </summary>
        public void MoveUp(int count = 1)
        {
            for (int i = 0; i < count; i++) {
                Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP);
            }
        }

        /// <summary>
        /// Move caret down a number of line
        /// </summary>
        public void MoveDown(int count = 1)
        {
            for (int i = 0; i < count; i++) {
                Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN);
            }
        }

        /// <summary>
        /// Move caret left by a number of characters
        /// </summary>
        public void MoveLeft(int count = 1)
        {
            for (int i = 0; i < count; i++) {
                Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT);
            }
        }

        /// <summary>
        /// Move caret right by a number character
        /// </summary>
        public void MoveRight(int count = 1)
        {
            for (int i = 0; i < count; i++) {
                Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT);
            }
        }
    }
}
