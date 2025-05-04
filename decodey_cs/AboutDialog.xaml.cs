using Microsoft.Maui.Controls;
using System;

namespace Decodey.Views
{
    public partial class AboutDialog : ContentPage
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private async void OnScoringClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScoringPage());
        }

        private async void OnPrivacyClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PrivacyPage());
        }

        private void OnContactSupportClicked(object sender, EventArgs e)
        {
            // Open email client with support email
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Decodey Support Request",
                    Body = "",
                    To = new List<string> { "support@uncryptgame.com" }
                };

                Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Could not open email client. Please email support@uncryptgame.com directly.", "OK");
                Console.WriteLine($"Error launching email: {ex}");
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}