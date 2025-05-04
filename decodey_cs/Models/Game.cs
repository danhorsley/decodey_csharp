using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Decodey.Models
{
    /// <summary>
    /// Main game model that holds the game state
    /// </summary>
    public class Game
    {
        // Original content
        public string OriginalParagraph { get; set; }
        public string Author { get; set; }
        public string MinorAttribution { get; set; }

        // Encrypted content
        public string Encrypted { get; set; }
        public string Display { get; set; }
        public Dictionary<string, string> Mapping { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ReverseMapping { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> LetterFrequency { get; set; } = new Dictionary<string, int>();

        // Game state
        public List<string> CorrectlyGuessed { get; set; } = new List<string>();
        public Dictionary<string, string> GuessedMappings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> IncorrectGuesses { get; set; } = new Dictionary<string, List<string>>();
        public int Mistakes { get; set; }
        public int MaxMistakes { get; set; }
        public bool HasWon { get; set; }
        public bool HasLost { get; set; }
        public string Difficulty { get; set; } = "medium";
        public bool HardcoreMode { get; set; }

        // UI state
        public string SelectedEncryptedLetter { get; set; }
        public string LastCorrectGuess { get; set; }

        // Timing
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }

        // Game ID for API integration (not used in local version)
        public string GameId { get; set; }

        /// <summary>
        /// Get the time taken in seconds
        /// </summary>
        public int GetTimeTaken()
        {
            if (CompletionTime.HasValue)
            {
                return (int)(CompletionTime.Value - StartTime).TotalSeconds;
            }

            return (int)(DateTime.Now - StartTime).TotalSeconds;
        }

        /// <summary>
        /// Get the score for this game
        /// </summary>
        public int CalculateScore()
        {
            if (!HasWon)
                return 0;

            // Basic score components
            double baseScore = 100;
            double difficultyMultiplier = Difficulty.ToLower() switch
            {
                "easy" => 1.0,
                "medium" => 1.5,
                "hard" => 2.0,
                _ => 1.0
            };

            // Adjust for number of unique letters
            int uniqueLetters = 0;
            foreach (char c in Encrypted)
            {
                if (char.IsLetter(c) && LetterFrequency.ContainsKey(c.ToString()) && LetterFrequency[c.ToString()] > 0)
                {
                    uniqueLetters++;
                }
            }

            double letterMultiplier = Math.Min(1.0 + (uniqueLetters / 26.0), 2.0);

            // Time factor (faster is better)
            int totalSeconds = GetTimeTaken();
            double timeMultiplier = Math.Max(0.5, Math.Min(1.5, 600.0 / Math.Max(totalSeconds, 60)));

            // Mistakes factor (fewer is better)
            double mistakesPenalty = Math.Max(0.5, 1.0 - (double)Mistakes / MaxMistakes);

            // Hardcore mode bonus
            double hardcoreBonus = HardcoreMode ? 1.5 : 1.0;

            // Calculate final score
            double score = baseScore * difficultyMultiplier * letterMultiplier * timeMultiplier * mistakesPenalty * hardcoreBonus;

            return (int)Math.Round(score);
        }

        /// <summary>
        /// Get a formatted time string
        /// </summary>
        public string GetTimeString()
        {
            int seconds = GetTimeTaken();
            int minutes = seconds / 60;
            seconds %= 60;

            return $"{minutes}:{seconds:D2}";
        }

        /// <summary>
        /// Get string rows for the encrypted letters arranged in a grid
        /// </summary>
        public List<List<string>> GetEncryptedLetterRows()
        {
            List<string> uniqueLetters = new List<string>();

            // Get unique letters in order of appearance
            foreach (char c in Encrypted)
            {
                if (char.IsLetter(c) && !uniqueLetters.Contains(c.ToString()))
                {
                    uniqueLetters.Add(c.ToString());
                }
            }

            // Arrange in rows of 7
            const int columnsPerRow = 7;
            List<List<string>> rows = new List<List<string>>();

            for (int i = 0; i < uniqueLetters.Count; i += columnsPerRow)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < columnsPerRow && i + j < uniqueLetters.Count; j++)
                {
                    row.Add(uniqueLetters[i + j]);
                }
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Get string rows for the original letter grid (A-Z)
        /// </summary>
        public List<List<string>> GetGuessLetterRows()
        {
            // Create A-Z grid
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
        /// Get the display text as a list of TextLine objects
        /// </summary>
        public List<TextLine> GetTextLines()
        {
            List<TextLine> lines = new List<TextLine>();

            // Split the text by newlines or long spaces
            string[] paragraphs = GetParagraphs();

            foreach (string paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;

                TextLine line = new TextLine();

                // Add encrypted characters
                for (int i = 0; i < paragraph.Length; i++)
                {
                    line.EncryptedChars.Add(Encrypted[i].ToString());
                }

                // Add display characters
                for (int i = 0; i < paragraph.Length; i++)
                {
                    char c = paragraph[i];

                    if (char.IsLetter(c))
                    {
                        // Check if we've guessed this letter
                        string encryptedChar = Encrypted[i].ToString();
                        bool isGuessed = CorrectlyGuessed.Contains(encryptedChar);

                        DisplayChar displayChar = new DisplayChar
                        {
                            Text = isGuessed ? ReverseMapping[encryptedChar] : "█",
                            IsPlaceholder = !isGuessed
                        };

                        line.DisplayChars.Add(displayChar);
                    }
                    else
                    {
                        // Non-letter character (space, punctuation, etc.)
                        DisplayChar displayChar = new DisplayChar
                        {
                            Text = HardcoreMode ? " " : c.ToString(),
                            IsPlaceholder = false
                        };

                        line.DisplayChars.Add(displayChar);
                    }
                }

                lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Split the original paragraph into multiple paragraphs
        /// </summary>
        private string[] GetParagraphs()
        {
            // For simplicity, we'll just treat the whole text as one paragraph
            // In a more sophisticated implementation, we could split by newlines
            return new[] { OriginalParagraph };
        }

        /// <summary>
        /// Create a dummy puzzle for testing
        /// </summary>
        public static Game CreateDummyPuzzle()
        {
            string originalText = "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG";

            // Create the game
            Game game = new Game
            {
                OriginalParagraph = originalText,
                Author = "Typing Practice",
                MinorAttribution = "English Pangram",
                Encrypted = "KVD HCFRJ EAPMD SPQ BCNIO PMLA KVD AKGW UPS",
                Display = "███ █████ █████ ███ █████ ████ ███ ████ ███",
                Difficulty = "medium",
                MaxMistakes = 5,
                StartTime = DateTime.Now
            };

            // Set up mapping (normally this would be randomly generated)
            var mapping = new Dictionary<string, string>
            {
                { "T", "K" }, { "H", "V" }, { "E", "D" }, { "Q", "H" }, { "U", "C" },
                { "I", "F" }, { "C", "R" }, { "K", "J" }, { "B", "E" }, { "R", "A" },
                { "O", "P" }, { "W", "M" }, { "N", "D" }, { "F", "S" }, { "X", "Q" },
                { "J", "B" }, { "U", "C" }, { "M", "N" }, { "P", "I" }, { "S", "O" },
                { "V", "L" }, { "E", "A" }, { "T", "K" }, { "L", "G" }, { "A", "W" },
                { "Z", "U" }, { "Y", "Y" }, { "D", "S" }, { "G", "Q" }
            };

            game.Mapping = mapping;

            // Create reverse mapping
            game.ReverseMapping = new Dictionary<string, string>();
            foreach (var kv in mapping)
            {
                game.ReverseMapping[kv.Value] = kv.Key;
            }

            // Set up letter frequency
            foreach (char c in game.Encrypted)
            {
                if (char.IsLetter(c))
                {
                    string key = c.ToString();

                    if (game.LetterFrequency.ContainsKey(key))
                    {
                        game.LetterFrequency[key]++;
                    }
                    else
                    {
                        game.LetterFrequency[key] = 1;
                    }
                }
            }

            return game;
        }
    }
}