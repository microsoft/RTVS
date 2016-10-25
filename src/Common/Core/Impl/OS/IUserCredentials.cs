using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.OS {
    public interface IUserCredentials {
        string Username { get; set; }
        string Password { get; set; }
        string Domain { get; set; }
    }
}
