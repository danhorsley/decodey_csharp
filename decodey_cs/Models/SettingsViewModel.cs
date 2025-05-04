using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Decodey.Models;
using Decodey.Services;
using Decodey.Helpers; // Add this line for StatusBar

namespace Decodey.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IGameService _gameService;

        // Observable properties for settings
        [ObservableProperty]
        private string _theme;

        [ObservableProperty]
        private string _difficulty;

        [ObservableProperty]
        private bool _longText;

        [ObservableProperty]
        private bool _hardcoreMode;

        [ObservableProperty]
        private string _gridSorting;

        [ObservableProperty]
        private string _textColor;

        [ObservableProperty]
        private bool _soundEnabled;

        [ObservableProperty]
        private bool _vibrationEnabled;

        [ObservableProperty]
        private bool _backdoorMode;

        [ObservableProperty]
        private string _mobileMode = "auto";

        // Collections for UI bindings
        public ObservableCollection<string> ThemeOptions { get; } = new ObservableCollection<string> { "light", "dark" };
        public ObservableCollection<string> DifficultyOptions { get; } = new ObservableCollection<string> { "easy", "medium", "hard" };
        public ObservableCollection<string> GridSortingOptions { get; } = new ObservableCollection<string> { "default", "alphabetical" };
        public ObservableCollection<string> MobileModeOptions { get; } = new ObservableCollection<string> { "auto", "always", "never" };

        // Max mistakes dictionary (used for display information)
        public Dictionary<string, int> MaxMistakesMap { get; } = new Dictionary<string, int>
        {
            { "easy", 8 },
            { "medium", 5 },
            { "hard", 3 }
        };

        // State to track changes
        private bool _hasUnsavedChanges = false;
        private bool _settingsInitialized = false;
        private bool _showWarningModal = false;
        private string _pendingSettingType;
        private object _pendingSettingValue;

        public SettingsViewModel(ISettingsService settingsService, IGameService gameService)
        {
            _settingsService = settingsService;
            _gameService = gameService;

            // Load settings from service
            LoadSettings();

            // Set initialized flag
            _settingsInitialized = true;
        }

        /// <summary>
        /// Loads settings from the settings service
        /// </summary>
        private void LoadSettings()
        {
            var settings = _settingsService.GetSettings();

            // Apply settings to observable properties
            Theme = settings.Theme;
            Difficulty = settings.Difficulty;
            LongText = settings.LongText;
            HardcoreMode = settings.HardcoreMode;
            GridSorting = settings.GridSorting;
            TextColor = settings.TextColor;
            SoundEnabled = settings.SoundEnabled;
            VibrationEnabled = settings.VibrationEnabled;
            BackdoorMode = settings.BackdoorMode;
            MobileMode = settings.MobileMode;
        }

        /// <summary>
        /// Saves settings to the settings service
        /// </summary>
        [RelayCommand]
        public void SaveSettings()
        {
            if (!_hasUnsavedChanges)
                return;

            // Create settings object
            var settings = new GameSettings
            {
                Theme = Theme,
                Difficulty = Difficulty,
                LongText = LongText,
                HardcoreMode = HardcoreMode,
                GridSorting = GridSorting,
                TextColor = TextColor,
                SoundEnabled = SoundEnabled,
                VibrationEnabled = VibrationEnabled,
                BackdoorMode = BackdoorMode,
                MobileMode = MobileMode
            };

            // Save settings
            _settingsService.SaveSettings(settings);

            // Apply theme changes
            ApplyTheme();

            // Reset unsaved changes flag
            _hasUnsavedChanges = false;
        }

        /// <summary>
        /// Cancels changes and reloads original settings
        /// </summary>
        [RelayCommand]
        public void CancelChanges()
        {
            // Reload settings from service
            LoadSettings();

            // Reset unsaved changes flag
            _hasUnsavedChanges = false;
        }

        /// <summary>
        /// Handles setting changes with potential warnings for gameplay-affecting settings
        /// </summary>
        /// <param name="settingType">The setting type being changed</param>
        /// <param name="value">The new value</param>
        public void HandleSettingChange(string settingType, object value)
        {
            // If we're not initialized yet, don't handle changes
            if (!_settingsInitialized)
                return;

            // Check if this is a gameplay-affecting setting
            var gameplaySettings = new[] { "Difficulty", "HardcoreMode", "LongText" };

            // If game is in progress and this is a gameplay setting, show warning
            if (_gameService.HasGameStarted && gameplaySettings.Contains(settingType))
            {
                // Store pending change
                _pendingSettingType = settingType;
                _pendingSettingValue = value;

                // Show warning modal
                _showWarningModal = true;
                OnPropertyChanged(nameof(ShowWarningModal));
            }
            else
            {
                // Apply change directly
                ApplySettingChange(settingType, value);
            }
        }

        /// <summary>
        /// Applies a setting change directly
        /// </summary>
        /// <param name="settingType">The setting type to change</param>
        /// <param name="value">The new value</param>
        private void ApplySettingChange(string settingType, object value)
        {
            // Use reflection to set the property
            var property = GetType().GetProperty(settingType);
            if (property != null)
            {
                // Convert value if needed
                object convertedValue = value;
                if (property.PropertyType == typeof(bool) && value is string)
                {
                    convertedValue = bool.Parse((string)value);
                }

                // Set the property
                property.SetValue(this, convertedValue);

                // Special handling for theme changes
                if (settingType == "Theme")
                {
                    // If we changed to dark theme and text color is default, change to scifi-blue
                    if ((string)value == "dark" && TextColor == "default")
                    {
                        TextColor = "scifi-blue";
                    }

                    // Apply theme changes immediately
                    ApplyTheme();
                }

                // Mark that we have unsaved changes
                _hasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Confirms the pending setting change
        /// </summary>
        [RelayCommand]
        public void ConfirmSettingChange()
        {
            if (_pendingSettingType != null)
            {
                // Apply the pending change
                ApplySettingChange(_pendingSettingType, _pendingSettingValue);

                // Reset pending change
                _pendingSettingType = null;
                _pendingSettingValue = null;

                // Hide warning modal
                _showWarningModal = false;
                OnPropertyChanged(nameof(ShowWarningModal));
            }
        }

        /// <summary>
        /// Cancels the pending setting change
        /// </summary>
        [RelayCommand]
        public void CancelSettingChange()
        {
            // Reset pending change
            _pendingSettingType = null;
            _pendingSettingValue = null;

            // Hide warning modal
            _showWarningModal = false;
            OnPropertyChanged(nameof(ShowWarningModal));
        }

        /// <summary>
        /// Applies theme changes to the app
        /// </summary>
        private void ApplyTheme()
        {
            try
            {
                // Apply theme changes to the app
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    // Get the resource dictionaries
                    var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

                    // Remove any existing theme dictionaries
                    var themeDictionaries = mergedDictionaries
                        .Where(d => d.Source?.OriginalString.Contains("Themes/") ?? false)
                        .ToList();

                    foreach (var dict in themeDictionaries)
                    {
                        mergedDictionaries.Remove(dict);
                    }

                    // Add the new theme dictionary
                    var themePath = $"Resources/Styles/Themes/{Theme}Theme.xaml";
                    mergedDictionaries.Add(new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) });

                    // Set system colors for mobile devices if needed
                    if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        var backgroundColor = Theme == "dark" ? Colors.Black : Colors.White;
                        var statusBarColor = Theme == "dark" ? Colors.Black : Colors.White;
                        var statusBarStyle = Theme == "dark" ? StatusBarStyle.LightContent : StatusBarStyle.DarkContent;

                        // Set navigation bar color
                        Microsoft.Maui.Controls.Application.Current.MainPage.BackgroundColor = backgroundColor;

                        // Set status bar style
                        StatusBar.SetStyle(statusBarStyle);
                        StatusBar.SetColor(statusBarColor);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the maximum mistakes allowed for the current difficulty
        /// </summary>
        public int GetMaxMistakes()
        {
            return MaxMistakesMap.TryGetValue(Difficulty, out int value) ? value : 5;
        }

        /// <summary>
        /// Gets whether the warning modal should be shown
        /// </summary>
        public bool ShowWarningModal => _showWarningModal;

        /// <summary>
        /// Gets the warning message for the pending setting change
        /// </summary>
        public string WarningMessage
        {
            get
            {
                if (_pendingSettingType == null)
                    return string.Empty;

                return _pendingSettingType switch
                {
                    "Difficulty" => "Changing difficulty will affect how many mistakes you can make before losing. This change will only apply to your next game.",
                    "HardcoreMode" => "Enabling hardcore mode removes spaces and punctuation, making the game more challenging. This change will only apply to your next game.",
                    "LongText" => "Changing quote length will affect the types of quotes you'll see. This change will only apply to your next game.",
                    _ => "This change will only apply to your next game."
                };
            }
        }

        /// <summary>
        /// Gets whether the current user has backdoor access
        /// </summary>
        public bool HasBackdoorAccess
        {
            get
            {
                var authService = ServiceProvider.GetRequiredService<IAuthService>();
                return authService.IsSubAdmin;
            }
        }
    }
}