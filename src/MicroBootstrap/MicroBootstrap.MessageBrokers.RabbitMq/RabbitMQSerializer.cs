using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public class RabbitMQSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public RabbitMQSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public byte[] Serialize<T>(T value)
        {
            var payload = JsonConvert.SerializeObject(value, _settings);
            return Encoding.UTF8.GetBytes(payload);
        }

        public byte[] Serialize(object value)
        {
            var payload = JsonConvert.SerializeObject(value, _settings);
            return Encoding.UTF8.GetBytes(payload);
        }

        public T Deserialize<T>(byte[] value)
        {
            var stringValue = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<T>(stringValue, _settings);
        }

        public object Deserialize(byte[] value)
        {
            var stringValue = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject(stringValue, _settings);
        }
    }
}