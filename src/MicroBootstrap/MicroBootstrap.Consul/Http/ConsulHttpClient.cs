using System.Net.Http;
using MicroBootstrap.HTTP;

namespace MicroBootstrap.Consul.Http
{
    internal sealed class ConsulHttpClient : MBHttpClient, IConsulHttpClient
    {
        public ConsulHttpClient(HttpClient client, HttpClientOptions options,
            ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
            : base(client, options, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}