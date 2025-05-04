using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Decodey.Models;
using Decodey.Services;

namespace Decodey.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly IGameService _gameService;
        private readonly ISettingsService _settingsService;
        private readonly ISoundService _soundService;
        private readonly IAnimationService _animationService;

        #region Observable Properties

        [ObservableProperty]
        private string _encrypted = "";

        [ObservableProperty]
        private string _display = "";

        [ObservableProperty]
        private int _mistakes;

        [ObservableProperty]
        private int _maxMistakes;

        [ObservableProperty]
        private bool _hasWon;

        [ObservableProperty]
        private bool _hasLost;

        [ObservableProperty]
        private string _selectedEncryptedLetter;

        [ObservableProperty]
        private string _lastCorrectGuess;

        [ObservableProperty]
        private bool _isHintInProgress;

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isLoadingOrCelebrating;

        [ObservableProperty]
        private bool _showWinCelebration;

        [ObservableProperty]
        private string _matrixMessage = "DECODING...";

        [ObservableProperty]
        private Color _matrixColor = Colors.LightGreen;

        [ObservableProperty]
        private bool _isDailyChallenge;

        [ObservableProperty]
        private bool _isHardcoreMode;

        [ObservableProperty]
        private string _attributionText;

        [ObservableProperty]
        private string _winStatusText = "Puzzle Solved!";

        [ObservableProperty]
        private string _gameOverMessage;

        [ObservableProperty]
        private bool _isMenuOpen;

        [ObservableProperty]
        private bool _isTutorialActive;

        [ObservableProperty]
        private int _currentTutorialStep;

        [ObservableProperty]
        private double _charFontSize = 20.0;

        #endregion

        #region Collections

        // Original Game collections
        public ObservableCollection<string> SortedEncryptedLetters { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OriginalLetters { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CorrectlyGuessed { get; } = new ObservableCollection<string>();
        public ObservableCollection<TextLine> TextLines { get; } = new ObservableCollection<TextLine>();

        // Cached values from Game
        private Dictionary<string, int> LetterFrequency { get; } = new Dictionary<string, int>();
        private Dictionary<string, string> GuessedMappings { get; } = new Dictionary<string, string>();
        private Dictionary<string, List<string>> IncorrectGuesses { get; } = new Dictionary<string, List<string>>();

        // UI Display Collections
        private List<List<string>> _encryptedLetterRows;
        public List<List<string>> EncryptedLetterRows
        {
            get => _encryptedLetterRows;
            set => SetProperty(ref _encryptedLetterRows, value);
        }

        private List<List<string>> _guessLetterRows;
        public List<List<string>> GuessLetterRows
        {
            get => _guessLetterRows;
            set => SetProperty(ref _guessLetterRows, value);
        }

        // Win data
        public Game WinData { get; private set; }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task StartNewGame()
        {
            IsLoading = true;
            IsLoadingOrCelebrating = true;
            MatrixMessage = "GENERATING PUZZLE...";

            var settings = _settingsService.GetSettings();
            await _gameService.StartNewGame(settings.LongText, settings.HardcoreMode);

            IsLoading = false;
            IsLoadingOrCelebrating = false;
            ShowWinCelebration = false;
        }

        [RelayCommand]
        private async Task StartDailyChallenge()
        {
            IsLoading = true;
            IsLoadingOrCelebrating = true;
            MatrixMessage = "LOADING DAILY CHALLENGE...";
            IsDailyChallenge = true;

            var settings = _settingsService.GetSettings();
            await _gameService.StartNewGame(true, settings.HardcoreMode);

            IsLoading = false;
            IsLoadingOrCelebrating = false;
            ShowWinCelebration = false;
        }

        [RelayCommand]
        private void EncryptedSelect(string letter)
        {
            _gameService.SelectEncryptedLetter(letter);
        }

        [RelayCommand]
        private async Task SubmitGuess(string letter)
        {
            if (HasWon || HasLost || string.IsNullOrEmpty(SelectedEncryptedLetter)) return;

            // Check if this guess was already tried
            if (IsIncorrectGuess(SelectedEncryptedLetter, letter))
                return;

            await _gameService.SubmitGuess(SelectedEncryptedLetter, letter);
        }

        [RelayCommand]
        private async Task GetHint()
        {
            if (HasWon || HasLost || IsHintInProgress) return;

            IsHintInProgress = true;
            await _gameService.GetHint();
            IsHintInProgress = false;
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        [RelayCommand]
        private void StartTutorial()
        {
            IsTutorialActive = true;
            CurrentTutorialStep = 0;
        }

        [RelayCommand]
        private void NextTutorialStep()
        {
            CurrentTutorialStep++;

            // End tutorial if we're at the last step
            if (CurrentTutorialStep > 5)
            {
                EndTutorial();
            }
        }

        [RelayCommand]
        private void EndTutorial()
        {
            IsTutorialActive = false;

            // Save that tutorial has been completed
            var settings = _settingsService.GetSettings();
            settings.TutorialCompleted = true;
            _settingsService.SaveSettings(settings);
        }

        [RelayCommand]
        private void NavigateToHome()
        {
            // For now, just close celebrations and reset game
            ShowWinCelebration = false;

            // In a more sophisticated implementation, this would navigate to the home page
        }

        [RelayCommand]
        private async Task OpenLeaderboard()
        {
            // In a real implementation, this would navigate to the leaderboard page
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // In a real implementation, this would open the settings dialog
        }

        [RelayCommand]
        private void OpenAbout()
        {
            // In a real implementation, this would open the about dialog
        }

        [RelayCommand]
        private async Task AuthAction()
        {
            // In a real implementation, this would handle login/logout
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ShowLogin()
        {
            // In a real implementation, this would show the login dialog
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ShareResults()
        {
            // In a real implementation, this would share the game results
            await Task.CompletedTask;
        }

        #endregion

        #region Events

        // Events for animations
        public event EventHandler<CorrectGuessEventArgs> CorrectGuessAnimationRequested;
        public event EventHandler<IncorrectGuessEventArgs> IncorrectGuessAnimationRequested;

        #endregion

        #region Constructor

        public GameViewModel(IGameService gameService, ISettingsService settingsService,
                           ISoundService soundService = null, IAnimationService animationService = null)
        {
            _gameService = gameService;
            _settingsService = settingsService;
            _soundService = soundService;
            _animationService = animationService;

            // Initialize letter arrays
            EncryptedLetterRows = new List<List<string>>();
            GuessLetterRows = new List<List<string>>();

            // Initialize letter arrays for original alphabet
            GuessLetterRows = InitializeAlphabetGrid();

            // Subscribe to game service events
            _gameService.GameStateChanged += OnGameStateChanged;

            // Start a new game
            StartNewGameCommand.ExecuteAsync(null);
        }

        #endregion

        #region Game State Handlers

        /// <summary>
        /// Handle game state changes from the game service
        /// </summary>
        private void OnGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            // Get current game state
            var game = e.Game;

            if (game == null)
            {
                // Game was reset
                Encrypted = "";
                Display = "";
                Mistakes = 0;
                MaxMistakes = 0;
                HasWon = false;
                HasLost = false;
                SelectedEncryptedLetter = null;
                LastCorrectGuess = null;

                // Clear collections
                SortedEncryptedLetters.Clear();
                CorrectlyGuessed.Clear();
                LetterFrequency.Clear();
                GuessedMappings.Clear();
                IncorrectGuesses.Clear();
                TextLines.Clear();

                return;
            }

            // Update observable properties
            Encrypted = game.Encrypted;
            Display = game.Display;
            Mistakes = game.Mistakes;
            MaxMistakes = game.MaxMistakes;
            HasWon = game.HasWon;
            HasLost = game.HasLost;
            SelectedEncryptedLetter = game.SelectedEncryptedLetter;
            LastCorrectGuess = game.LastCorrectGuess;
            IsHardcoreMode = game.HardcoreMode;

            // Update game over message if the game has ended
            if (HasWon || HasLost)
            {
                GameOverMessage = HasWon
                    ? "Congratulations! You solved the puzzle!"
                    : "Game Over! You ran out of mistakes.";

                // Show win celebration if the player won
                if (HasWon)
                {
                    WinData = game;
                    AttributionText = $"- {game.Author}, {game.MinorAttribution}";
                    ShowWinCelebration = true;
                    IsLoadingOrCelebrating = true;
                    MatrixMessage = "CONGRATULATIONS!";
                    MatrixColor = Colors.Green;
                }
            }

            // Update collections
            UpdateSortedEncryptedLetters(game);
            UpdateCorrectlyGuessed(game);
            UpdateLetterFrequency(game);
            UpdateGuessedMappings(game);
            UpdateIncorrectGuesses(game);
            UpdateTextLines(game);

            // Update letter grids
            EncryptedLetterRows = game.GetEncryptedLetterRows();

            // Request animation for newly guessed letter
            if (e.HasWon && !string.IsNullOrEmpty(LastCorrectGuess))
            {
                string correctLetter = game.ReverseMapping[LastCorrectGuess];
                OnCorrectGuessAnimationRequested(LastCorrectGuess, correctLetter);
            }
        }

        /// <summary>
        /// Update the sorted encrypted letters collection
        /// </summary>
        private void UpdateSortedEncryptedLetters(Game game)
        {
            SortedEncryptedLetters.Clear();

            if (string.IsNullOrEmpty(game.Encrypted)) return;

            // Get unique encrypted letters
            var uniqueLetters = game.Encrypted
                .Where(char.IsLetter)
                .Select(c => c.ToString())
                .Distinct()
                .ToList();

            // Sort according to settings
            var settings = _settingsService.GetSettings();
            if (settings.GridSorting == "alphabetical")
                uniqueLetters.Sort();

            // Add to collection
            foreach (var letter in uniqueLetters)
                SortedEncryptedLetters.Add(letter);
        }

        /// <summary>
        /// Update the correctly guessed letters collection
        /// </summary>
        private void UpdateCorrectlyGuessed(Game game)
        {
            CorrectlyGuessed.Clear();

            if (game.CorrectlyGuessed == null) return;

            foreach (var letter in game.CorrectlyGuessed)
                CorrectlyGuessed.Add(letter);
        }

        /// <summary>
        /// Update the letter frequency dictionary
        /// </summary>
        private void UpdateLetterFrequency(Game game)
        {
            LetterFrequency.Clear();

            if (game.LetterFrequency == null) return;

            foreach (var kvp in game.LetterFrequency)
                LetterFrequency[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Update the guessed mappings dictionary
        /// </summary>
        private void UpdateGuessedMappings(Game game)
        {
            GuessedMappings.Clear();

            if (game.GuessedMappings == null) return;

            foreach (var kvp in game.GuessedMappings)
                GuessedMappings[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Update the incorrect guesses dictionary
        /// </summary>
        private void UpdateIncorrectGuesses(Game game)
        {
            IncorrectGuesses.Clear();

            if (game.IncorrectGuesses == null) return;

            foreach (var kvp in game.IncorrectGuesses)
                IncorrectGuesses[kvp.Key] = new List<string>(kvp.Value);
        }

        /// <summary>
        /// Update the text lines collection
        /// </summary>
        private void UpdateTextLines(Game game)
        {
            TextLines.Clear();

            var lines = game.GetTextLines();
            foreach (var line in lines)
                TextLines.Add(line);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Initialize the alphabet grid for the guess letters
        /// </summary>
        private List<List<string>> InitializeAlphabetGrid()
        {
            List<string> alphabet = new List<string>
            {
                "A", "B", "C", "D", "E", "F", "G",
                "H", "I", "J", "K", "L", "M", "N",
                "O", "P", "Q", "R", "S", "T", "U",
                "V", "W", "X", "Y", "Z"
            };

            // Arrange in rows of 7
            const int columnsPerRow = 7;
            List<List<string>> rows = new List<List<string>>();

            for (int i = 0; i < alphabet.Count; i += columnsPerRow)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < columnsPerRow && i + j < alphabet.Count; j++)
                {
                    row.Add(alphabet[i + j]);
                }
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Get the frequency of a letter
        /// </summary>
        public int GetFrequency(char letter)
        {
            string key = letter.ToString();
            return LetterFrequency.TryGetValue(key, out int frequency) ? frequency : 0;
        }

        /// <summary>
        /// Check if a letter is selected
        /// </summary>
        public bool IsEncryptedSelected(char letter)
        {
            return letter.ToString() == SelectedEncryptedLetter;
        }

        /// <summary>
        /// Check if a letter has been guessed
        /// </summary>
        public bool IsLetterGuessed(char letter)
        {
            return CorrectlyGuessed.Contains(letter.ToString());
        }

        /// <summary>
        /// Check if a guess is incorrect
        /// </summary>
        public bool IsIncorrectGuess(string encryptedLetter, string guessedLetter)
        {
            if (string.IsNullOrEmpty(encryptedLetter) || string.IsNullOrEmpty(guessedLetter))
                return false;

            if (IncorrectGuesses.TryGetValue(encryptedLetter, out var incorrectList))
                return incorrectList.Contains(guessedLetter);

            return false;
        }

        /// <summary>
        /// Update screen size and orientation
        /// </summary>
        public void UpdateScreenSize(double width, double height, object orientation)
        {
            // Adjust font size based on screen width
            if (width < 400)
                CharFontSize = 16.0;
            else if (width < 600)
                CharFontSize = 18.0;
            else
                CharFontSize = 20.0;
        }

        /// <summary>
        /// Request animation for correct guess
        /// </summary>
        private void OnCorrectGuessAnimationRequested(string encryptedLetter, string guessedLetter)
        {
            CorrectGuessAnimationRequested?.Invoke(this, new CorrectGuessEventArgs
            {
                EncryptedLetter = encryptedLetter,
                GuessedLetter = guessedLetter
            });
        }

        /// <summary>
        /// Request animation for incorrect guess
        /// </summary>
        private void OnIncorrectGuessAnimationRequested(string guessedLetter)
        {
            IncorrectGuessAnimationRequested?.Invoke(this, new IncorrectGuessEventArgs
            {
                GuessedLetter = guessedLetter
            });
        }

        #endregion

        #region Display Properties

        // Status Text
        public string HintText => $"{MaxMistakes - Mistakes}";

        // Win Stats
        public string ScoreText => WinData != null ? $"Score: {WinData.CalculateScore()}" : "";
        public string TimeText => WinData != null ? WinData.GetTimeString() : "";
        public string MistakesText => WinData != null ? $"{WinData.Mistakes}/{WinData.MaxMistakes}" : "";
        public string DifficultyText => WinData != null ? WinData.Difficulty.ToUpperFirst() : "";
        public string RatingText => GetRatingText();

        // Streak Info (would be implemented in a real app)
        public string CurrentStreakText => "1";
        public string BestStreakText => "1";

        // User Info
        public string UserDisplayName => "Guest";
        public bool IsAuthenticated => false;
        public string AppVersion => "v1.0";
        public string AuthIcon => IsAuthenticated ? "👤" : "🔑";
        public string AuthActionLabel => IsAuthenticated ? "Logout" : "Login / Signup";

        // UI States
        public bool HasActiveGame => !HasWon && !HasLost && !string.IsNullOrEmpty(Encrypted);
        public bool HasError => false;
        public bool HasAttribution => !string.IsNullOrEmpty(AttributionText);
        public bool IsStatsCalculated => WinData != null;
        public bool ShouldShowLoginPrompt => !IsAuthenticated && HasWon;
        public bool HasGameEnded => HasWon || HasLost;

        private string GetRatingText()
        {
            if (WinData == null)
                return "";

            int score = WinData.CalculateScore();

            if (score >= 1000)
                return "⭐⭐⭐ MASTER";
            else if (score >= 800)
                return "⭐⭐ EXPERT";
            else if (score >= 600)
                return "⭐ SKILLED";
            else if (score >= 400)
                return "PROFICIENT";
            else
                return "NOVICE";
        }

        #endregion
    }

    #region Animation Event Args

    public class CorrectGuessEventArgs : EventArgs
    {
        public string EncryptedLetter { get; set; }
        public string GuessedLetter { get; set; }
    }

    public class IncorrectGuessEventArgs : EventArgs
    {
        public string GuessedLetter { get; set; }
    }

    #endregion

    #region Extensions

    public static class StringExtensions
    {
        public static string ToUpperFirst(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
    }

    #endregion
}