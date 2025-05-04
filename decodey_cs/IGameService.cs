using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for the Game Service which handles all game logic and state management
    /// </summary>
    public interface IGameService
    {
        #region Game State Properties

        // Game state
        bool HasGameStarted { get; }
        bool HasWon { get; }
        bool HasLost { get; }
        bool IsResetting { get; }
        bool IsDailyChallenge { get; }
        bool IsHardcoreMode { get; }

        // Game content
        string Encrypted { get; }
        string Display { get; }
        List<char> OriginalLetters { get; }

        // Game progress
        int Mistakes { get; }
        int MaxMistakes { get; }
        Dictionary<char, int> LetterFrequency { get; }
        List<char> CorrectlyGuessed { get; }
        char? SelectedEncrypted { get; }
        char? LastCorrectGuess { get; }
        Dictionary<char, char> GuessedMappings { get; }
        Dictionary<char, List<char>> IncorrectGuesses { get; }

        // Win data
        WinData WinData { get; }

        // Game metrics
        DateTime? StartTime { get; }
        DateTime? CompletionTime { get; }

        #endregion

        #region Game Actions

        /// <summary>
        /// Initialize a new game with the specified parameters
        /// </summary>
        Task<GameInitResult> InitializeGame(GameSettings settings = null);

        /// <summary>
        /// Start the daily challenge
        /// </summary>
        Task<DailyChallengeResult> StartDailyChallenge();

        /// <summary>
        /// Check if the daily challenge has been completed
        /// </summary>
        Task<DailyCompletionResult> IsDailyCompleted();

        /// <summary>
        /// Continue a previously saved game
        /// </summary>
        Task<GameContinueResult> ContinueGame();

        /// <summary>
        /// Check if a game can be continued
        /// </summary>
        Task<bool> CheckForContinuableGame();

        /// <summary>
        /// Save the current game state
        /// </summary>
        Task SaveGameState();

        /// <summary>
        /// Reset and start a new game
        /// </summary>
        Task<GameInitResult> ResetAndStartNewGame(bool longText = false, bool hardcoreMode = false,
            Dictionary<string, object> options = null);

        /// <summary>
        /// Handle the selection of an encrypted letter
        /// </summary>
        void HandleEncryptedSelect(char letter);

        /// <summary>
        /// Submit a guess for the selected encrypted letter
        /// </summary>
        Task<GuessResult> SubmitGuess(char encryptedLetter, char guessedLetter);

        /// <summary>
        /// Get a hint for the current game
        /// </summary>
        Task<HintResult> GetHint();

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the game is initialized
        /// </summary>
        event EventHandler GameInitialized;

        /// <summary>
        /// Event raised when the game state changes
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        /// Event raised when a guess is submitted
        /// </summary>
        event EventHandler<GuessResult> GuessSubmitted;

        /// <summary>
        /// Event raised when a hint is used
        /// </summary>
        event EventHandler<HintResult> HintUsed;

        /// <summary>
        /// Event raised when the game is won
        /// </summary>
        event EventHandler<WinData> GameWon;

        /// <summary>
        /// Event raised when the game is lost
        /// </summary>
        event EventHandler GameLost;

        #endregion
    }

    #region Data Transfer Objects

    /// <summary>
    /// Result of game initialization
    /// </summary>
    public class GameInitResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool IsDailyChallenge { get; set; }
        public bool IsActiveGameContinued { get; set; }
    }

    /// <summary>
    /// Result of starting a daily challenge
    /// </summary>
    public class DailyChallengeResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool AlreadyCompleted { get; set; }
        public DailyCompletionData CompletionData { get; set; }
    }

    /// <summary>
    /// Result of checking daily challenge completion
    /// </summary>
    public class DailyCompletionResult
    {
        public bool IsCompleted { get; set; }
        public DailyCompletionData CompletionData { get; set; }
    }

    /// <summary>
    /// Daily challenge completion data
    /// </summary>
    public class DailyCompletionData
    {
        public int Score { get; set; }
        public int TimeTaken { get; set; }
        public int Mistakes { get; set; }
        public int MaxMistakes { get; set; }
        public string Rating { get; set; }
        public DateTime CompletionDate { get; set; }
    }

    /// <summary>
    /// Result of continuing a saved game
    /// </summary>
    public class GameContinueResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool HasActiveGame { get; set; }
    }

    /// <summary>
    /// Result of submitting a guess
    /// </summary>
    public class GuessResult
    {
        public bool IsCorrect { get; set; }
        public bool IsIncorrect { get; set; }
        public char EncryptedLetter { get; set; }
        public char GuessedLetter { get; set; }
        public bool HasWon { get; set; }
        public bool HasLost { get; set; }
    }

    /// <summary>
    /// Result of using a hint
    /// </summary>
    public class HintResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public char? RevealedEncrypted { get; set; }
        public char? RevealedOriginal { get; set; }
        public int RemainingHints { get; set; }
    }

    /// <summary>
    /// Win data for tracking game completion
    /// </summary>
    public class WinData
    {
        public string Encrypted { get; set; }
        public string Display { get; set; }
        public List<char> CorrectlyGuessed { get; set; }
        public Dictionary<char, char> GuessedMappings { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletionTime { get; set; }
        public int Mistakes { get; set; }
        public int MaxMistakes { get; set; }
        public int GameTimeSeconds { get; set; }
        public bool HardcoreMode { get; set; }
        public bool IsDailyChallenge { get; set; }
        public Attribution Attribution { get; set; }
        public int? Score { get; set; }
        public int? CurrentStreak { get; set; }
        public int? BestStreak { get; set; }
    }

    /// <summary>
    /// Attribution for the quote used in the game
    /// </summary>
    public class Attribution
    {
        public string MajorAttribution { get; set; }
        public string MinorAttribution { get; set; }
    }

    /// <summary>
    /// Game settings for initialization
    /// </summary>
    public class GameSettings
    {
        public bool LongText { get; set; } = false;
        public bool HardcoreMode { get; set; } = false;
        public bool CustomGameRequested { get; set; } = false;
        public bool SkipDailyCheck { get; set; } = false;
        public bool BackdoorMode { get; set; } = false;
    }

    #endregion
}