using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MicroBootstrap.Consul.Services;

namespace MicroBootstrap.Consul.MessageHandlers
{
    internal sealed class ConsulServiceDiscoveryMessageHandler : DelegatingHandler
    {
        private readonly IConsulServicesRegistry _servicesRegistry;
        private readonly ConsulOptions _options;
        private readonly string _serviceName;
        private readonly bool? _overrideRequestUri;

        public ConsulServiceDiscoveryMessageHandler(IConsulServicesRegistry servicesRegistry,
            ConsulOptions options, string serviceName = null, bool? overrideRequestUri = null)
        {
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                throw new InvalidOperationException("Consul URL was not provided.");
            }

            _servicesRegistry = servicesRegistry;
            _options = options;
            _serviceName = serviceName;
            _overrideRequestUri = overrideRequestUri;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var uri = GetUri(request);
            var serviceName = string.IsNullOrWhiteSpace(_serviceName) ? uri.Host : _serviceName;

            return await SendAsync(request, serviceName, uri, cancellationToken);
        }

        private Uri GetUri(HttpRequestMessage request)
            => string.IsNullOrWhiteSpace(_serviceName)
                ? request.RequestUri
                : _overrideRequestUri == true
                    ? new Uri(
                        $"{request.RequestUri.Scheme}://{_serviceName}/{request.RequestUri.Host}{request.RequestUri.PathAndQuery}")
                    : request.RequestUri;

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            string serviceName, Uri uri, CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return await base.SendAsync(request, cancellationToken);
            }
            //before we send a request we ask consul to give us a available instance and we override original request uri
            request.RequestUri = await GetRequestUriAsync(request, serviceName, uri);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<Uri> GetRequestUriAsync(HttpRequestMessage request,
            string serviceName, Uri uri)
        {
            //ask the consul registry give me a instance of service from available instances (pick first from unused instance) with 'client side load balancing' or with some sort of work like load balancing then override request address with one provide by consul.
            var service = await _servicesRegistry.GetAsync(serviceName);
            if (service is null)
            {
                throw new ConsulServiceNotFoundException($"Consul service: '{serviceName}' was not found.",
                    serviceName);
            }

            if (!_options.SkipLocalhostDockerDnsReplace)
            {
                service.Address = service.Address.Replace("docker.for.mac.localhost", "localhost")
                    .Replace("docker.for.win.localhost", "localhost")
                    .Replace("host.docker.internal", "localhost");
            }

            var uriBuilder = new UriBuilder(uri)
            {
                Host = service.Address,
                Port = service.Port
            };

            return uriBuilder.Uri;
        }
    }
}