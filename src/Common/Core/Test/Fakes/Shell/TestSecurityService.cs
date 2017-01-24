using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public class TestSecurityService : ISecurityService {
        public Task<Credentials> GetUserCredentialsAsync(string authority, string workspaceName, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public bool ValidateX509Certificate(X509Certificate certificate, string message) {
            throw new System.NotImplementedException();
        }

        public bool DeleteUserCredentials(string authority) => true;
    }
}