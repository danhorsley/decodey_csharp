using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.Communication;

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
        try
        {
            // Navigate to scoring page
            await Shell.Current.GoToAsync("scoring");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation error: {ex.Message}");
            await DisplayAlert("Error", "Could not navigate to Scoring page.", "OK");
        }
    }

    private async void OnPrivacyClicked(object sender, EventArgs e)
    {
        try
        {
            // Navigate to privacy page
            await Shell.Current.GoToAsync("privacy");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation error: {ex.Message}");
            await DisplayAlert("Error", "Could not navigate to Privacy page.", "OK");
        }
    }

    private async void OnContactSupportClicked(object sender, EventArgs e)
    {
        // Open email client with support email
        try
        {
            var message = new EmailMessage
            {
                Subject = "Decodey Support Request",
                Body = "",
                To = new List<string> { "support@decodey.com" }
            };

            await Email.ComposeAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error launching email: {ex}");
            await DisplayAlert("Error", "Could not open email client. Please email support@decodey.com directly.", "OK");
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // Close the dialog by popping the modal page
        await Navigation.PopModalAsync();
    }
}
}