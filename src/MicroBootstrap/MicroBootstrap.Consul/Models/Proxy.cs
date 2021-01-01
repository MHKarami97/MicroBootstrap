using System.Collections.Generic;

namespace MicroBootstrap.Consul.Models
{
    public class Proxy
    {
        public List<Upstream> Upstreams { get; set; }
    }
}