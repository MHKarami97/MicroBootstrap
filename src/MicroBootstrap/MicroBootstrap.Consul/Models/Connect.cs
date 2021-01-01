using System.Text.Json.Serialization;

namespace MicroBootstrap.Consul.Models
{
    public class Connect
    {
        [JsonPropertyName("sidecar_service")]
        public SidecarService SidecarService { get; set; }
    }

}