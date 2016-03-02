// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Controller.Constants;

namespace Microsoft.R.Editor.Application.Test.TestShell
{
    public partial class TestScript
    {
        private int _idleSleepTimeout = 25;

        /// <summary>
        /// Simulates typing in the editor
        /// </summary>
        /// <param name="textToType"></param>
        public void Type(string textToType, int idleTime = 10)
        {
            _idleSleepTimeout = idleTime;

            for (int i = 0; i < textToType.Length; i++)
            {
                char ch = textToType[i];

                int length = TranslateSpecialChar(textToType, i);
                if (length > 0)
                {
                    i += length;
                }
                else
                {
                    Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.TYPECHAR, ch, idleTime);
                    if (idleTime > 0)
                    {
                        DoIdle(idleTime);
                    }
                }
            }
        }

        private int TranslateSpecialChar(string textToType, int index)
        {
            int length = 0;

            if (index < textToType.Length - 1 && textToType[index] == '{' && Char.IsUpper(textToType[index + 1]))
            {
                index++;

                int closeBrace = textToType.IndexOf('}', index);
                if (closeBrace >= 0)
                {
                    length = closeBrace - index + 1;
                    var s = textToType.Substring(index, closeBrace - index);
 
                    if (String.Compare(s, "ENTER") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.RETURN, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "TAB") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.TAB);
                    }
                    else if (String.Compare(s, "BACKSPACE") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.BACKSPACE, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "DELETE") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.DELETE, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "UP") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.UP, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "DOWN") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.DOWN, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "LEFT") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.LEFT, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "RIGHT") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.RIGHT, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "HOME") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.HOME, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "END") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.END, _idleSleepTimeout);
                    }
                    else if (String.Compare(s, "ESC") == 0)
                    {
                        Execute(VSConstants.VSStd2KCmdID.CANCEL, _idleSleepTimeout);
                    }
                }
            }

            return length;
        }

        public void Enter()
        {
            Execute(VSConstants.VSStd2KCmdID.RETURN, _idleSleepTimeout);
        }

        public void Backspace()
        {
            Execute(VSConstants.VSStd2KCmdID.BACKSPACE, _idleSleepTimeout);
        }

        public void Delete()
        {
            Execute(VSConstants.VSStd2KCmdID.DELETE, _idleSleepTimeout);
        }
    }
}
