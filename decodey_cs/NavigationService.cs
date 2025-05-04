using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for navigation service
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigate to a specified page by name
        /// </summary>
        Task NavigateToPage(string pageName, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Navigate back to the previous page
        /// </summary>
        Task NavigateBack();

        /// <summary>
        /// Go to the home page
        /// </summary>
        Task GoToHome();
    }

    /// <summary>
    /// Service for handling navigation between pages
    /// </summary>
    public class NavigationService : INavigationService
    {
        // Map of page names to page types
        private readonly Dictionary<string, Type> _pageMap = new Dictionary<string, Type>
        {
            { "HomePage", typeof(HomePage) },
            { "GamePage", typeof(MainPage) },
            { "LeaderboardPage", typeof(LeaderboardPage) },
            { "PrivacyPage", typeof(PrivacyPage) },
            { "ScoringPage", typeof(ScoringPage) }
        };

        /// <summary>
        /// Navigate to a specified page by name
        /// </summary>
        public async Task NavigateToPage(string pageName, Dictionary<string, object> parameters = null)
        {
            // Check if page exists
            if (!_pageMap.TryGetValue(pageName, out var pageType))
            {
                throw new ArgumentException($"Page '{pageName}' not found");
            }

            // Create page instance
            var page = (Page)Activator.CreateInstance(pageType);

            // Set parameters if any
            if (parameters != null && page.BindingContext != null)
            {
                foreach (var param in parameters)
                {
                    SetProperty(page.BindingContext, param.Key, param.Value);
                }
            }

            // Navigate to page
            if (Application.Current.MainPage is NavigationPage navPage)
            {
                await navPage.PushAsync(page);
            }
            else if (Application.Current.MainPage is Shell shell)
            {
                // Use Shell navigation if available
                var route = GetRouteForPage(pageName);
                if (!string.IsNullOrEmpty(route))
                {
                    await Shell.Current.GoToAsync(route);
                }
                else
                {
                    // Fallback to setting page directly
                    Application.Current.MainPage = new NavigationPage(page);
                }
            }
            else
            {
                // Set as main page with navigation
                Application.Current.MainPage = new NavigationPage(page);
            }
        }

        /// <summary>
        /// Navigate back to the previous page
        /// </summary>
        public async Task NavigateBack()
        {
            if (Application.Current.MainPage is NavigationPage navPage)
            {
                await navPage.PopAsync();
            }
            else if (Application.Current.MainPage is Shell shell)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        /// <summary>
        /// Go to the home page
        /// </summary>
        public async Task GoToHome()
        {
            await NavigateToPage("HomePage");
        }

        /// <summary>
        /// Helper to set a property on an object by name
        /// </summary>
        private void SetProperty(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Get the shell route for a page name
        /// </summary>
        private string GetRouteForPage(string pageName)
        {
            // Map page names to shell routes
            switch (pageName)
            {
                case "HomePage":
                    return "//home";
                case "GamePage":
                    return "//game";
                case "LeaderboardPage":
                    return "//leaderboard";
                default:
                    return string.Empty;
            }
        }
    }
}