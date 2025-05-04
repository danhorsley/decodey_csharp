using System;
using Microsoft.Maui.Controls;
using Decodey.Views;
using Decodey.Services;
using Decodey.Views.Dialogs; // Add this for LoginDialog and SignupDialog

namespace Decodey
{
    public partial class AppShell : Shell
    {
        private readonly ISettingsService _settingsService;

        public AppShell()
        {
            InitializeComponent();

            // Get settings service
            _settingsService = ServiceProvider.GetService<ISettingsService>();

            // Register routes for navigation
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Register routes for navigation using Shell
            Routing.RegisterRoute("privacy", typeof(PrivacyPage));
            Routing.RegisterRoute("scoring", typeof(ScoringPage));
            Routing.RegisterRoute("about", typeof(AboutDialog));
            Routing.RegisterRoute("settings", typeof(Decodey.Views.Dialogs.SettingsDialog)); // Use fully qualified name
            Routing.RegisterRoute("login", typeof(LoginDialog));
            Routing.RegisterRoute("signup", typeof(SignupDialog));
        }
    }
}