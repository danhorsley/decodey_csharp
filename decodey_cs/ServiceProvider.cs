using System;
using Microsoft.Extensions.DependencyInjection;

namespace Decodey
{
    /// <summary>
    /// Provides access to registered services
    /// </summary>
    public static class ServiceProvider
    {
        /// <summary>
        /// Gets a service of the specified type
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (Application.Current?.MainPage?.Handler?.MauiContext == null)
                return null;

            return Application.Current.MainPage.Handler.MauiContext.Services.GetService<T>();
        }

        /// <summary>
        /// Gets a required service of the specified type
        /// </summary>
        public static T GetRequiredService<T>() where T : class
        {
            if (Application.Current?.MainPage?.Handler?.MauiContext == null)
                throw new InvalidOperationException("Service provider is not available");

            return Application.Current.MainPage.Handler.MauiContext.Services.GetRequiredService<T>();
        }
    }