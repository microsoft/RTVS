using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.OS {
    public interface IUserProfileCreatorResult {
        uint Error { get; set; }
        bool ProfileExists { get; set; }
        string ProfilePath { get; set; }
    }
}
