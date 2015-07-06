using System;

namespace Microsoft.Languages.Core.Utility
{
    public static class TimeUtility
    {
        public static int MillisecondsSinceUTC(DateTime since)
        {
            var diff = DateTime.UtcNow - since;
            return (int)diff.TotalMilliseconds;
        }
    }
}
