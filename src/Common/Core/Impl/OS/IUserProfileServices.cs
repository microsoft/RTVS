using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.OS {
    public interface IUserProfileServices {
        IUserProfileCreatorResult CreateUserProfile(IUserCredentials credentails, ILogger logger);
    }
}
