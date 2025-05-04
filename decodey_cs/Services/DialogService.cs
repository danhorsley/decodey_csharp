using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Decodey.Views;
using Decodey.Views.Dialogs;
using Decodey.Models;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for dialog service
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Show an alert dialog
        /// </summary>
        Task ShowAlertAsync(string title, string message, string buttonText);

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText);

        /// <summary>
        /// Show the settings dialog
        /// </summary>
        Task ShowSettingsDialog();

        /// <summary>
        /// Show the about dialog
        /// </summary>
        Task ShowAboutDialog();

        /// <summary>
        /// Show the login dialog
        /// </summary>
        Task<bool> ShowLoginDialog();

        /// <summary>
        /// Show the signup dialog
        /// </summary>
        Task<bool> ShowSignupDialog();

        /// <summary>
        /// Show a dialog to prompt for continuing a game
        /// </summary>
        Task<bool> ShowContinueGameDialog();
    }

    /// <summary>
    /// Service for showing dialogs
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// Show an alert dialog
        /// </summary>
        public async Task ShowAlertAsync(string title, string message, string buttonText)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, buttonText);
        }

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        public async Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, acceptText, cancelText);
        }

        /// <summary>
        /// Show the settings dialog
        /// </summary>
        public async Task ShowSettingsDialog()
        {
            // Create dialog with fully qualified type name to avoid ambiguity
            var dialog = new Decodey.Views.Dialogs.SettingsDialog();

            // Show dialog
            await Application.Current.MainPage.Navigation.PushModalAsync(dialog);
        }

        /// <summary>
        /// Show the about dialog
        /// </summary>
        public async Task ShowAboutDialog()
        {
            // Create dialog
            var dialog = new AboutDialog();

            // Show dialog
            await Application.Current.MainPage.Navigation.PushModalAsync(dialog);
        }

        /// <summary>
        /// Show the login dialog
        /// </summary>
        public async Task<bool> ShowLoginDialog()
        {
            // Create dialog
            var dialog = new LoginDialog();

            // Show dialog
            await Application.Current.MainPage.Navigation.PushModalAsync(dialog);

            // Wait for result
            var result = await dialog.GetResultAsync();

            // Close dialog if still open
            if (dialog.Navigation?.ModalStack.Contains(dialog) == true)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }

            return result;
        }

        /// <summary>
        /// Show the signup dialog
        /// </summary>
        public async Task<bool> ShowSignupDialog()
        {
            // Create dialog
            var dialog = new SignupDialog();

            // Show dialog
            await Application.Current.MainPage.Navigation.PushModalAsync(dialog);

            // Wait for result
            var result = await dialog.GetResultAsync();

            // Close dialog if still open
            if (dialog.Navigation?.ModalStack.Contains(dialog) == true)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }

            return result;
        }

        /// <summary>
        /// Show a dialog to prompt for continuing a game
        /// </summary>
        public async Task<bool> ShowContinueGameDialog()
        {
            return await Application.Current.MainPage.DisplayAlert(
                "Continue Game",
                "You have an active game in progress. Would you like to continue it?",
                "Continue",
                "New Game");
        }
    }
}