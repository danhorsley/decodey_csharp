using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decodey.Models;

namespace Decodey.Services
{
    /// <summary>
    /// Service for managing the core game logic for Decodey
    /// </summary>
    public class GameService : IGameService
    {
        private readonly ISettingsService _settingsService;
        private readonly ISoundService _soundService;
        private Game _currentGame;
        private static readonly Random _random = new Random();

        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public GameService(ISettingsService settingsService, ISoundService soundService = null)
        {
            _settingsService = settingsService;
            _soundService = soundService;
        }

        /// <summary>
        /// Gets the current game
        /// </summary>
        public Game CurrentGame => _currentGame;

        /// <summary>
        /// Gets whether a game has been started
        /// </summary>
        public bool HasGameStarted => _currentGame != null;

        /// <summary>
        /// Starts a new game
        /// </summary>
        /// <param name="longText">Whether to use a long quote</param>
        /// <param name="hardcoreMode">Whether to hide spaces and punctuation</param>
        /// <returns>True if successful</returns>
        public async Task<bool> StartNewGame(bool longText = false, bool hardcoreMode = false)
        {
            try
            {
                // Get difficulty from settings
                var settings = _settingsService.GetSettings();
                string difficulty = settings.Difficulty;

                // Create a new game with a random quote
                // In a real implementation, this would load quotes from a database
                // Here we'll just use a few hardcoded quotes for demonstration
                var quote = GetRandomQuote(longText);

                // Create the game object
                _currentGame = new Game
                {
                    Difficulty = difficulty,
                    HardcoreMode = hardcoreMode,
                    StartTime = DateTime.Now
                };

                // Set max mistakes based on difficulty
                _currentGame.MaxMistakes = GetMaxMistakesFromDifficulty(difficulty);

                // Set up the game text
                _currentGame.OriginalParagraph = quote.Text;
                _currentGame.Author = quote.Author;
                _currentGame.MinorAttribution = quote.MinorAttribution;

                // Generate substitution mapping
                var mapping = GenerateMapping();
                _currentGame.Mapping = mapping;
                _currentGame.ReverseMapping = mapping.ToDictionary(kv => kv.Value, kv => kv.Key);

                // Encrypt the paragraph
                _currentGame.Encrypted = EncryptParagraph(quote.Text, mapping, hardcoreMode);

                // Generate display blocks
                _currentGame.Display = GenerateDisplayBlocks(quote.Text, hardcoreMode);

                // Calculate letter frequency
                _currentGame.LetterFrequency = GetLetterFrequency(_currentGame.Encrypted);

                // Initialize collections
                _currentGame.CorrectlyGuessed = new List<string>();
                _currentGame.GuessedMappings = new Dictionary<string, string>();
                _currentGame.IncorrectGuesses = new Dictionary<string, List<string>>();

                // Set game state
                _currentGame.HasWon = false;
                _currentGame.HasLost = false;
                _currentGame.Mistakes = 0;

                // Notify of state change
                OnGameStateChanged();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Submit a guess for a letter
        /// </summary>
        /// <param name="encryptedLetter">The encrypted letter being guessed</param>
        /// <param name="guessedLetter">The original letter guessed</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SubmitGuess(string encryptedLetter, string guessedLetter)
        {
            if (_currentGame == null)
                return false;

            try
            {
                // Convert to uppercase
                encryptedLetter = encryptedLetter.ToUpper();
                guessedLetter = guessedLetter.ToUpper();

                // Validate the guess
                if (!_currentGame.ReverseMapping.ContainsKey(encryptedLetter))
                    return false;

                // Check if guess is correct
                string correctLetter = _currentGame.ReverseMapping[encryptedLetter];
                bool isCorrect = guessedLetter == correctLetter;

                // Update game state based on correctness
                if (isCorrect)
                {
                    // Play correct sound if available
                    _soundService?.PlaySound(SoundType.Correct);

                    // Add to correctly guessed letters
                    if (!_currentGame.CorrectlyGuessed.Contains(encryptedLetter))
                    {
                        _currentGame.CorrectlyGuessed.Add(encryptedLetter);
                        _currentGame.GuessedMappings[encryptedLetter] = guessedLetter;
                        _currentGame.LastCorrectGuess = encryptedLetter;
                    }
                }
                else
                {
                    // Play incorrect sound if available
                    _soundService?.PlaySound(SoundType.Incorrect);

                    // Increment mistakes
                    _currentGame.Mistakes++;

                    // Track incorrect guesses
                    if (!_currentGame.IncorrectGuesses.ContainsKey(encryptedLetter))
                        _currentGame.IncorrectGuesses[encryptedLetter] = new List<string>();

                    // Only add if this guess hasn't been tried before
                    if (!_currentGame.IncorrectGuesses[encryptedLetter].Contains(guessedLetter))
                        _currentGame.IncorrectGuesses[encryptedLetter].Add(guessedLetter);
                }

                // Update display text
                UpdateDisplayText();

                // Check for win/loss conditions
                CheckGameStatus();

                // Notify of state change
                OnGameStateChanged();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting guess: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get a hint for a letter
        /// </summary>
        /// <returns>True if successful</returns>
        public async Task<bool> GetHint()
        {
            if (_currentGame == null)
                return false;

            try
            {
                // Play hint sound if available
                _soundService?.PlaySound(SoundType.Hint);

                // Get all encrypted letters that appear in the text
                var allEncrypted = new HashSet<string>(
                    _currentGame.Encrypted
                        .Where(c => char.IsLetter(c))
                        .Select(c => c.ToString())
                );

                // Filter out already guessed letters
                var unguessed = allEncrypted
                    .Where(letter => !_currentGame.CorrectlyGuessed.Contains(letter))
                    .ToList();

                if (unguessed.Count == 0)
                    return false;

                // Select a random unguessed letter
                string hintLetter = unguessed[_random.Next(unguessed.Count)];
                string hintValue = _currentGame.ReverseMapping[hintLetter];

                // Add to correctly guessed letters
                _currentGame.CorrectlyGuessed.Add(hintLetter);
                _currentGame.GuessedMappings[hintLetter] = hintValue;
                _currentGame.LastCorrectGuess = hintLetter;

                // Apply hint penalty (1 mistake)
                _currentGame.Mistakes++;

                // Update display text
                UpdateDisplayText();

                // Check for win/loss conditions
                CheckGameStatus();

                // Notify of state change
                OnGameStateChanged();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hint: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Select an encrypted letter
        /// </summary>
        /// <param name="letter">The letter to select</param>
        public void SelectEncryptedLetter(string letter)
        {
            if (_currentGame == null)
                return;

            // Convert to uppercase
            letter = letter.ToUpper();

            // Check if the letter is already guessed
            if (_currentGame.CorrectlyGuessed.Contains(letter))
            {
                _currentGame.SelectedEncryptedLetter = null;
            }
            else
            {
                // Play click sound
                _soundService?.PlaySound(SoundType.Click);

                // Set selected letter
                _currentGame.SelectedEncryptedLetter = letter;
            }

            // Notify of state change
            OnGameStateChanged();
        }

        /// <summary>
        /// Abandon the current game
        /// </summary>
        /// <returns>True if successful</returns>
        public async Task<bool> AbandonGame()
        {
            if (_currentGame == null)
                return true;  // No game to abandon

            try
            {
                // Clear the current game
                _currentGame = null;

                // Notify of state change
                OnGameStateChanged();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error abandoning game: {ex.Message}");
                return false;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Generate a substitution mapping for uppercase letters
        /// </summary>
        private static Dictionary<string, string> GenerateMapping()
        {
            // Create alphabet
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            // Create shuffled alphabet
            var shuffled = new List<char>(alphabet);
            Shuffle(shuffled);

            // Create mapping
            var mapping = new Dictionary<string, string>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                mapping[alphabet[i].ToString()] = shuffled[i].ToString();
            }

            return mapping;
        }

        /// <summary>
        /// Shuffle a list using Fisher-Yates algorithm
        /// </summary>
        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Encrypt a paragraph using a substitution mapping
        /// </summary>
        private static string EncryptParagraph(string text, Dictionary<string, string> mapping, bool hardcore)
        {
            var encrypted = new StringBuilder();
            foreach (char c in text.ToUpper())
            {
                string key = c.ToString();

                if (mapping.ContainsKey(key))
                {
                    encrypted.Append(mapping[key]);
                }
                else if (hardcore && (c == ' ' || char.IsPunctuation(c)))
                {
                    // In hardcore mode, replace spaces and punctuation with spaces
                    encrypted.Append(' ');
                }
                else
                {
                    encrypted.Append(c);
                }
            }
            return encrypted.ToString();
        }

        /// <summary>
        /// Generate display blocks for a paragraph
        /// </summary>
        private static string GenerateDisplayBlocks(string text, bool hardcore)
        {
            var display = new StringBuilder();
            foreach (char c in text.ToUpper())
            {
                if (char.IsLetter(c))
                {
                    display.Append('█');
                }
                else if (hardcore && (c == ' ' || char.IsPunctuation(c)))
                {
                    // In hardcore mode, replace spaces and punctuation with spaces
                    display.Append(' ');
                }
                else
                {
                    display.Append(c);
                }
            }
            return display.ToString();
        }

        /// <summary>
        /// Get the frequency of each letter in a text
        /// </summary>
        private static Dictionary<string, int> GetLetterFrequency(string text)
        {
            var frequency = new Dictionary<string, int>();

            // Initialize all letters with zero count
            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                frequency[c.ToString()] = 0;
            }

            // Count letters
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    string key = c.ToString();
                    frequency[key] = frequency.ContainsKey(key) ? frequency[key] + 1 : 1;
                }
            }

            return frequency;
        }

        /// <summary>
        /// Update the display text based on correctly guessed letters
        /// </summary>
        private void UpdateDisplayText()
        {
            if (_currentGame == null)
                return;

            var display = new StringBuilder();
            foreach (char c in _currentGame.Encrypted)
            {
                if (char.IsLetter(c))
                {
                    string key = c.ToString();
                    if (_currentGame.CorrectlyGuessed.Contains(key))
                    {
                        display.Append(_currentGame.ReverseMapping[key]);
                    }
                    else
                    {
                        display.Append('█');
                    }
                }
                else
                {
                    display.Append(c);
                }
            }
            _currentGame.Display = display.ToString();
        }

        /// <summary>
        /// Check if game is complete (won or lost)
        /// </summary>
        private void CheckGameStatus()
        {
            if (_currentGame == null)
                return;

            // Game is lost if mistakes exceed max allowed
            if (_currentGame.Mistakes >= _currentGame.MaxMistakes)
            {
                _currentGame.HasLost = true;
                _currentGame.CompletionTime = DateTime.Now;
                return;
            }

            // Get all unique encrypted letters in the text
            var encryptedLetters = new HashSet<string>(
                _currentGame.Encrypted
                    .Where(c => char.IsLetter(c))
                    .Select(c => c.ToString())
            );

            // Get all correctly guessed letters
            var guessedLetters = new HashSet<string>(_currentGame.CorrectlyGuessed);

            // Check if all letters are guessed
            if (encryptedLetters.Count > 0 && guessedLetters.SetEquals(encryptedLetters))
            {
                _currentGame.HasWon = true;
                _currentGame.CompletionTime = DateTime.Now;

                // Play win sound if available
                _soundService?.PlaySound(SoundType.Win);
            }
        }

        /// <summary>
        /// Get the maximum number of mistakes allowed for a difficulty level
        /// </summary>
        private static int GetMaxMistakesFromDifficulty(string difficulty)
        {
            return difficulty.ToLower() switch
            {
                "easy" => 8,
                "medium" => 5,
                "hard" => 3,
                _ => 5 // Default to medium
            };
        }

        /// <summary>
        /// Get a random quote from the predefined list
        /// </summary>
        private Quote GetRandomQuote(bool longText)
        {
            // Predefined quotes for the demo
            var quotes = longText ? LongQuotes : ShortQuotes;

            // Select a random quote
            return quotes[_random.Next(quotes.Count)];
        }

        /// <summary>
        /// Notify that the game state has changed
        /// </summary>
        private void OnGameStateChanged()
        {
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs
            {
                Game = _currentGame,
                HasWon = _currentGame?.HasWon ?? false,
                HasLost = _currentGame?.HasLost ?? false
            });
        }

        #endregion

        #region Predefined Quotes

        // Short quotes for simpler puzzles
        private static readonly List<Quote> ShortQuotes = new List<Quote>
        {
            new Quote
            {
                Text = "THE JOURNEY OF A THOUSAND MILES BEGINS WITH ONE STEP",
                Author = "Lao Tzu",
                MinorAttribution = "Ancient Chinese Philosopher"
            },
            new Quote
            {
                Text = "BE THE CHANGE THAT YOU WISH TO SEE IN THE WORLD",
                Author = "Mahatma Gandhi",
                MinorAttribution = "Indian Independence Leader"
            },
            new Quote
            {
                Text = "THE ONLY WAY TO DO GREAT WORK IS TO LOVE WHAT YOU DO",
                Author = "Steve Jobs",
                MinorAttribution = "Apple Co-founder"
            },
            new Quote
            {
                Text = "TO BE YOURSELF IN A WORLD THAT IS CONSTANTLY TRYING TO MAKE YOU SOMETHING ELSE IS THE GREATEST ACCOMPLISHMENT",
                Author = "Ralph Waldo Emerson",
                MinorAttribution = "American Essayist"
            },
            new Quote
            {
                Text = "IN THE MIDDLE OF DIFFICULTY LIES OPPORTUNITY",
                Author = "Albert Einstein",
                MinorAttribution = "Theoretical Physicist"
            }
        };

        // Long quotes for more challenging puzzles
        private static readonly List<Quote> LongQuotes = new List<Quote>
        {
            new Quote
            {
                Text = "LIFE IS WHAT HAPPENS WHEN YOU'RE BUSY MAKING OTHER PLANS. TWENTY YEARS FROM NOW YOU WILL BE MORE DISAPPOINTED BY THE THINGS YOU DIDN'T DO THAN BY THE ONES YOU DID DO.",
                Author = "John Lennon & Mark Twain",
                MinorAttribution = "Combined Quote"
            },
            new Quote
            {
                Text = "IT DOES NOT MATTER HOW SLOWLY YOU GO AS LONG AS YOU DO NOT STOP. THE DIFFERENCE BETWEEN THE IMPOSSIBLE AND THE POSSIBLE LIES IN A PERSON'S DETERMINATION.",
                Author = "Confucius & Tommy Lasorda",
                MinorAttribution = "Combined Quote"
            },
            new Quote
            {
                Text = "SUCCESS IS NOT FINAL, FAILURE IS NOT FATAL: IT IS THE COURAGE TO CONTINUE THAT COUNTS. A WINNER IS A DREAMER WHO NEVER GIVES UP.",
                Author = "Winston Churchill & Nelson Mandela",
                MinorAttribution = "Combined Quote"
            },
            new Quote
            {
                Text = "THE FUTURE BELONGS TO THOSE WHO BELIEVE IN THE BEAUTY OF THEIR DREAMS. CHALLENGES ARE WHAT MAKE LIFE INTERESTING AND OVERCOMING THEM IS WHAT MAKES LIFE MEANINGFUL.",
                Author = "Eleanor Roosevelt & Joshua J. Marine",
                MinorAttribution = "Combined Quote"
            },
            new Quote
            {
                Text = "THE BEST WAY TO PREDICT THE FUTURE IS TO CREATE IT. WHATEVER YOU DO, DO IT WELL. DO IT SO WELL THAT WHEN PEOPLE SEE YOU DO IT, THEY WILL WANT TO COME BACK AND SEE YOU DO IT AGAIN, AND THEY WILL WANT TO BRING OTHERS AND SHOW THEM HOW WELL YOU DO WHAT YOU DO.",
                Author = "Abraham Lincoln & Walt Disney",
                MinorAttribution = "Combined Quote"
            }
        };

        #endregion
    }

    /// <summary>
    /// Quote model for storing text and attribution
    /// </summary>
    public class Quote
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public string MinorAttribution { get; set; }
    }
}