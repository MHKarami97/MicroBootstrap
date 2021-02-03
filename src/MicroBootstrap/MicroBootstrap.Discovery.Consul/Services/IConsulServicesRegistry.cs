using System.Threading.Tasks;
using MicroBootstrap.Discovery.Consul.Models;

namespace MicroBootstrap.Discovery.Consul.Services
{
    public interface IConsulServicesRegistry
    {
        Task<ServiceAgent> GetAsync(string name);
    }
}