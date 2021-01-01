using System.Threading.Tasks;
using MicroBootstrap.Consul.Models;

namespace MicroBootstrap.Consul.Services
{
    public interface IConsulServicesRegistry
    {
        Task<ServiceAgent> GetAsync(string name);
    }
}