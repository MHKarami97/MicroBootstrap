using Microsoft.Extensions.DependencyInjection;
using MicroBootstrap.Serialization;
public static class Extensions
{
    public static IServiceCollection AddSerialization(this IServiceCollection serviceCollection, SerializerType type = SerializerType.SystemTextJson)
    {
        var serializer = SerializerFactory.Create(type);
        serviceCollection.AddSingleton<ISerializer>(serializer);

        return serviceCollection;
    }
}