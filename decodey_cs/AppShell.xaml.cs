using System;
using Microsoft.Maui.Controls;
using Decodey.Views;
using Decodey.Services;

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
            Routing.RegisterRoute("settings", typeof(SettingsDialog));
            Routing.RegisterRoute("login", typeof(LoginDialog));
            Routing.RegisterRoute("signup", typeof(SignupDialog));
        }
    }
}