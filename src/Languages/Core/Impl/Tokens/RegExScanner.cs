using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    public static class RegExScanner
    {
        public static int Scan(CharacterStream cs)
        {
            int start = cs.Position;
            bool escape = false;

            cs.MoveToNextChar();

            while (!cs.IsEndOfStream())
            {
                if (escape)
                {
                    escape = false;
                }
                else if (cs.CurrentChar == '\\')
                {
                    escape = true;
                }
                else if (cs.CurrentChar == '\n' || cs.CurrentChar == '\r')
                {
                    cs.Advance(-1);
                    break;
                }
                else if (cs.CurrentChar == '/')
                {
                    if (cs.Position == start + 1)
                    {
                        cs.Position = start;
                        return 0;
                    }
                    else
                    {
                        // Handle /regex/g and such. 'g', 'i' and 'm' are allowed, see
                        // http://www.regular-expressions.info/javascript.html
                        // Basically any combination of 'g', 'i' and 'm' up to three
                        // characters.

                        for (int i = 0; i < 3; i++)
                        {
                            if (cs.NextChar != 'g' && cs.NextChar != 'i' && cs.NextChar != 'm')
                                break;

                            cs.MoveToNextChar();
                        }

                        break;
                    }
                }

                cs.MoveToNextChar();
            }

            cs.MoveToNextChar();

            return cs.Position - start;
        }
    }
}
