using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MicroBootstrap.Vault
{
    public interface ICertificatesIssuer
    {
        Task<X509Certificate2> IssueAsync();
    }
}