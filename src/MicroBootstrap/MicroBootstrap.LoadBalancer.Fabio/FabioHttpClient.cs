using System.Net.Http;
using MicroBootstrap.HTTP;

namespace MicroBootstrap.LoadBalancer.Fabio
{
    internal sealed class FabioHttpClient : MBHttpClient, IFabioHttpClient
    {
        public FabioHttpClient(HttpClient client, HttpClientOptions options,
            ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
            : base(client, options, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}