using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Test.Text
{
    [ExcludeFromCodeCoverage]
    public class InclusiveTextRange : TextRange
    {
        private bool _allowZeroLength;
        private bool _isStartInclusive;
        private bool _isEndInclusive;

        public InclusiveTextRange(int start, int length, bool allowZeroLength, bool isStartInclusive, bool isEndInclusive) :
            base(start, length)
        {
            _allowZeroLength = allowZeroLength;
            _isStartInclusive = isStartInclusive;
            _isEndInclusive = isEndInclusive;
        }

        public override bool AllowZeroLength
        {
            get
            {
                return _allowZeroLength;
            }
        }

        public override bool IsStartInclusive
        {
            get
            {
                return _isStartInclusive;
            }
        }

        public override bool IsEndInclusive
        {
            get
            {
                return _isEndInclusive;
            }
        }
    }
}
