using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.UserProfileInterface {
    public interface IRUserProfileCreatorResult {
        string ProfilePath { get; }
        uint Win32Result { get;}
        bool ProfileExists { get; }
    }
}
