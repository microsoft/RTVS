using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Interop {
    internal sealed class MapiMail {
        public bool AddRecipientTo(string email) {
            return AddRecipient(email, HowTo.MAPI_TO);
        }

        public void AddAttachment(string strAttachmentFileName) {
            m_attachments.Add(strAttachmentFileName);
        }

        public int SendMailPopup(string strSubject, string strBody) {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG | MAPI_DIALOG_MODELESS);
        }

        //public int SendMailDirect(string strSubject, string strBody) {
        //    return SendMail(strSubject, strBody, MAPI_LOGON_UI);
        //}

        [DllImport("MAPI32.DLL")]
        static extern int MAPISendMail(IntPtr sess, IntPtr hwnd,
            MapiMessage message, int flg, int rsv);

        int SendMail(string subject, string body, int how) {
            IntPtr vsWindow;
            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.GetDialogOwnerHwnd(out vsWindow);

            MapiMessage msg = new MapiMessage();
            msg.subject = subject;
            msg.noteText = body;

            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);

            m_lastError = MAPISendMail(IntPtr.Zero, vsWindow, msg, how, 0);

            Cleanup(ref msg);
            return m_lastError;
        }

        bool AddRecipient(string email, HowTo howTo) {
            MapiRecipDesc recipient = new MapiRecipDesc();

            recipient.recipClass = (int)howTo;
            recipient.name = email;
            m_recipients.Add(recipient);

            return true;
        }

        IntPtr GetRecipients(out int recipCount) {
            recipCount = 0;
            if (m_recipients.Count == 0)
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

            int ptr = (int)intPtr;
            foreach (MapiRecipDesc mapiDesc in m_recipients) {
                Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            recipCount = m_recipients.Count;
            return intPtr;
        }

        IntPtr GetAttachments(out int fileCount) {
            fileCount = 0;
            if (m_attachments == null)
                return IntPtr.Zero;

            if ((m_attachments.Count <= 0) || (m_attachments.Count >
                maxAttachments))
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiFileDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

            MapiFileDesc mapiFileDesc = new MapiFileDesc();
            mapiFileDesc.position = -1;
            int ptr = (int)intPtr;

            foreach (string strAttachment in m_attachments) {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            fileCount = m_attachments.Count;
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

            m_recipients.Clear();
            m_attachments.Clear();
            m_lastError = 0;
        }

        List<MapiRecipDesc> m_recipients = new
            List<MapiRecipDesc>();
        List<string> m_attachments = new List<string>();
        int m_lastError = 0;

        const int MAPI_LOGON_UI = 0x00000001;
        const int MAPI_DIALOG_MODELESS = 0x00000004;
        const int MAPI_DIALOG = 0x00000008;
        const int maxAttachments = 20;

        enum HowTo { MAPI_ORIG = 0, MAPI_TO, MAPI_CC, MAPI_BCC };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiMessage {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiFileDesc {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiRecipDesc {
        public int reserved;
        public int recipClass;
        public string name;
        public string address;
        public int eIDSize;
        public IntPtr entryID;
    }

    // MAPI error codes
    public enum MapiErrorCode {
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

        MAPI_SUCCESS = 0
    }
}
