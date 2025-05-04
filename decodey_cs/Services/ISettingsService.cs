using Decodey.Models;

namespace Decodey.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the current settings
        /// </summary>
        GameSettings GetSettings();

        /// <summary>
        /// Saves the settings
        /// </summary>
        void SaveSettings(GameSettings settings);

        /// <summary>
        /// Event that is raised when settings change
        /// </summary>
        event EventHandler<SettingsChangedEventArgs> SettingsChanged;
    }

    /// <summary>
    /// Event args for settings changed event
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public GameSettings Settings { get; set; }
    }
}