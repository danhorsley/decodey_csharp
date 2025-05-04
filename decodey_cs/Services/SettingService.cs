using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.IO;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for settings service
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Get a setting value
        /// </summary>
        T GetSetting<T>(string key, T defaultValue = default);

        /// <summary>
        /// Set a setting value
        /// </summary>
        void SetSetting<T>(string key, T value);

        /// <summary>
        /// Get a setting value asynchronously
        /// </summary>
        Task<T> GetSettingAsync<T>(string key, T defaultValue = default);

        /// <summary>
        /// Set a setting value asynchronously
        /// </summary>
        Task SetSettingAsync<T>(string key, T value);

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        Task ResetSettingsAsync();

        /// <summary>
        /// Export settings to a file
        /// </summary>
        Task<string> ExportSettingsAsync();

        /// <summary>
        /// Import settings from a file
        /// </summary>
        Task ImportSettingsAsync(string filePath);
    }

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

        /// <summary>
        /// Constructor loads settings
        /// </summary>
        public SettingsService()
        {
            // Ensure settings are loaded
            LoadSettings();
        }

        /// <summary>
        /// Get a setting value
        /// </summary>
        public T GetSetting<T>(string key, T defaultValue = default)
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
        /// Set a setting value
        /// </summary>
        public void SetSetting<T>(string key, T value)
        {
            // Ensure settings are loaded
            LoadSettings();

            // Update setting
            _settings[key] = value;

            // Save settings
            SaveSettings();
        }

        /// <summary>
        /// Get a setting value asynchronously
        /// </summary>
        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
        {
            // Ensure settings are loaded
            await LoadSettingsAsync();

            // Get setting
            return GetSetting<T>(key, defaultValue);
        }

        /// <summary>
        /// Set a setting value asynchronously
        /// </summary>
        public async Task SetSettingAsync<T>(string key, T value)
        {
            // Ensure settings are loaded
            await LoadSettingsAsync();

            // Update setting
            _settings[key] = value;

            // Save settings
            await SaveSettingsAsync();
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public async Task ResetSettingsAsync()
        {
            // Create new settings dictionary with defaults
            _settings = new Dictionary<string, object>(_defaultSettings);

            // Save settings
            await SaveSettingsAsync();
        }

        /// <summary>
        /// Export settings to a file
        /// </summary>
        public async Task<string> ExportSettingsAsync()
        {
            try
            {
                // Ensure settings are loaded
                await LoadSettingsAsync();

                // Create JSON string
                var settingsJson = JsonSerializer.Serialize(_settings);

                // Get cache directory
                var cacheDir = FileSystem.CacheDirectory;
                var filePath = Path.Combine(cacheDir, "decodey_settings.json");

                // Write to file
                await File.WriteAllTextAsync(filePath, settingsJson);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Import settings from a file
        /// </summary>
        public async Task ImportSettingsAsync(string filePath)
        {
            try
            {
                // Read file
                var settingsJson = await File.ReadAllTextAsync(filePath);

                // Parse JSON
                var importedSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsJson);

                // Ensure all required settings exist
                foreach (var defaultSetting in _defaultSettings)
                {
                    if (!importedSettings.ContainsKey(defaultSetting.Key))
                    {
                        importedSettings[defaultSetting.Key] = defaultSetting.Value;
                    }
                }

                // Update settings
                _settings = importedSettings;

                // Save settings
                await SaveSettingsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing settings: {ex.Message}");
                throw;
            }
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
        /// Load settings from preferences asynchronously
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            if (_settingsLoaded) return;

            await Task.Run(() => LoadSettings());
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
        /// Save settings to preferences asynchronously
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            await Task.Run(() => SaveSettings());
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