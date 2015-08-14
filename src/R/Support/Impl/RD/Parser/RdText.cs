using System.Text;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    internal static class RdText
    {
        public static string GetText(ParseContext context)
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

        public static string FromTokens(ParseContext context, int start, int end)
        {
            // Clean descripton so it only consists of plain text
            var sb = new StringBuilder();

            for (int i = start + 1; i < end; i++)
            {
                RdToken token = context.Tokens[i];
                if (token.TokenType == RdTokenType.Argument)
                {
                    string s = context.TextProvider.GetText(token);
                    for (int j = 0; j < s.Length; j++)
                    {
                        char ch = s[j];

                        if (ch == '\n' || ch == '\r' || ch == '\t' || char.IsWhiteSpace(ch))
                        {
                            ch = ' ';
                        }

                        if (ch != ' ' || (sb.Length > 0 && sb[sb.Length - 1] != ' '))
                        {
                            sb.Append(ch);
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
