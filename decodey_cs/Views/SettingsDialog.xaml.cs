using System;
using Microsoft.Maui.Controls;
using Decodey.ViewModels;
using Decodey.Services;

namespace Decodey.Views
{
    public partial class SettingsDialog : ContentPage
    {
        private readonly SettingsViewModel _viewModel;
        private readonly ISoundService _soundService;

        public SettingsDialog()
        {
            InitializeComponent();

            // Get view model
            _viewModel = ServiceProvider.GetService<SettingsViewModel>();

            if (_viewModel == null)
            {
                // Create a view model if not found from service provider
                var settingsService = ServiceProvider.GetService<ISettingsService>();
                var gameService = ServiceProvider.GetService<IGameService>();
                _viewModel = new SettingsViewModel(settingsService, gameService);
            }

            BindingContext = _viewModel;

            // Get sound service
            _soundService = ServiceProvider.GetService<ISoundService>();
        }

        protected override bool OnBackButtonPressed()
        {
            // Save settings before closing
            _viewModel.SaveSettingsCommand.Execute(null);

            // Close the dialog
            CloseDialog();

            // Don't propagate the event
            return true;
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

        // Difficulty radio button checked changed event
        private void OnDifficultyRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Skip if not checked
            if (!e.Value) return;

            // Get radio button
            RadioButton radioButton = sender as RadioButton;

            // Update difficulty
            string difficulty = radioButton.Content.ToString().ToLower().Split(' ')[0];
            _viewModel.Difficulty = difficulty;
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
            _viewModel.HardcoreMode = e.Value;
        }

        // Long text checkbox checked changed event
        private void OnLongTextCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Play click sound
            _soundService?.PlaySound(SoundType.Click);

            // Update long text
            _viewModel.LongText = e.Value;
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
            _viewModel.SoundEnabled = e.Value;
        }

        // Volume slider value changed event
        private void OnVolumeSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Update volume
            _viewModel.SoundVolume = e.NewValue;

            // Update sound service
            _soundService?.SetVolume((float)e.NewValue);

            // Play sound to test volume
            if (e.NewValue > 0 && _viewModel.SoundEnabled &&
                Math.Abs(e.NewValue - e.OldValue) > 0.05)
            {
                _soundService?.PlaySound(SoundType.Click);
            }
        }

        // Handle back button
        public void OnBackButtonPressed(object sender, EventArgs e)
        {
            // Save settings before closing
            _viewModel.SaveSettingsCommand.Execute(null);

            // Close the dialog
            CloseDialog();
        }

        #endregion
    }
}