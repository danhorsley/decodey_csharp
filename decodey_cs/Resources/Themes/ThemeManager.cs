using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Decodey.Resources.Themes
{
    /// <summary>
    /// Manages theme switching and theme-related resources
    /// Converted from React theme handling logic
    /// </summary>
    public class ThemeManager : INotifyPropertyChanged
    {
        // Singleton instance
        private static ThemeManager _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        // Available themes
        public enum ThemeType
        {
            Light,
            Dark
        }

        // Available text colors
        public enum TextColorType
        {
            Default,
            ScifiBlue,
            RetroGreen
        }

        // Current theme and color
        private ThemeType _currentTheme = ThemeType.Light;
        private TextColorType _currentTextColor = TextColorType.Default;

        // Theme change notification event
        public event PropertyChangedEventHandler PropertyChanged;

        // Observable properties
        public ThemeType CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged(nameof(CurrentTheme));
                    ApplyTheme();
                }
            }
        }

        public TextColorType CurrentTextColor
        {
            get => _currentTextColor;
            set
            {
                if (_currentTextColor != value)
                {
                    _currentTextColor = value;
                    OnPropertyChanged(nameof(CurrentTextColor));
                    ApplyTextColor();
                }
            }
        }

        // Settings convenience properties
        public bool IsDarkTheme => CurrentTheme == ThemeType.Dark;
        public bool IsScifiBlue => CurrentTextColor == TextColorType.ScifiBlue;
        public bool IsRetroGreen => CurrentTextColor == TextColorType.RetroGreen;

        // Constructor is private for singleton pattern
        private ThemeManager()
        {
            // Load theme settings from preferences
            LoadThemeSettings();
        }

        // Load theme settings from device preferences
        private void LoadThemeSettings()
        {
            try
            {
                // Try to get settings from preferences
                if (Preferences.ContainsKey("theme"))
                {
                    string themeValue = Preferences.Get("theme", "light");
                    _currentTheme = themeValue.ToLower() == "dark" ? ThemeType.Dark : ThemeType.Light;
                }
                else
                {
                    // Default to system preference
                    _currentTheme = Application.Current.RequestedTheme == AppTheme.Dark ?
                        ThemeType.Dark : ThemeType.Light;
                }

                // Load text color setting
                if (Preferences.ContainsKey("textColor"))
                {
                    string colorValue = Preferences.Get("textColor", "default");
                    _currentTextColor = colorValue.ToLower() switch
                    {
                        "scifi-blue" => TextColorType.ScifiBlue,
                        "retro-green" => TextColorType.RetroGreen,
                        _ => TextColorType.Default
                    };
                }

                // Apply the loaded theme
                ApplyTheme();
                ApplyTextColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading theme settings: {ex.Message}");
                // Default to light theme on error
                _currentTheme = ThemeType.Light;
                _currentTextColor = TextColorType.Default;
            }
        }

        // Apply the current theme to the application
        public void ApplyTheme()
        {
            if (Application.Current == null) return;

            // Save theme selection to preferences
            Preferences.Set("theme", _currentTheme.ToString().ToLower());

            // Set the app's theme
            Application.Current.UserAppTheme = _currentTheme == ThemeType.Dark ?
                AppTheme.Dark : AppTheme.Light;

            // Update UI elements
            var resources = Application.Current.Resources;
            UpdateSystemColors();
        }

        // Apply the current text color scheme
        public void ApplyTextColor()
        {
            if (Application.Current == null) return;

            // Save color selection to preferences
            string colorValue = _currentTextColor switch
            {
                TextColorType.ScifiBlue => "scifi-blue",
                TextColorType.RetroGreen => "retro-green",
                _ => "default"
            };
            Preferences.Set("textColor", colorValue);

            // Text color is primarily handled in XAML via app resources
            // This method triggers events that views can listen to for color changes
            OnPropertyChanged(nameof(IsScifiBlue));
            OnPropertyChanged(nameof(IsRetroGreen));
        }

        // Update system UI colors for Android/iOS status bars
        private void UpdateSystemColors()
        {
            // Set status bar color based on theme
            // This is platform specific and would need to be implemented per platform
            Microsoft.Maui.ApplicationModel.StatusBar.SetStatusBarColor(
                _currentTheme == ThemeType.Dark ?
                    Colors.Black :
                    Color.FromRgb(248, 249, 250));

            // Set status bar style (light/dark text)
            Microsoft.Maui.ApplicationModel.StatusBar.SetStatusBarStyle(
                _currentTheme == ThemeType.Light ?
                    Microsoft.Maui.ApplicationModel.StatusBarStyle.DarkContent :
                    Microsoft.Maui.ApplicationModel.StatusBarStyle.LightContent);
        }

        // Toggle between light and dark themes
        public void ToggleTheme()
        {
            CurrentTheme = _currentTheme == ThemeType.Light ?
                ThemeType.Dark : ThemeType.Light;
        }

        // Set the theme based on a string value (for settings loading)
        public void SetTheme(string themeName)
        {
            CurrentTheme = themeName?.ToLower() == "dark" ?
                ThemeType.Dark : ThemeType.Light;
        }

        // Set the text color based on a string value
        public void SetTextColor(string colorName)
        {
            CurrentTextColor = colorName?.ToLower() switch
            {
                "scifi-blue" => TextColorType.ScifiBlue,
                "retro-green" => TextColorType.RetroGreen,
                _ => TextColorType.Default
            };
        }

        // Property changed notification method
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Get appropriate resource dictionary for the current theme
        public ResourceDictionary GetThemeResources()
        {
            return _currentTheme == ThemeType.Dark ?
                Application.Current.Resources.MergedDictionaries.First(d => d.Source?.OriginalString?.Contains("DarkTheme") == true) :
                Application.Current.Resources.MergedDictionaries.First(d => d.Source?.OriginalString?.Contains("LightTheme") == true);
        }
    }
}