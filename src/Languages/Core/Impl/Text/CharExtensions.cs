namespace Microsoft.Languages.Core.Text {
    public static class CharExtensions {
        public static readonly char[] LineBreakChars = new char[] { '\n', '\r' };

        public static bool IsLineBreak(this char ch) {
            return ch == '\r' || ch == '\n';
        }
    }
}
