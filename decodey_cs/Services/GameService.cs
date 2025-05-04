using Decodey.Models;

namespace Decodey.Services
{
    public class GameService : IGameService
    {
        private Game _currentGame;
        private readonly ISettingsService _settingsService;

        public GameService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public Game CurrentGame => _currentGame;

        public bool HasGameStarted => _currentGame != null;

        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public Task<bool> StartNewGame(bool longText = false, bool hardcoreMode = false)
        {
            // For now, just create a dummy puzzle
            _currentGame = Game.CreateDummyPuzzle();

            // Override with provided parameters
            _currentGame.HardcoreMode = hardcoreMode;

            // Get max mistakes from settings based on difficulty
            var settings = _settingsService.GetSettings();
            _currentGame.Difficulty = settings.Difficulty;
            _currentGame.MaxMistakes = GetMaxMistakes(settings.Difficulty);

            // Notify of state change
            OnGameStateChanged();

            return Task.FromResult(true);
        }

        public Task<bool> SubmitGuess(string encryptedLetter, string guessedLetter)
        {
            if (_currentGame == null || string.IsNullOrEmpty(encryptedLetter) || string.IsNullOrEmpty(guessedLetter))
                return Task.FromResult(false);

            // Get the solution for this dummy puzzle - "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG"
            var solution = new Dictionary<string, string>
            {
                { "K", "T" }, { "V", "H" }, { "D", "E" }, { "H", "Q" }, { "C", "U" },
                { "F", "I" }, { "R", "C" }, { "J", "K" }, { "E", "B" }, { "A", "R" },
                { "P", "O" }, { "M", "W" }, { "D", "N" }, { "S", "F" }, { "Q", "X" },
                { "B", "J" }, { "C", "U" }, { "N", "M" }, { "I", "P" }, { "O", "S" },
                { "M", "W" }, { "L", "V" }, { "A", "E" }, { "K", "T" }, { "G", "L" },
                { "W", "A" }, { "U", "Z" }, { "P", "Y" }, { "S", "D" }, { "Q", "G" }
            };

            // Check if the guess is correct
            bool isCorrect = solution.TryGetValue(encryptedLetter, out string correctLetter) &&
                             correctLetter == guessedLetter;

            if (isCorrect)
            {
                // Add to correctly guessed letters
                if (!_currentGame.CorrectlyGuessed.Contains(encryptedLetter))
                {
                    _currentGame.CorrectlyGuessed.Add(encryptedLetter);
                    _currentGame.LastCorrectGuess = encryptedLetter;
                }

                // Add to guessed mappings
                _currentGame.GuessedMappings[encryptedLetter] = guessedLetter;

                // Update display - replace █ with the correct letter
                string updatedDisplay = "";
                for (int i = 0; i < _currentGame.Encrypted.Length; i++)
                {
                    string encChar = _currentGame.Encrypted.Substring(i, 1).ToUpper();

                    if (_currentGame.GuessedMappings.ContainsKey(encChar))
                        updatedDisplay += _currentGame.GuessedMappings[encChar];
                    else if (encChar == " ")
                        updatedDisplay += " ";
                    else
                        updatedDisplay += "█";
                }

                _currentGame.Display = updatedDisplay;

                // Check for win condition
                var uniqueEncryptedLetters = _currentGame.Encrypted
                    .Where(char.IsLetter)
                    .Select(c => c.ToString().ToUpper())
                    .Distinct()
                    .ToList();

                if (uniqueEncryptedLetters.All(l => _currentGame.CorrectlyGuessed.Contains(l)))
                {
                    _currentGame.HasWon = true;
                    _currentGame.CompletionTime = DateTime.Now;
                }
            }
            else
            {
                // Add to incorrect guesses
                if (!_currentGame.IncorrectGuesses.ContainsKey(encryptedLetter))
                    _currentGame.IncorrectGuesses[encryptedLetter] = new List<string>();

                if (!_currentGame.IncorrectGuesses[encryptedLetter].Contains(guessedLetter))
                    _currentGame.IncorrectGuesses[encryptedLetter].Add(guessedLetter);

                // Increment mistakes
                _currentGame.Mistakes++;

                // Check for lose condition
                if (_currentGame.Mistakes >= _currentGame.MaxMistakes)
                {
                    _currentGame.HasLost = true;
                    _currentGame.CompletionTime = DateTime.Now;
                }
            }

            // Clear selected letter
            _currentGame.SelectedEncryptedLetter = null;

            // Notify of state change
            OnGameStateChanged();

            return Task.FromResult(true);
        }

