using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Security
{
    public static class Extensions
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services
                .AddSingleton<IEncryptor, Encryptor>()
                .AddSingleton<IHasher, Hasher>()
                .AddSingleton<ISigner, Signer>();

            return services;
        }
    }
}