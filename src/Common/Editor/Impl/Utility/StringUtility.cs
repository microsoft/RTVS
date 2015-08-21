using System.Text;

namespace Microsoft.Languages.Editor.Utility
{
    public static class StringUtility
    {
        /// <summary>
        /// Wraps long string so each line is no longer
        /// that the specified number of characters 
        /// </summary>
        /// <returns></returns>
        public static string Wrap(this string s, int limit)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];

                if (char.IsWhiteSpace(ch) && count >= limit)
                {
                    sb.Append("\r\n");
                    count = 0;
                }
                else
                {
                    sb.Append(ch);
                    count++;
                }
            }

            return sb.ToString();
        }
    }
}
