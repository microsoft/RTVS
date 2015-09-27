using System;

namespace Microsoft.Markdown.Editor.Commands
{
    public static class MdPackageCommandId
    {
        public static readonly Guid MdCmdSetGuid = new Guid("0BF33C69-94C2-4985-81A0-2556F8DB88A6");

        // GuidList.CmdSetGuid
        public const int icmdKnitHtml = 601;
        public const int icmdKnitPdf = 602;
        public const int icmdKnitWord = 603;
    }
}
