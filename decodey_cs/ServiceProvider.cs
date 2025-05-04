using System;
using Microsoft.Extensions.DependencyInjection;

namespace Decodey
{
    /// <summary>
    /// Static service provider for accessing services throughout the app
    /// </summary>
    public static class ServiceProvider
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// Initialize the service provider
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Get a service of the specified type
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
                return null;

            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Get a required service of the specified type (throws if not found)
        /// </summary>
        public static T GetRequiredService<T>() where T : class
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider has not been initialized");

            return _serviceProvider.GetRequiredService<T>();
        }
    }
}