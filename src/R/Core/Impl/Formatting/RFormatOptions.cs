using Microsoft.Languages.Core.Formatting;

namespace Microsoft.R.Core.Formatting
{
    public class RFormatOptions
    {
        public bool BracesOnNewLine { get; set; } = false;

        public int IndentSize { get; set; } = 4;

        public int TabSize { get; set; } = 4;

        public IndentType IndentType { get; set; } = IndentType.Spaces;

        public bool SpaceAfterComma { get; set; } = true;

        public bool SpaceAfterKeyword { get; set; } = true;
    }
}
