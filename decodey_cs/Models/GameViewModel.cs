using System.Collections.ObjectModel;
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

        // Game state
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

        // Letter collections
        public ObservableCollection<string> SortedEncryptedLetters { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OriginalLetters { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CorrectlyGuessed { get; } = new ObservableCollection<string>();

        public Dictionary<string, int> LetterFrequency { get; } = new Dictionary<string, int>();
        public Dictionary<string, string> GuessedMappings { get; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> IncorrectGuesses { get; } = new Dictionary<string, List<string>>();

        public GameViewModel(IGameService gameService, ISettingsService settingsService)
        {
            _gameService = gameService;
            _settingsService = settingsService;

            // Subscribe to game state changes
            _gameService.GameStateChanged += OnGameStateChanged;

            // Start a new game
            StartNewGameCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task StartNewGame()
        {
            IsLoading = true;

            var settings = _settingsService.GetSettings();
            await _gameService.StartNewGame(settings.LongText, settings.HardcoreMode);

            IsLoading = false;
        }

        [RelayCommand]
        private void HandleEncryptedLetterClick(string letter)
        {
            if (HasWon || HasLost) return;

            _gameService.SelectEncryptedLetter(letter);
        }

        [RelayCommand]
        private async Task HandleGuessLetterClick(string letter)
        {
            if (HasWon || HasLost || string.IsNullOrEmpty(SelectedEncryptedLetter)) return;

            // Check if this guess was already tried
            if (IncorrectGuesses.TryGetValue(SelectedEncryptedLetter, out var incorrectList) &&
                incorrectList.Contains(letter))
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

        private void OnGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            // Update observable properties from game state
            if (e.Game != null)
            {
                Encrypted = e.Game.Encrypted;
                Display = e.Game.Display;
                Mistakes = e.Game.Mistakes;
                MaxMistakes = e.Game.MaxMistakes;
                HasWon = e.Game.HasWon;
                HasLost = e.Game.HasLost;
                SelectedEncryptedLetter = e.Game.SelectedEncryptedLetter;
                LastCorrectGuess = e.Game.LastCorrectGuess;

                // Update collections
                UpdateSortedEncryptedLetters(e.Game);
                UpdateOriginalLetters(e.Game);
                UpdateCorrectlyGuessed(e.Game);
                UpdateLetterFrequency(e.Game);
                UpdateGuessedMappings(e.Game);
                UpdateIncorrectGuesses(e.Game);
            }
            else
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
                OriginalLetters.Clear();
                CorrectlyGuessed.Clear();
                LetterFrequency.Clear();
                GuessedMappings.Clear();
                IncorrectGuesses.Clear();
            }
        }

        private void UpdateSortedEncryptedLetters(Game game)
        {
            SortedEncryptedLetters.Clear();

            if (string.IsNullOrEmpty(game.Encrypted)) return;

            // Get unique encrypted letters
            var uniqueLetters = game.Encrypted
                .Where(char.IsLetter)
                .Select(c => c.ToString().ToUpper())
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

        private void UpdateOriginalLetters(Game game)
        {
            OriginalLetters.Clear();

            if (game.OriginalLetters == null) return;

            foreach (var letter in game.OriginalLetters)
                OriginalLetters.Add(letter);
        }

        private void UpdateCorrectlyGuessed(Game game)
        {
            CorrectlyGuessed.Clear();

            if (game.CorrectlyGuessed == null) return;

            foreach (var letter in game.CorrectlyGuessed)
                CorrectlyGuessed.Add(letter);
        }

        private void UpdateLetterFrequency(Game game)
        {
            LetterFrequency.Clear();

            if (game.LetterFrequency == null) return;

            foreach (var kvp in game.LetterFrequency)
                LetterFrequency[kvp.Key] = kvp.Value;
        }

        private void UpdateGuessedMappings(Game game)
        {
            GuessedMappings.Clear();

            if (game.GuessedMappings == null) return;

            foreach (var kvp in game.GuessedMappings)
                GuessedMappings[kvp.Key] = kvp.Value;
        }

        private void UpdateIncorrectGuesses(Game game)
        {
            IncorrectGuesses.Clear();

            if (game.IncorrectGuesses == null) return;

            foreach (var kvp in game.IncorrectGuesses)
                IncorrectGuesses[kvp.Key] = new List<string>(kvp.Value);
        }

        // Computed properties
        public bool IsGameActive => !HasWon && !HasLost;

        public bool CanGetHint => IsGameActive &&
                                 !IsHintInProgress &&
                                 Mistakes < MaxMistakes - 1;
    }
}