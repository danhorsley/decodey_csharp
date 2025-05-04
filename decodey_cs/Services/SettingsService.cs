using System.Text.Json;
using Decodey.Models;

namespace Decodey.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SettingsKey = "uncrypt-settings";
        private GameSettings _cachedSettings;

        public SettingsService()
        {
            // Load settings on startup
            _cachedSettings = LoadSettingsFromStorage();
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public GameSettings GetSettings()
        {
            return _cachedSettings.Clone();
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        public void SaveSettings(GameSettings settings)
        {
            if (settings == null)
                return;

            // Update cached settings
            _cachedSettings = settings.Clone();

            // Save to storage
            SaveSettingsToStorage(settings);

            // Raise settings changed event
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Settings = settings });
        }

        /// <summary>
        /// Loads settings from device storage
        /// </summary>
        private GameSettings LoadSettingsFromStorage()
        {
            try
            {
                // Try to get settings from preferences
                string settingsJson = Preferences.Get(SettingsKey, null);

                if (!string.IsNullOrEmpty(settingsJson))
                {
                    // Deserialize settings
                    var settings = JsonSerializer.Deserialize<GameSettings>(settingsJson);
                    if (settings != null)
                        return settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if loading fails
            return new GameSettings();
        }

        /// <summary>
        /// Saves settings to device storage
        /// </summary>
        private void SaveSettingsToStorage(GameSettings settings)
        {
            try
            {
                // Serialize settings
                string settingsJson = JsonSerializer.Serialize(settings);

                // Save to preferences
                Preferences.Set(SettingsKey, settingsJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Event that is raised when settings change
        /// </summary>
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;
    }

    /// <summary>
    /// Event args for settings changed event
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public GameSettings Settings { get; set; }
    }
}