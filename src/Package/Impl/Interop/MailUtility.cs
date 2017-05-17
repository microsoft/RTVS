// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Interop {
    internal sealed class MapiMail {
        private static readonly ManualResetEventSlim _completed = new ManualResetEventSlim();
        private static volatile int _result;

        private List<MapiRecipDesc> _recipients = new List<MapiRecipDesc>();
        private List<string> _attachments = new List<string>();

        const int MAPI_LOGON_UI = 0x00000001;
        const int MAPI_DIALOG_MODELESS = 0x00000004;
        const int MAPI_DIALOG = 0x00000008;
        const int maxAttachments = 20;

        public bool AddRecipientTo(string email) => AddRecipient(email, HowTo.MAPI_TO);

        public void AddAttachment(string strAttachmentFileName) => _attachments.Add(strAttachmentFileName);

        public int SendMailPopup(string strSubject, string strBody) => SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG | MAPI_DIALOG_MODELESS);

        class ThreadParam {
            public MapiMessage Message { get; set; }
            public IntPtr VsWindow { get; set; }
            public int MapiFlags { get; set; }
        }

        [DllImport("MAPI32.DLL")]
        static extern int MAPISendMail(IntPtr sess, IntPtr hwnd,
            MapiMessage message, int flg, int rsv);

        int SendMail(string subject, string body, int how) {
            IntPtr vsWindow;
            IVsUIShell shell = VsAppShell.Current.GetService<IVsUIShell>(typeof(SVsUIShell));
            shell.GetDialogOwnerHwnd(out vsWindow);

            MapiMessage msg = new MapiMessage();
            msg.subject = subject;
            msg.noteText = body;

            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);

            ThreadParam p = new ThreadParam() {
                Message = msg,
                VsWindow = vsWindow,
                MapiFlags = how
            };

            bool success = false;
            Thread t = null;
            try {
                _completed.Reset();
                t = new Thread(ThreadProc, 8192);
                t.Start(p);
                success = _completed.Wait(5000);
            } catch (Exception) { }

            if (!success) {
                if (t != null) {
                    t.Abort();
                }
            }

            Cleanup(ref msg);
            return _result;
        }

        private static void ThreadProc(object o) {
            try {
                ThreadParam p = o as ThreadParam;
                _result = MAPISendMail(IntPtr.Zero, p.VsWindow, p.Message, p.MapiFlags, 0);
            } catch (ThreadAbortException) {
                _result = (int)MapiErrorCode.MAPI_TIMEOUT;
            } catch (Exception) {
                _result = (int)MapiErrorCode.MAPI_E_FAILURE;
            } finally {
                _completed.Set();
            }
        }

        bool AddRecipient(string email, HowTo howTo) {
            MapiRecipDesc recipient = new MapiRecipDesc();

            recipient.recipClass = (int)howTo;
            recipient.name = email;
            _recipients.Add(recipient);

            return true;
        }

        IntPtr GetRecipients(out int recipCount) {
            recipCount = 0;
            if (_recipients.Count == 0) {
                return IntPtr.Zero;
            }

            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(_recipients.Count * size);

            int ptr = (int)intPtr;
            foreach (MapiRecipDesc mapiDesc in _recipients) {
                Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            recipCount = _recipients.Count;
            return intPtr;
        }

        IntPtr GetAttachments(out int fileCount) {
            fileCount = 0;
            if (_attachments == null) {
                return IntPtr.Zero;
            }

            if ((_attachments.Count <= 0) || (_attachments.Count >
                maxAttachments)) {
                return IntPtr.Zero;
            }

            int size = Marshal.SizeOf(typeof(MapiFileDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(_attachments.Count * size);

            MapiFileDesc mapiFileDesc = new MapiFileDesc();
            mapiFileDesc.position = -1;
            int ptr = (int)intPtr;

            foreach (string strAttachment in _attachments) {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            fileCount = _attachments.Count;
            return intPtr;
        }

        void Cleanup(ref MapiMessage msg) {
            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            int ptr = 0;

            if (msg.recips != IntPtr.Zero) {
                ptr = (int)msg.recips;
                for (int i = 0; i < msg.recipCount; i++) {
                    Marshal.DestroyStructure((IntPtr)ptr,
                        typeof(MapiRecipDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.recips);
            }

            if (msg.files != IntPtr.Zero) {
                size = Marshal.SizeOf(typeof(MapiFileDesc));

                ptr = (int)msg.files;
                for (int i = 0; i < msg.fileCount; i++) {
                    Marshal.DestroyStructure((IntPtr)ptr,
                        typeof(MapiFileDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.files);
            }

            _recipients.Clear();
            _attachments.Clear();
            _result = (int)MapiErrorCode.MAPI_SUCCESS;
        }

        enum HowTo { MAPI_ORIG = 0, MAPI_TO, MAPI_CC, MAPI_BCC };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class MapiMessage {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        public IntPtr originator;
        public int recipCount;
        [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        public IntPtr recips;
        public int fileCount;
        [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class MapiFileDesc {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class MapiRecipDesc {
        public int reserved;
        public int recipClass;
        public string name;
        public string address;
        public int eIDSize;
        [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        public IntPtr entryID;
    }

    // MAPI error codes
    enum MapiErrorCode {
        /// <summary>
        /// A recipient matched more than one of the recipient descriptor structures and MAPI_DIALOG was not set. No message was sent.
        /// </summary>
        MAPI_E_AMBIGUOUS_RECIPIENT = 21,

        /// <summary>
        /// The specified attachment was not found. No message was sent.
        /// </summary>
        MAPI_E_ATTACHMENT_NOT_FOUND = 11,

        /// <summary>
        /// The specified attachment could not be opened.No message was sent.
        /// </summary>
        MAPI_E_ATTACHMENT_OPEN_FAILURE = 12,

        /// <summary>
        /// The type of a recipient was not MAPI_TO, MAPI_CC, or MAPI_BCC. No message was sent.
        /// </summary>
        MAPI_E_BAD_RECIPTYPE = 15,

        /// <summary>
        /// One or more unspecified errors occurred. No message was sent.
        /// </summary>
        MAPI_E_FAILURE = 2,

        /// <summary>
        /// There was insufficient memory to proceed. No message was sent.
        /// </summary>
        MAPI_E_INSUFFICIENT_MEMORY = 5,
        /// <summary>
        /// One or more recipients were invalid or did not resolve to any address.
        /// </summary>
        MAPI_E_INVALID_RECIPS = 25,

        /// <summary>
        /// There was no default logon, and the user failed to log on successfully when the logon dialog box was displayed.No message was sent.
        /// </summary>
        MAPI_E_LOGIN_FAILURE = 3,

        /// <summary>
        /// The text in the message was too large.No message was sent.
        /// </summary>
        MAPI_E_TEXT_TOO_LARGE = 18,

        /// <summary>
        /// There were too many file attachments. No message was sent.
        /// </summary>
        MAPI_E_TOO_MANY_FILES = 9,

        /// <summary>
        /// There were too many recipients.No message was sent.
        /// </summary>
        MAPI_E_TOO_MANY_RECIPIENTS = 10,

        /// <summary>
        /// The MAPI_FORCE_UNICODE flag is specified and Unicode is not supported.
        /// Note This value can be returned by MAPISendMailW only.
        /// </summary>
        MAPI_E_UNICODE_NOT_SUPPORTED = 27,

        /// <summary>
        /// A recipient did not appear in the address list.No message was sent.
        /// </summary>
        MAPI_E_UNKNOWN_RECIPIENT = 14,

        /// <summary>
        /// The user canceled one of the dialog boxes. No message was sent.
        /// </summary>
        MAPI_E_USER_ABORT = 1,

        MAPI_SUCCESS = 0,

        /// <summary>
        /// Custom error, thread timed out trying to launch mail client
        /// </summary>
        MAPI_TIMEOUT = 1000
    }
}