        public Task<bool> GetHint()
        {
            if (_currentGame == null)
                return Task.FromResult(false);

            // Get the solution for this dummy puzzle
            var solution = new Dictionary<string, string>
            {
                { "K", "T" }, { "V", "H" }, { "D", "E" }, { "H", "Q" }, { "C", "U" },
                { "F", "I" }, { "R", "C" }, { "J", "K" }, { "E", "B" }, { "A", "R" },
                { "P", "O" }, { "M", "W" }, { "D", "N" }, { "S", "F" }, { "Q", "X" },
                { "B", "J" }, { "C", "U" }, { "N", "M" }, { "I", "P" }, { "O", "S" },
                { "M", "W" }, { "L", "V" }, { "A", "E" }, { "K", "T" }, { "G", "L" },
                { "W", "A" }, { "U", "Z" }, { "P", "Y" }, { "S", "D" }, { "Q", "G" }
            };

            // Find a letter that hasn't been guessed yet
            var unguessedLetters = solution.Keys
                .Where(k => !_currentGame.CorrectlyGuessed.Contains(k))
                .ToList();

            if (unguessedLetters.Count == 0)
                return Task.FromResult(false);

            // Pick a random letter to reveal
            var random = new Random();
            string letterToReveal = unguessedLetters[random.Next(unguessedLetters.Count)];
            string correctValue = solution[letterToReveal];

            // Add to correctly guessed
            _currentGame.CorrectlyGuessed.Add(letterToReveal);
            _currentGame.GuessedMappings[letterToReveal] = correctValue;

            // Update display
            string updatedDisplay = "";
            for (int i = 0; i < _currentGame.Encrypted.Length; i++)
            {
                string encChar = _currentGame.Encrypted.Substring(i, 1).ToUpper();

                if (_currentGame.GuessedMappings.ContainsKey(encChar))
                    updatedDisplay += _currentGame.GuessedMappings[encChar];
                else if (encChar == " ")
                    updatedDisplay += " ";
                else
                    updatedDisplay += "█";
            }

            _currentGame.Display = updatedDisplay;

            // Increment mistakes for using a hint
            _currentGame.Mistakes++;

            // Check for lose condition
            if (_currentGame.Mistakes >= _currentGame.MaxMistakes)
            {
                _currentGame.HasLost = true;
                _currentGame.CompletionTime = DateTime.Now;
            }

            // Check for win condition
            var uniqueEncryptedLetters = _currentGame.Encrypted
                .Where(char.IsLetter)
                .Select(c => c.ToString().ToUpper())
                .Distinct()
                .ToList();

            if (uniqueEncryptedLetters.All(l => _currentGame.CorrectlyGuessed.Contains(l)))
            {
                _currentGame.HasWon = true;
                _currentGame.CompletionTime = DateTime.Now;
            }

            // Notify of state change
            OnGameStateChanged();

            return Task.FromResult(true);
        }

        public void SelectEncryptedLetter(string letter)
        {
            if (_currentGame == null)
                return;

            // Only allow selection if the letter isn't already correctly guessed
            if (!_currentGame.CorrectlyGuessed.Contains(letter))
            {
                _currentGame.SelectedEncryptedLetter = letter;
                OnGameStateChanged();
            }
        }

        public Task<bool> AbandonGame()
        {
            _currentGame = null;
            OnGameStateChanged();
            return Task.FromResult(true);
        }

        private int GetMaxMistakes(string difficulty)
        {
            return difficulty switch
            {
                "easy" => 8,
                "medium" => 5,
                "hard" => 3,
                _ => 5
            };
        }

        private void OnGameStateChanged()
        {
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs
            {
                Game = _currentGame,
                HasWon = _currentGame?.HasWon ?? false,
                HasLost = _currentGame?.HasLost ?? false
            });
        }
    }
}