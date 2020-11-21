using System;
using Microsoft.Extensions.Caching.ServiceStackRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceStackRedisCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceStackRedisCache(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IServiceStackRedisCache, ServiceStackRedisCache>());
            return services;
        }

        public static IServiceCollection AddServiceStackRedisCache(this IServiceCollection services, IConfigurationSection  section)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (section == null)
                throw new ArgumentNullException(nameof(section));
            services.AddServiceStackRedisCache();
            services.Configure<ServiceStackRedisOptions>(section);
            return services;
        }

        public static IServiceCollection AddServiceStackRedisCache(this IServiceCollection services, Action<ServiceStackRedisOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));
            services.AddServiceStackRedisCache();
            services.Configure(setupAction);
            return services;
        }
    }
}
