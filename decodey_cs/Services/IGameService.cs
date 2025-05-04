using Decodey.Models;

namespace Decodey.Services
{
    public interface IGameService
    {
        // Game state
        Game CurrentGame { get; }
        bool HasGameStarted { get; }

        // Game actions
        Task<bool> StartNewGame(bool longText = false, bool hardcoreMode = false);
        Task<bool> SubmitGuess(string encryptedLetter, string guessedLetter);
        Task<bool> GetHint();
        void SelectEncryptedLetter(string letter);

        // Game management
        Task<bool> AbandonGame();

        // Events
        event EventHandler<GameStateChangedEventArgs> GameStateChanged;
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public Game Game { get; set; }
        public bool HasWon { get; set; }
        public bool HasLost { get; set; }
    }
}