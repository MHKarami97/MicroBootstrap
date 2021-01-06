using System.Threading.Tasks;

namespace MicroBootstrap.LoadBalancer.Fabio
{
    public interface IFabioHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}