using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.Sessions {
    public class BrokerMaxedUsersException : Exception{
        public BrokerMaxedUsersException() { }
        public BrokerMaxedUsersException(string message) : base(message) { }
    }
}
