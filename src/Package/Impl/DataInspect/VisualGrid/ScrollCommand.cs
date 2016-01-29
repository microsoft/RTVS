using System;
using System.Diagnostics;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class ScrollCommand {
        public ScrollCommand(ScrollType code, double param) {
            Debug.Assert(code != ScrollType.SizeChange
                && code != ScrollType.SetHorizontalOffset
                && code != ScrollType.SetVerticalOffset);

            Code = code;
            Param = param;
        }

        public ScrollCommand(ScrollType code, Size size) {
            Debug.Assert(code == ScrollType.SizeChange);

            Code = code;
            Param = size;
        }

        public ScrollCommand(ScrollType code, double offset, ThumbTrack thumbtrack) {
            Debug.Assert(code == ScrollType.SetHorizontalOffset
                || code == ScrollType.SetVerticalOffset);

            Code = code;
            Param = new Tuple<double, ThumbTrack>(offset, thumbtrack);
        }

        public ScrollType Code { get; set; }

        public object Param { get; set; }
    }
}
