using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MicroBootstrap.Authentication
{
    public static class Extensions
    {
        private const string SectionName = "jwt";
        private const string RegistryName = "auth";

        public static IServiceCollection AddJwt(this IServiceCollection serviceCollection, string sectionName = SectionName,
            Action<JwtBearerOptions> optionsFactory = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            var options = serviceCollection.GetOptions<JwtOptions>(SectionName);
            serviceCollection.AddSingleton(options);
            serviceCollection.AddJwt(options, optionsFactory);
            return serviceCollection;
        }

        private static IServiceCollection AddJwt(this IServiceCollection serviceCollection, JwtOptions options,
            Action<JwtBearerOptions> optionsFactory = null)
        {
            serviceCollection.AddSingleton<IJwtHandler, JwtHandler>();
            serviceCollection.AddSingleton<IAccessTokenService, RedisAccessTokenService>();
            serviceCollection.AddTransient<AccessTokenValidatorMiddleware>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireAudience = options.RequireAudience,
                ValidIssuer = options.ValidIssuer,
                ValidIssuers = options.ValidIssuers,
                ValidateActor = options.ValidateActor,
                ValidAudience = options.ValidAudience,
                ValidAudiences = options.ValidAudiences,
                ValidateAudience = options.ValidateAudience,
                ValidateIssuer = options.ValidateIssuer,
                ValidateLifetime = options.ValidateLifetime,
                ValidateTokenReplay = options.ValidateTokenReplay,
                ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
                SaveSigninToken = options.SaveSigninToken,
                RequireExpirationTime = options.RequireExpirationTime,
                RequireSignedTokens = options.RequireSignedTokens,
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrWhiteSpace(options.AuthenticationType))
            {
                tokenValidationParameters.AuthenticationType = options.AuthenticationType;
            }

            var hasCertificate = false;
            if (options.Certificate is { })
            {
                X509Certificate2 certificate = null;
                var password = options.Certificate.Password;
                var hasPassword = !string.IsNullOrWhiteSpace(password);
                if (!string.IsNullOrWhiteSpace(options.Certificate.Location))
                {
                    certificate = hasPassword
                        ? new X509Certificate2(options.Certificate.Location, password)
                        : new X509Certificate2(options.Certificate.Location);
                    Console.WriteLine($"Loaded X.509 certificate from location: '{options.Certificate.Location}'.");
                }

                if (!string.IsNullOrWhiteSpace(options.Certificate.RawData))
                {
                    var rawData = Convert.FromBase64String(options.Certificate.RawData);
                    certificate = hasPassword
                        ? new X509Certificate2(rawData, password)
                        : new X509Certificate2(rawData);
                    Console.WriteLine("Loaded X.509 certificate from raw data.");
                }

                if (certificate is { })
                {
                    if (string.IsNullOrWhiteSpace(options.Algorithm))
                    {
                        options.Algorithm = SecurityAlgorithms.RsaSha256;
                    }

                    hasCertificate = true;
                    tokenValidationParameters.IssuerSigningKey = new X509SecurityKey(certificate);
                    Console.WriteLine("Using X.509 certificate for issuing tokens.");
                }
            }

            if (!string.IsNullOrWhiteSpace(options.IssuerSigningKey))
            {
                if (string.IsNullOrWhiteSpace(options.Algorithm) || hasCertificate)
                {
                    options.Algorithm = SecurityAlgorithms.HmacSha256;
                }

                var rawKey = Encoding.UTF8.GetBytes(options.IssuerSigningKey);
                tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(rawKey);
                Console.WriteLine("Using symmetric encryption for issuing tokens.");
            }

            if (!string.IsNullOrWhiteSpace(options.NameClaimType))
            {
                tokenValidationParameters.NameClaimType = options.NameClaimType;
            }

            if (!string.IsNullOrWhiteSpace(options.RoleClaimType))
            {
                tokenValidationParameters.RoleClaimType = options.RoleClaimType;
            }

            serviceCollection
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.Authority = options.Authority;
                    o.Audience = options.Audience;
                    o.MetadataAddress = options.MetadataAddress;
                    o.SaveToken = options.SaveToken;
                    o.RefreshOnIssuerKeyNotFound = options.RefreshOnIssuerKeyNotFound;
                    o.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    o.IncludeErrorDetails = options.IncludeErrorDetails;
                    o.TokenValidationParameters = tokenValidationParameters;
                    if (!string.IsNullOrWhiteSpace(options.Challenge))
                    {
                        o.Challenge = options.Challenge;
                    }

                    optionsFactory?.Invoke(o);
                });

            serviceCollection.AddSingleton(options);
            serviceCollection.AddSingleton(tokenValidationParameters);
            return serviceCollection;
        }

        public static IApplicationBuilder UseAccessTokenValidator(this IApplicationBuilder app)
            => app.UseMiddleware<AccessTokenValidatorMiddleware>();
        public static long ToTimestamp(this DateTime dateTime)
        {
            var centuryBegin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expectedDate = dateTime.Subtract(new TimeSpan(centuryBegin.Ticks));

            return expectedDate.Ticks / 10000;
        }
    }
}