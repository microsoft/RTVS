using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    internal static class RdText
    {
        public static string GetText(RdParseContext context)
        {
            string text = string.Empty;

            int start = context.Tokens.Position;
            int end = RdParseUtility.FindRdKeywordArgumentBounds(context.Tokens);
            if (end > 0)
            {
                text = RdText.FromTokens(context, start, end);
                context.Tokens.Position = end;
            }

            return text;
        }

        public static string FromTokens(RdParseContext context, int start, int end)
        {
            // Clean descripton so it only consists of plain text
            var sb = new StringBuilder();

            for (int i = start + 1; i < end; i++)
            {
                TextRange range = TextRange.FromBounds(context.Tokens[i - 1].End, context.Tokens[i].Start);
                string s = context.TextProvider.GetText(range);

                for (int j = 0; j < s.Length; j++)
                {
                    char ch = s[j];

                    if (ch == '\n' || ch == '\r' || ch == '\t' || char.IsWhiteSpace(ch))
                    {
                        ch = ' ';
                    }

                    if (ch == '\\')
                    {
                        continue; // skip escapes
                    }

                    if (ch != ' ' || (sb.Length > 0 && sb[sb.Length - 1] != ' '))
                    {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
