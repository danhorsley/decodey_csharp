using System;
using Microsoft.Maui.Controls;

namespace Decodey
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Register routes for navigation using Shell
            Routing.RegisterRoute("privacy", typeof(PrivacyPage));
            Routing.RegisterRoute("scoring", typeof(ScoringPage));
            Routing.RegisterRoute("about", typeof(AboutPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("signup", typeof(SignupPage));

            // Set the startup page - Home by default
            // This can be overridden to show Game directly if there's an active game
            RouteToStartupPage();
        }

        private void RouteToStartupPage()
        {
            try
            {
                // Check if we should show the game directly
                bool hasActiveGame = false;

                // Try to get from preferences or service
                var settingsService = App.GetService<ISettingsService>();
                if (settingsService != null)
                {
                    hasActiveGame = settingsService.GetSetting<bool>("has-active-game", false);
                }

                // Show tutorial for first-time users
                bool tutorialCompleted = false;
                if (settingsService != null)
                {
                    tutorialCompleted = settingsService.GetSetting<bool>("tutorial-completed", false);
                }

                // Determine which page to show
                if (!tutorialCompleted)
                {
                    // First-time user, show game with tutorial
                    Current.GoToAsync("//game");
                }
                else if (hasActiveGame)
                {
                    // Has active game, show game directly
                    Current.GoToAsync("//game");
                }
                else
                {
                    // Show home by default
                    Current.GoToAsync("//home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error routing to startup page: {ex.Message}");

                // Fallback to home page
                Current.GoToAsync("//home");
            }
        }
    }
}