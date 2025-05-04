using System.Collections.ObjectModel;

namespace Decodey.Models
{
    public class Game
    {
        // Encrypted and display text
        public string Encrypted { get; set; }
        public string Display { get; set; }

        // Game state
        public int Mistakes { get; set; }
        public int MaxMistakes { get; set; }
        public bool HasWon { get; set; }
        public bool HasLost { get; set; }
        public string SelectedEncryptedLetter { get; set; }
        public string LastCorrectGuess { get; set; }

        // Game configuration
        public bool HardcoreMode { get; set; }
        public string Difficulty { get; set; }

        // Letter collections
        public List<string> OriginalLetters { get; set; } = new List<string>();
        public List<string> CorrectlyGuessed { get; set; } = new List<string>();
        public Dictionary<string, int> LetterFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, string> GuessedMappings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> IncorrectGuesses { get; set; } = new Dictionary<string, List<string>>();

        // Game ID for API integration
        public string GameId { get; set; }

        // Timing
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }

        // Create a dummy puzzle for testing
        public static Game CreateDummyPuzzle()
        {
            var game = new Game
            {
                Encrypted = "KVD HCFRJ EAPMD SPQ BCNIO PMLA KVD AKGW UPS",
                Display = "███ █████ █████ ███ █████ ████ ███ ████ ███",
                Mistakes = 0,
                MaxMistakes = 8,
                Difficulty = "easy",
                HardcoreMode = false,
                StartTime = DateTime.Now,
                OriginalLetters = new List<string> {
                    "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                    "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
                }
            };

            // Calculate letter frequency
            foreach (char c in game.Encrypted)
            {
                if (char.IsLetter(c))
                {
                    string letter = c.ToString().ToUpper();
                    if (game.LetterFrequency.ContainsKey(letter))
                        game.LetterFrequency[letter]++;
                    else
                        game.LetterFrequency[letter] = 1;
                }
            }

            return game;
        }
    }
}