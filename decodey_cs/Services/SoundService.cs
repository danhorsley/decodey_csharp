using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;

namespace Decodey.Services
{
    /// <summary>
    /// Enum for the different sound types used in the game
    /// </summary>
    public enum SoundType
    {
        Click,
        Correct,
        Incorrect,
        Hint,
        Win,
        Lose
    }

    /// <summary>
    /// Interface for sound service
    /// </summary>
    public interface ISoundService
    {
        /// <summary>
        /// Play a sound effect
        /// </summary>
        void PlaySound(SoundType soundType);

        /// <summary>
        /// Enable or disable sound
        /// </summary>
        void SetSoundEnabled(bool enabled);

        /// <summary>
        /// Set the volume level (0.0 to 1.0)
        /// </summary>
        void SetVolume(float volume);
    }

    /// <summary>
    /// Service for playing sound effects in the game
    /// </summary>
    public class SoundService : ISoundService, IDisposable
    {
        private readonly IAudioManager _audioManager;
        private readonly Dictionary<SoundType, IAudioPlayer> _players = new Dictionary<SoundType, IAudioPlayer>();
        private bool _soundEnabled = true;
        private float _volume = 0.7f;

        /// <summary>
        /// Constructor loads sound resources
        /// </summary>
        public SoundService()
        {
            _audioManager = AudioManager.Current;
            InitializeSounds();
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySound(SoundType soundType)
        {
            if (!_soundEnabled) return;

            if (_players.TryGetValue(soundType, out var player))
            {
                try
                {
                    player.Volume = _volume;
                    player.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Enable or disable sound
        /// </summary>
        public void SetSoundEnabled(bool enabled)
        {
            _soundEnabled = enabled;
        }

        /// <summary>
        /// Set the volume level (0.0 to 1.0)
        /// </summary>
        public void SetVolume(float volume)
        {
            _volume = Math.Clamp(volume, 0.0f, 1.0f);
        }

        /// <summary>
        /// Load all sound resources
        /// </summary>
        private void InitializeSounds()
        {
            Task.Run(async () =>
            {
                try
                {
                    // Load all sounds asynchronously
                    await LoadSoundAsync(SoundType.Click, "Sounds/click.mp3");
                    await LoadSoundAsync(SoundType.Correct, "Sounds/correct.mp3");
                    await LoadSoundAsync(SoundType.Incorrect, "Sounds/incorrect.mp3");
                    await LoadSoundAsync(SoundType.Hint, "Sounds/hint.mp3");
                    await LoadSoundAsync(SoundType.Win, "Sounds/win.mp3");
                    await LoadSoundAsync(SoundType.Lose, "Sounds/lose.mp3");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading sounds: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Load a single sound resource
        /// </summary>
        private async Task LoadSoundAsync(SoundType soundType, string resourcePath)
        {
            try
            {
                // Check if file exists
                if (!await FileSystem.AppPackageFileExistsAsync(resourcePath))
                {
                    Console.WriteLine($"Sound file not found: {resourcePath}");
                    return;
                }

                // Get resource stream
                using var stream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

                // Create player
                var player = _audioManager.CreatePlayer(stream);

                // Configure player
                player.Volume = _volume;

                // Store player for later use
                _players[soundType] = player;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Sound file not found: {resourcePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound {resourcePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose all audio players
        /// </summary>
        public void Dispose()
        {
            foreach (var player in _players.Values)
            {
                player?.Dispose();
            }

            _players.Clear();
        }
    }
}