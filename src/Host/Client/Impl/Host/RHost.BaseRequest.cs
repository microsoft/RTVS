using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BaseRequest {
            public readonly string Id;
            public readonly string MessageName;

            public BaseRequest(string id, string messageName) {
                Id = id;
                MessageName = messageName;
            }
        }
    }
}
