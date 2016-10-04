using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.UserProfileInterface
{
    public interface IRUserProfileCreator
    {
         IRUserProfileCreatorResult Create(string username, string domain, string password);
    }
}
