namespace Decodey.Models
{
    public class GameSettings
    {
        // Default settings
        public string Theme { get; set; } = "light";
        public string Difficulty { get; set; } = "medium";
        public bool LongText { get; set; } = false;
        public bool HardcoreMode { get; set; } = false;
        public string GridSorting { get; set; } = "default";
        public string TextColor { get; set; } = "default";
        public bool SoundEnabled { get; set; } = true;
        public bool VibrationEnabled { get; set; } = true;
        public bool BackdoorMode { get; set; } = false;
        public string MobileMode { get; set; } = "auto";

        // Clone the settings
        public GameSettings Clone()
        {
            return new GameSettings
            {
                Theme = this.Theme,
                Difficulty = this.Difficulty,
                LongText = this.LongText,
                HardcoreMode = this.HardcoreMode,
                GridSorting = this.GridSorting,
                TextColor = this.TextColor,
                SoundEnabled = this.SoundEnabled,
                VibrationEnabled = this.VibrationEnabled,
                BackdoorMode = this.BackdoorMode,
                MobileMode = this.MobileMode
            };
        }
    }
}