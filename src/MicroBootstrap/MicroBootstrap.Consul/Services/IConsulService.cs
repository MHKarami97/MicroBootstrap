using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MicroBootstrap.Consul.Models;

namespace MicroBootstrap.Consul.Services
{
    public interface IConsulService
    {
        Task<HttpResponseMessage> RegisterServiceAsync(ServiceRegistration registration);
        Task<HttpResponseMessage> DeregisterServiceAsync(string id);
        Task<IDictionary<string, ServiceAgent>> GetServiceAgentsAsync(string service = null);
    }
}