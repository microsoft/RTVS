using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public class TestSecurityService : ISecurityService {
        public Task<Credentials> GetUserCredentialsAsync(string authority, string workspaceName, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public Task<bool> ValidateX509CertificateAsync(X509Certificate certificate, string message, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public bool DeleteUserCredentials(string authority) => true;
    }
}