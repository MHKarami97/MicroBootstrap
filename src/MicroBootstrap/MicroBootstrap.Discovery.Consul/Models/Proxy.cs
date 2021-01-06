using System.Collections.Generic;

namespace MicroBootstrap.Discovery.Consul.Models
{
    public class Proxy
    {
        public List<Upstream> Upstreams { get; set; }
    }
}