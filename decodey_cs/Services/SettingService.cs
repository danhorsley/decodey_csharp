using System.Text.Json;
using Decodey.Models;

namespace Decodey.Services
{
    /// <summary>
    /// Service for managing application settings
    /// </summary>
    public class SettingsService : ISettingsService
    {
        // Key for storing structured settings in preferences
        private const string SettingsKey = "decodey_settings";

        // Default settings
        private readonly Dictionary<string, object> _defaultSettings = new Dictionary<string, object>
        {
            { "theme", "light" },
            { "textColor", "default" },
            { "sound-enabled", true },
            { "sound-volume", 0.7f },
            { "grid-sorting", "default" },
            { "hardcore-mode", false },
            { "long-text", false },
            { "tutorial-completed", false },
            { "backdoor-mode", false }
        };

        // Current settings
        private Dictionary<string, object> _settings;

        // Settings loaded flag
        private bool _settingsLoaded = false;

        // Event for settings changed
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        // Constructor
        public SettingsService()
        {
            // Ensure settings are loaded
            LoadSettings();
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public GameSettings GetSettings()
        {
            // Create settings object
            var settings = new GameSettings
            {
                Theme = GetSetting<string>("theme", "light"),
                Difficulty = GetSetting<string>("difficulty", "medium"),
                LongText = GetSetting<bool>("long-text", false),
                HardcoreMode = GetSetting<bool>("hardcore-mode", false),
                GridSorting = GetSetting<string>("grid-sorting", "default"),
                TextColor = GetSetting<string>("textColor", "default"),
                SoundEnabled = GetSetting<bool>("sound-enabled", true),
                VibrationEnabled = GetSetting<bool>("vibration-enabled", true),
                BackdoorMode = GetSetting<bool>("backdoor-mode", false),
                MobileMode = GetSetting<string>("mobile-mode", "auto"),
                TutorialCompleted = GetSetting<bool>("tutorial-completed", false)
            };

            return settings;
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        public void SaveSettings(GameSettings settings)
        {
            // Ensure settings are loaded
            LoadSettings();

            // Update settings
            _settings["theme"] = settings.Theme;
            _settings["difficulty"] = settings.Difficulty;
            _settings["long-text"] = settings.LongText;
            _settings["hardcore-mode"] = settings.HardcoreMode;
            _settings["grid-sorting"] = settings.GridSorting;
            _settings["textColor"] = settings.TextColor;
            _settings["sound-enabled"] = settings.SoundEnabled;
            _settings["vibration-enabled"] = settings.VibrationEnabled;
            _settings["backdoor-mode"] = settings.BackdoorMode;
            _settings["mobile-mode"] = settings.MobileMode;
            _settings["tutorial-completed"] = settings.TutorialCompleted;

            // Save settings
            SaveSettings();

            // Raise settings changed event
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Settings = settings });
        }

        /// <summary>
        /// Get a setting value
        /// </summary>
        private T GetSetting<T>(string key, T defaultValue = default)
        {
            // Ensure settings are loaded
            LoadSettings();

            // Check if setting exists
            if (_settings.TryGetValue(key, out var value))
            {
                try
                {
                    // Handle case where stored value is JsonElement (from deserialization)
                    if (value is JsonElement jsonElement)
                    {
                        return DeserializeJsonElement<T>(jsonElement);
                    }

                    // Convert value to requested type
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            // Check default settings if not found in current settings
            if (_defaultSettings.TryGetValue(key, out var defaultSettingValue))
            {
                try
                {
                    // Convert default value to requested type
                    return (T)Convert.ChangeType(defaultSettingValue, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            // Return provided default if not found
            return defaultValue;
        }

        /// <summary>
        /// Load settings from preferences
        /// </summary>
        private void LoadSettings()
        {
            if (_settingsLoaded) return;

            // Get settings JSON from preferences
            var settingsJson = Preferences.Get(SettingsKey, string.Empty);

            // Parse JSON or use defaults
            if (!string.IsNullOrEmpty(settingsJson))
            {
                try
                {
                    _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsJson);

                    // Ensure all required settings exist
                    foreach (var defaultSetting in _defaultSettings)
                    {
                        if (!_settings.ContainsKey(defaultSetting.Key))
                        {
                            _settings[defaultSetting.Key] = defaultSetting.Value;
                        }
                    }
                }
                catch
                {
                    _settings = new Dictionary<string, object>(_defaultSettings);
                }
            }
            else
            {
                _settings = new Dictionary<string, object>(_defaultSettings);
            }

            _settingsLoaded = true;
        }

        /// <summary>
        /// Save settings to preferences
        /// </summary>
        private void SaveSettings()
        {
            // Convert settings to JSON
            var settingsJson = JsonSerializer.Serialize(_settings);

            // Save to preferences
            Preferences.Set(SettingsKey, settingsJson);
        }

        /// <summary>
        /// Deserialize a JsonElement to the specified type
        /// </summary>
        private T DeserializeJsonElement<T>(JsonElement element)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)element.GetBoolean();

                case TypeCode.Int32:
                    return (T)(object)element.GetInt32();

                case TypeCode.Int64:
                    return (T)(object)element.GetInt64();

                case TypeCode.Single:
                    return (T)(object)element.GetSingle();

                case TypeCode.Double:
                    return (T)(object)element.GetDouble();

                case TypeCode.String:
                    return (T)(object)element.GetString();

                default:
                    // For complex types, re-serialize and deserialize
                    return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
        }
    }
}