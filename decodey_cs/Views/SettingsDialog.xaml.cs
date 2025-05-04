using System;
using Microsoft.Maui.Controls;
using Decodey.ViewModels;
using Decodey.Services;

namespace Decodey.Views.Dialogs
{
    public partial class SettingsDialog : ContentPage
    {
        private readonly SettingsViewModel _viewModel;
        private readonly ISoundService _soundService;

        public SettingsDialog()
        {
            InitializeComponent();

            // Get view model
            _viewModel = BindingContext as SettingsViewModel;

            // Get sound service
            _soundService = App.GetService<ISoundService>();

            // Subscribe to back button pressed event
            BackButtonPressed += OnBackButtonPressed;
        }

        protected override bool OnBackButtonPressed()
        {
            // Close the dialog
            CloseDialog();

            // Don't propagate the event
            return true;
        }

        private void OnBackButtonPressed(object sender, EventArgs e)
        {
            // Close the dialog
            CloseDialog();
        }

        private async void CloseDialog()
        {
            // Close the dialog
            await Navigation.PopModalAsync();
        }

        #region Event Handlers

        // Theme radio button checked changed event
        private void OnThemeRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Skip if not checked
            if (!e.Value) return;

            // Get radio button
            RadioButton radioButton = sender as RadioButton;

            // Update theme
            string theme = radioButton.Content.ToString().ToLower();
            _viewModel.Theme = theme;
        }

        // Text color radio button checked changed event
        private void OnTextColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Skip if not checked
            if (!e.Value) return;

            // Get radio button
            RadioButton radioButton = sender as RadioButton;

            // Update text color
            string textColor = radioButton.Content.ToString();

            // Map display name to value
            switch (textColor)
            {
                case "Default":
                    _viewModel.TextColor = "default";
                    break;

                case "Sci-Fi Blue":
                    _viewModel.TextColor = "scifi-blue";
                    break;

                case "Retro Green":
                    _viewModel.TextColor = "retro-green";
                    break;
            }
        }

        // Grid sorting radio button checked changed event
        private void OnGridSortingRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Skip if not checked
            if (!e.Value) return;

            // Get radio button
            RadioButton radioButton = sender as RadioButton;

            // Update grid sorting
            string gridSorting = radioButton.Content.ToString();

            // Map display name to value
            switch (gridSorting)
            {
                case "As Appears in Text":
                    _viewModel.GridSorting = "default";
                    break;

                case "Alphabetical":
                    _viewModel.GridSorting = "alphabetical";
                    break;
            }
        }

        // Hardcore mode checkbox checked changed event
        private void OnHardcoreModeCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Update hardcore mode
            _viewModel.IsHardcoreMode = e.Value;
        }

        // Long text checkbox checked changed event
        private void OnLongTextCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Update long text
            _viewModel.IsLongText = e.Value;
        }

        // Sound enabled checkbox checked changed event
        private void OnSoundEnabledCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            if (e.Value)
            {
                _soundService?.PlaySound(SoundType.Click);
            }

            // Update sound enabled
            _viewModel.IsSoundEnabled = e.Value;
        }

        // Volume slider value changed event
        private void OnVolumeSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Update volume
            _viewModel.SoundVolume = e.NewValue;

            // Update sound service
            _soundService?.SetVolume((float)e.NewValue);

            // Play sound to test volume
            if (e.NewValue > 0 && _viewModel.IsSoundEnabled &&
                Math.Abs(e.NewValue - e.OldValue) > 0.05)
            {
                _soundService?.PlaySound(SoundType.Click);
            }
        }

        #endregion
    }
}