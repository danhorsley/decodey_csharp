using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Decodey.Views;

namespace Decodey.Services
{
    public interface INavigationService
    {
        Task NavigateTo<T>(IDictionary<string, object> parameters = null) where T : Page;
        Task GoBack();
    }

    public class NavigationService : INavigationService
    {
        public async Task NavigateTo<T>(IDictionary<string, object> parameters = null) where T : Page
        {
            var page = Activator.CreateInstance<T>();

            // Set parameters if any
            if (parameters != null && page.BindingContext != null)
            {
                foreach (var param in parameters)
                {
                    var property = page.BindingContext.GetType().GetProperty(param.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(page.BindingContext, param.Value);
                    }
                }
            }

            // Use Shell navigation if available
            if (Shell.Current != null)
            {
                var routeForType = GetRouteForType(typeof(T));
                if (!string.IsNullOrEmpty(routeForType))
                {
                    await Shell.Current.GoToAsync(routeForType);
                    return;
                }
            }

            // Fallback to page navigation
            var navigation = Application.Current?.MainPage?.Navigation;
            if (navigation != null)
            {
                // Check if the page type is a dialog
                if (typeof(T).Name.EndsWith("Dialog"))
                {
                    await navigation.PushModalAsync(page);
                }
                else
                {
                    await navigation.PushAsync(page);
                }
            }
            else
            {
                // No navigation context available, just set as main page
                Application.Current.MainPage = new NavigationPage(page);
            }
        }

        public async Task GoBack()
        {
            // Try Shell navigation first
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Fallback to page navigation
            var navigation = Application.Current?.MainPage?.Navigation;
            if (navigation != null && navigation.ModalStack.Count > 0)
            {
                await navigation.PopModalAsync();
            }
            else if (navigation != null && navigation.NavigationStack.Count > 1)
            {
                await navigation.PopAsync();
            }
        }

        private string GetRouteForType(Type pageType)
        {
            // Map page types to shell routes
            if (pageType == typeof(GamePage))
                return "//game";
            else if (pageType == typeof(AboutDialog))
                return "about";
            else if (pageType == typeof(LoginDialog))
                return "login";
            else if (pageType == typeof(SignupDialog))
                return "signup";
            else if (pageType == typeof(PrivacyPage))
                return "privacy";
            else if (pageType == typeof(ScoringPage))
                return "scoring";
            else if (pageType == typeof(SettingsDialog))
                return "settings";

            return string.Empty;
        }
    }
}