using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Decodey.Models;
using Decodey.Services;

namespace Decodey.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        #region Fields

        // Services
        private readonly IGameService _gameService;
        private readonly ISettingsService _settingsService;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly ISoundService _soundService;
        private readonly INavigationService _navigationService;
        private readonly IShareService _shareService;

        // Game state
        private bool _isLoading = true;
        private bool _showWinCelebration = false;
        private bool _isMenuOpen = false;
        private bool _isHintInProgress = false;
        private int _pendingHints = 0;
        private DeviceOrientation _deviceOrientation = DeviceOrientation.Portrait;
        private double _screenWidth;
        private double _screenHeight;
        private double _charFontSize = 18.0;

        // Tutorial state
        private bool _isTutorialActive = false;
        private int _currentTutorialStep = 0;

        #endregion

        #region Properties

        // User Info
        public string UserDisplayName => _authService.IsAuthenticated ?
            _authService.UserName : "Guest";

        public string AuthActionLabel => _authService.IsAuthenticated ?
            "Logout" : "Login / Create Account";

        public string AuthIcon => _authService.IsAuthenticated ?
            "🔓" : "🔒";

        public string AppVersion => $"Version {AppInfo.VersionString}";

        // Game State Properties
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public bool HasGameEnded => _gameService.HasWon || _gameService.HasLost;

        public bool IsGameActive => !_gameService.HasWon &&
                                   !_gameService.HasLost &&
                                   !_gameService.IsResetting;

        public bool ShowWinCelebration
        {
            get => _showWinCelebration;
            private set => SetProperty(ref _showWinCelebration, value);
        }

        public bool IsLoadingOrCelebrating => IsLoading || ShowWinCelebration;

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            private set => SetProperty(ref _isMenuOpen, value);
        }

        public bool IsDailyChallenge => _gameService.IsDailyChallenge;

        public bool IsHardcoreMode => _gameService.IsHardcoreMode;

        public bool IsHintInProgress
        {
            get => _isHintInProgress;
            private set => SetProperty(ref _isHintInProgress, value);
        }

        public int PendingHints
        {
            get => _pendingHints;
            private set => SetProperty(ref _pendingHints, value);
        }

        // Grid Layout Properties
        public List<List<char>> EncryptedLetterRows { get; private set; }

        public List<List<char>> GuessLetterRows { get; private set; }

        // Text Display Properties
        public ObservableCollection<TextLine> TextLines { get; private set; } =
            new ObservableCollection<TextLine>();

        public double CharFontSize
        {
            get => _charFontSize;
            private set => SetProperty(ref _charFontSize, value);
        }

        // Hint Button Properties
        public string HintText => _gameService.Mistakes.ToString();

        public Style HintButtonStyle
        {
            get
            {
                // Determine style based on mistakes count
                int mistakesLeft = _gameService.MaxMistakes - _gameService.Mistakes;

                if (mistakesLeft >= 5)
                {
                    // Plenty of mistakes left - green
                    return Application.Current.Resources["HintButtonSuccessFrameStyle"] as Style;
                }
                else if (mistakesLeft >= 2)
                {
                    // Getting low - yellow
                    return Application.Current.Resources["HintButtonWarningFrameStyle"] as Style;
                }
                else
                {
                    // Danger zone - red
                    return Application.Current.Resources["HintButtonDangerFrameStyle"] as Style;
                }
            }
        }

        // Win/Loss Properties
        public WinData WinData => _gameService.WinData;

        public string WinStatusText => _gameService.HasWon ?
            "MISSION ACCOMPLISHED!" : "MISSION FAILED";

        public bool HasAttribution => WinData?.Attribution != null &&
                                    !string.IsNullOrEmpty(WinData.Attribution.MajorAttribution);

        public string AttributionText
        {
            get
            {
                if (WinData?.Attribution == null) return string.Empty;

                var major = WinData.Attribution.MajorAttribution;
                var minor = WinData.Attribution.MinorAttribution;

                if (string.IsNullOrEmpty(major)) return string.Empty;
                if (string.IsNullOrEmpty(minor)) return major;

                return $"{major}, {minor}";
            }
        }

        public bool IsStatsCalculated => WinData?.Score != null;

        public string ScoreText => WinData?.Score != null ?
            $"SCORE: {WinData.Score:N0}" : "Calculating...";

        public string TimeText
        {
            get
            {
                if (WinData == null) return "N/A";

                int seconds = WinData.GameTimeSeconds;
                int minutes = seconds / 60;
                int remainingSeconds = seconds % 60;

                return $"{minutes}m {remainingSeconds}s";
            }
        }

        public string MistakesText => WinData != null ?
            $"{WinData.Mistakes} / {WinData.MaxMistakes}" : "N/A";

        public string DifficultyText => WinData != null ?
            (WinData.HardcoreMode ? "HARDCORE" : "NORMAL") : "N/A";

        public string RatingText => WinData?.Score != null ?
            DetermineRating(WinData.Score.Value) : "...";

        public string CurrentStreakText => WinData?.CurrentStreak != null ?
            WinData.CurrentStreak.ToString() : "0";

        public string BestStreakText => WinData?.BestStreak != null ?
            WinData.BestStreak.ToString() : "0";

        public bool ShouldShowLoginPrompt => !_authService.IsAuthenticated &&
                                           _gameService.HasWon;

        public string GameOverMessage => _gameService.HasWon ?
            "Congratulations! You solved the puzzle!" :
            "Game Over! Better luck next time!";

        public Color MatrixColor => Application.Current.UserAppTheme == AppTheme.Dark ?
            Color.FromArgb("#4cc9f0") : Color.FromArgb("#00ff41");

        public string MatrixMessage => IsLoading ?
            "Initializing Cipher..." :
            (_gameService.HasWon ? "Decryption Complete!" : "Decryption Failed!");

        // Tutorial Properties
        public bool IsTutorialActive
        {
            get => _isTutorialActive;
            private set => SetProperty(ref _isTutorialActive, value);
        }

        public int CurrentTutorialStep
        {
            get => _currentTutorialStep;
            set => SetProperty(ref _currentTutorialStep, value);
        }

        #endregion

        #region Commands

        // Game Actions
        public ICommand EncryptedSelectCommand { get; }
        public ICommand SubmitGuessCommand { get; }
        public ICommand GetHintCommand { get; }
        public ICommand StartNewGameCommand { get; }

        // Navigation
        public ICommand ToggleMenuCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand OpenLeaderboardCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand StartDailyChallengeCommand { get; }

        // Tutorial
        public ICommand StartTutorialCommand { get; }
        public ICommand NextTutorialStepCommand { get; }
        public ICommand EndTutorialCommand { get; }

        // Auth
        public ICommand AuthActionCommand { get; }
        public ICommand ShowLoginCommand { get; }

        // Sharing
        public ICommand ShareResultsCommand { get; }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<CorrectGuessEventArgs> CorrectGuessAnimationRequested;
        public event EventHandler<IncorrectGuessEventArgs> IncorrectGuessAnimationRequested;

        #endregion

        #region Constructor

        public GameViewModel()
        {
            // Get services
            _gameService = App.GetService<IGameService>() ??
                           throw new InvalidOperationException("GameService not found");
            _settingsService = App.GetService<ISettingsService>();
            _authService = App.GetService<IAuthService>();
            _dialogService = App.GetService<IDialogService>();
            _soundService = App.GetService<ISoundService>();
            _navigationService = App.GetService<INavigationService>();
            _shareService = App.GetService<IShareService>();

            // Initialize commands
            EncryptedSelectCommand = new Command<char>(HandleEncryptedSelect);
            SubmitGuessCommand = new Command<char>(async (c) => await HandleGuessSubmit(c));
            GetHintCommand = new Command(async () => await HandleGetHint());
            StartNewGameCommand = new Command(async () => await HandleStartNewGame());

            ToggleMenuCommand = new Command(HandleToggleMenu);
            NavigateToHomeCommand = new Command(HandleNavigateToHome);
            OpenLeaderboardCommand = new Command(HandleOpenLeaderboard);
            OpenSettingsCommand = new Command(HandleOpenSettings);
            OpenAboutCommand = new Command(HandleOpenAbout);
            StartDailyChallengeCommand = new Command(async () => await HandleStartDailyChallenge());

            StartTutorialCommand = new Command(HandleStartTutorial);
            NextTutorialStepCommand = new Command(HandleNextTutorialStep);
            EndTutorialCommand = new Command(HandleEndTutorial);

            AuthActionCommand = new Command(async () => await HandleAuthAction());
            ShowLoginCommand = new Command(HandleShowLogin);

            ShareResultsCommand = new Command(async () => await HandleShareResults());

            // Subscribe to game service events
            SubscribeToGameServiceEvents();

            // Initialize empty grids
            EncryptedLetterRows = new List<List<char>>();
            GuessLetterRows = new List<List<char>>();

            // Initialize the game
            Task.Run(InitializeGameAsync);
        }

        #endregion

        #region Initialization

        private void SubscribeToGameServiceEvents()
        {
            _gameService.GameInitialized += OnGameInitialized;
            _gameService.StateChanged += OnGameStateChanged;
            _gameService.GuessSubmitted += OnGuessSubmitted;
            _gameService.HintUsed += OnHintUsed;
            _gameService.GameWon += OnGameWon;
            _gameService.GameLost += OnGameLost;
        }

        private async Task InitializeGameAsync()
        {
            try
            {
                IsLoading = true;

                // Show the tutorial for first-time users
                bool tutorialCompleted = await _settingsService.GetSettingAsync<bool>("tutorial-completed");
                if (!tutorialCompleted)
                {
                    // Start tutorial after a short delay
                    await Task.Delay(1000);
                    IsTutorialActive = true;
                }

                // Initialize the game
                var result = await _gameService.InitializeGame();

                if (!result.Success)
                {
                    await _dialogService.ShowAlertAsync(
                        "Error",
                        $"Failed to initialize game: {result.Error}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(
                    "Error",
                    $"An error occurred: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Game Event Handlers

        private void OnGameInitialized(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Refresh UI with game data
                UpdateGridLayouts();
                UpdateTextDisplay();

                // Notify UI of property changes
                OnPropertyChanged(nameof(IsGameActive));
                OnPropertyChanged(nameof(IsDailyChallenge));
                OnPropertyChanged(nameof(IsHardcoreMode));
                OnPropertyChanged(nameof(HintText));
                OnPropertyChanged(nameof(HintButtonStyle));

                // Stop loading
                IsLoading = false;
            });
        }

        private void OnGameStateChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Refresh UI with game data
                UpdateGridLayouts();
                UpdateTextDisplay();

                // Notify UI of property changes
                OnPropertyChanged(nameof(IsGameActive));
                OnPropertyChanged(nameof(HasGameEnded));
                OnPropertyChanged(nameof(HintText));
                OnPropertyChanged(nameof(HintButtonStyle));
            });
        }

        private void OnGuessSubmitted(object sender, GuessResult e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Play sound
                if (e.IsCorrect)
                {
                    _soundService?.PlaySound(SoundType.Correct);
                    CorrectGuessAnimationRequested?.Invoke(this,
                        new CorrectGuessEventArgs(e.EncryptedLetter, e.GuessedLetter));
                }
                else if (e.IsIncorrect)
                {
                    _soundService?.PlaySound(SoundType.Incorrect);
                    IncorrectGuessAnimationRequested?.Invoke(this,
                        new IncorrectGuessEventArgs(e.GuessedLetter));
                }

                // Update UI
                UpdateGridLayouts();
                UpdateTextDisplay();

                // Notify UI of property changes
                OnPropertyChanged(nameof(HintText));
                OnPropertyChanged(nameof(HintButtonStyle));
            });
        }

        private void OnHintUsed(object sender, HintResult e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (e.Success)
                {
                    _soundService?.PlaySound(SoundType.Hint);

                    // Update UI
                    UpdateGridLayouts();
                    UpdateTextDisplay();

                    // Notify UI of property changes
                    OnPropertyChanged(nameof(HintText));
                    OnPropertyChanged(nameof(HintButtonStyle));
                }

                // Reset hint progress
                IsHintInProgress = false;
                PendingHints = 0;
            });
        }

        private void OnGameWon(object sender, WinData e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Play win sound
                _soundService?.PlaySound(SoundType.Win);

                // Show celebration after a short delay
                Task.Delay(800).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ShowWinCelebration = true;

                        // Notify UI of property changes
                        OnPropertyChanged(nameof(IsGameActive));
                        OnPropertyChanged(nameof(HasGameEnded));
                        OnPropertyChanged(nameof(WinData));
                        OnPropertyChanged(nameof(WinStatusText));
                        OnPropertyChanged(nameof(AttributionText));
                        OnPropertyChanged(nameof(HasAttribution));
                        OnPropertyChanged(nameof(IsStatsCalculated));
                        OnPropertyChanged(nameof(ScoreText));
                        OnPropertyChanged(nameof(TimeText));
                        OnPropertyChanged(nameof(MistakesText));
                        OnPropertyChanged(nameof(DifficultyText));
                        OnPropertyChanged(nameof(RatingText));
                        OnPropertyChanged(nameof(CurrentStreakText));
                        OnPropertyChanged(nameof(BestStreakText));
                        OnPropertyChanged(nameof(ShouldShowLoginPrompt));
                        OnPropertyChanged(nameof(GameOverMessage));
                    });
                });
            });
        }

        private void OnGameLost(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Play lose sound
                _soundService?.PlaySound(SoundType.Lose);

                // Notify UI of property changes
                OnPropertyChanged(nameof(IsGameActive));
                OnPropertyChanged(nameof(HasGameEnded));
                OnPropertyChanged(nameof(WinStatusText));
                OnPropertyChanged(nameof(GameOverMessage));
            });
        }

        #endregion

        #region Command Handlers

        private void HandleEncryptedSelect(char letter)
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Forward to game service
            _gameService.HandleEncryptedSelect(letter);

            // Update UI
            UpdateGridLayouts();
        }

        private async Task HandleGuessSubmit(char guessedLetter)
        {
            if (_gameService.SelectedEncrypted == null) return;

            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Submit guess
            await _gameService.SubmitGuess(_gameService.SelectedEncrypted.Value, guessedLetter);

            // UI is updated via event handlers
        }

        private async Task HandleGetHint()
        {
            if (IsHintInProgress) return;

            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Show hint in progress
            IsHintInProgress = true;
            PendingHints++;

            // Get hint
            await _gameService.GetHint();

            // UI is updated via event handlers
        }

        private async Task HandleStartNewGame()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Hide celebration if showing
            ShowWinCelebration = false;

            // Start loading
            IsLoading = true;

            // Start new game
            var settings = new GameSettings
            {
                LongText = _settingsService.GetSetting<bool>("long-text"),
                HardcoreMode = _settingsService.GetSetting<bool>("hardcore-mode"),
                CustomGameRequested = true
            };

            await _gameService.ResetAndStartNewGame(settings.LongText, settings.HardcoreMode);

            // UI is updated via event handlers
        }

        private void HandleToggleMenu()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Toggle menu
            IsMenuOpen = !IsMenuOpen;
        }

        private void HandleNavigateToHome()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Navigate to home
            _navigationService.NavigateToPage("HomePage");
        }

        private void HandleOpenLeaderboard()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Navigate to leaderboard
            _navigationService.NavigateToPage("LeaderboardPage");
        }

        private void HandleOpenSettings()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Show settings dialog
            _dialogService.ShowSettingsDialog();
        }

        private void HandleOpenAbout()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Show about dialog
            _dialogService.ShowAboutDialog();
        }

        private async Task HandleStartDailyChallenge()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Hide celebration if showing
            ShowWinCelebration = false;

            // Start loading
            IsLoading = true;

            // Start daily challenge
            var result = await _gameService.StartDailyChallenge();

            if (!result.Success)
            {
                if (result.AlreadyCompleted)
                {
                    // Show daily challenge completion dialog
                    await _dialogService.ShowDailyCompletedDialog(result.CompletionData);
                }
                else
                {
                    // Show error
                    await _dialogService.ShowAlertAsync(
                        "Error",
                        $"Failed to start daily challenge: {result.Error}",
                        "OK");
                }

                // Stop loading
                IsLoading = false;
            }

            // UI is updated via event handlers
        }

        private void HandleStartTutorial()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            // Start tutorial
            IsTutorialActive = true;
            CurrentTutorialStep = 0;
        }

        private void HandleNextTutorialStep()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Advance to next step
            CurrentTutorialStep++;

            // End tutorial if we've completed all steps
            if (CurrentTutorialStep >= 6)
            {
                HandleEndTutorial();
            }
        }

        private void HandleEndTutorial()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // End tutorial
            IsTutorialActive = false;

            // Save tutorial completed
            _settingsService.SetSettingAsync("tutorial-completed", true);
        }

        private async Task HandleAuthAction()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Close menu
            IsMenuOpen = false;

            if (_authService.IsAuthenticated)
            {
                // Confirm logout
                bool confirm = await _dialogService.ShowConfirmationAsync(
                    "Logout",
                    "Are you sure you want to logout?",
                    "Yes", "No");

                if (confirm)
                {
                    // Logout
                    await _authService.LogoutAsync();

                    // Notify properties changed
                    OnPropertyChanged(nameof(UserDisplayName));
                    OnPropertyChanged(nameof(AuthActionLabel));
                    OnPropertyChanged(nameof(AuthIcon));
                    OnPropertyChanged(nameof(ShouldShowLoginPrompt));
                }
            }
            else
            {
                // Show login dialog
                HandleShowLogin();
            }
        }

        private void HandleShowLogin()
        {
            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Show login dialog
            _dialogService.ShowLoginDialog();
        }

        private async Task HandleShareResults()
        {
            if (WinData == null) return;

            // Play sound
            _soundService?.PlaySound(SoundType.Click);

            // Create share text
            string shareText;

            if (_gameService.HasWon)
            {
                shareText = $"I solved today's Decodey puzzle in {TimeText} with {WinData.Mistakes} mistakes and scored {WinData.Score} points! Can you beat me? Play at https://decodey.game";
            }
            else
            {
                shareText = "I tried my best at Decodey, but couldn't crack the code this time. Try your luck at https://decodey.game";
            }

            // Share
            await _shareService.ShareTextAsync(shareText, "Decodey Results");
        }

        #endregion

        #region UI Update Methods

        public void UpdateScreenSize(double width, double height, DeviceOrientation orientation)
        {
            _screenWidth = width;
            _screenHeight = height;
            _deviceOrientation = orientation;

            // Update layouts for new screen size
            UpdateGridLayouts();
            UpdateTextDisplay();

            // Adjust font size based on screen width
            if (_deviceOrientation == DeviceOrientation.Portrait)
            {
                // Portrait mode
                if (_screenWidth < 360)
                {
                    CharFontSize = 14.0;
                }
                else if (_screenWidth < 480)
                {
                    CharFontSize = 16.0;
                }
                else
                {
                    CharFontSize = 18.0;
                }
            }
            else
            {
                // Landscape mode
                if (_screenWidth < 600)
                {
                    CharFontSize = 14.0;
                }
                else if (_screenWidth < 800)
                {
                    CharFontSize = 16.0;
                }
                else
                {
                    CharFontSize = 18.0;
                }
            }
        }

        private void UpdateGridLayouts()
        {
            // Create letter grid layouts
            CreateEncryptedGrid();
            CreateGuessGrid();

            // Notify UI of property changes
            OnPropertyChanged(nameof(EncryptedLetterRows));
            OnPropertyChanged(nameof(GuessLetterRows));
        }

        private void CreateEncryptedGrid()
        {
            // Get all unique encrypted letters
            var letters = GetSortedEncryptedLetters();

            // Skip if no letters
            if (letters.Count == 0)
            {
                EncryptedLetterRows = new List<List<char>>();
                return;
            }

            // Determine grid layout - depends on orientation and screen size
            int cols = DetermineGridColumns();
            int rows = (int)Math.Ceiling(letters.Count / (double)cols);

            // Create grid
            var grid = new List<List<char>>();

            for (int r = 0; r < rows; r++)
            {
                var row = new List<char>();

                for (int c = 0; c < cols; c++)
                {
                    int index = r * cols + c;

                    if (index < letters.Count)
                    {
                        row.Add(letters[index]);
                    }
                    else
                    {
                        row.Add(' '); // Empty cell
                    }
                }

                grid.Add(row);
            }

            EncryptedLetterRows = grid;
        }

        private void CreateGuessGrid()
        {
            // Grid for guessing - alphabet A-Z
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            // Determine grid layout
            int cols = DetermineGridColumns();
            int rows = (int)Math.Ceiling(alphabet.Length / (double)cols);

            // Create grid
            var grid = new List<List<char>>();

            for (int r = 0; r < rows; r++)
            {
                var row = new List<char>();

                for (int c = 0; c < cols; c++)
                {
                    int index = r * cols + c;

                    if (index < alphabet.Length)
                    {
                        row.Add(alphabet[index]);
                    }
                    else
                    {
                        row.Add(' '); // Empty cell
                    }
                }

                grid.Add(row);
            }

            GuessLetterRows = grid;
        }

        private List<char> GetSortedEncryptedLetters()
        {
            // Get all unique encrypted letters
            var encryptedText = _gameService.Encrypted;
            if (string.IsNullOrEmpty(encryptedText)) return new List<char>();

            // Get unique letters
            var letters = new HashSet<char>();

            foreach (char c in encryptedText)
            {
                if (char.IsLetter(c))
                {
                    letters.Add(c);
                }
            }

            // Convert to list for sorting
            var letterList = letters.ToList();

            // Sort letters based on user preference
            var sortType = _settingsService.GetSetting<string>("grid-sorting");

            if (sortType == "alphabetical")
            {
                // Sort alphabetically
                letterList.Sort();
            }
            else
            {
                // Sort by order of appearance in the text (default)
                letterList = encryptedText
                    .Where(char.IsLetter)
                    .Distinct()
                    .ToList();
            }

            return letterList;
        }

        private int DetermineGridColumns()
        {
            // Default columns
            int cols = 5;

            // Adjust based on orientation and screen size
            if (_deviceOrientation == DeviceOrientation.Landscape)
            {
                if (_screenWidth < 600)
                {
                    cols = 5;
                }
                else if (_screenWidth < 800)
                {
                    cols = 6;
                }
                else
                {
                    cols = 7;
                }
            }
            else
            {
                // Portrait
                if (_screenWidth < 360)
                {
                    cols = 5;
                }
                else if (_screenWidth < 480)
                {
                    cols = 6;
                }
                else
                {
                    cols = 7;
                }
            }

            return cols;
        }

        private void UpdateTextDisplay()
        {
            // Create text lines
            var lines = CreateTextLines();

            // Update observable collection
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TextLines.Clear();
                foreach (var line in lines)
                {
                    TextLines.Add(line);
                }
            });
        }

        private List<TextLine> CreateTextLines()
        {
            var lines = new List<TextLine>();

            // Get encrypted and display text
            var encrypted = _gameService.Encrypted;
            var display = _gameService.Display;

            if (string.IsNullOrEmpty(encrypted) || string.IsNullOrEmpty(display))
            {
                return lines;
            }

            // Split into paragraphs
            var encryptedParagraphs = SplitIntoParagraphs(encrypted);
            var displayParagraphs = SplitIntoParagraphs(display);

            // Create text lines for each paragraph
            for (int i = 0; i < encryptedParagraphs.Count; i++)
            {
                if (i < displayParagraphs.Count)
                {
                    var encryptedChars = encryptedParagraphs[i].ToCharArray().Select(c => c.ToString()).ToList();
                    var displayChars = CreateDisplayCharacters(displayParagraphs[i]);

                    lines.Add(new TextLine
                    {
                        EncryptedChars = encryptedChars,
                        DisplayChars = displayChars
                    });
                }
            }

            return lines;
        }

        private List<string> SplitIntoParagraphs(string text)
        {
            // Split on double newlines
            var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None).ToList();

            // If no paragraphs found, use the whole text
            if (paragraphs.Count == 0)
            {
                paragraphs.Add(text);
            }

            return paragraphs;
        }

        private List<DisplayChar> CreateDisplayCharacters(string text)
        {
            var chars = new List<DisplayChar>();

            foreach (char c in text)
            {
                // If letter, check if it has been guessed
                if (char.IsLetter(c))
                {
                    bool isGuessed = _gameService.CorrectlyGuessed.Contains(char.ToUpper(c));

                    chars.Add(new DisplayChar
                    {
                        Text = isGuessed ? c.ToString() : "█",
                        IsPlaceholder = !isGuessed
                    });
                }
                else
                {
                    // Non-letter character, always visible
                    chars.Add(new DisplayChar
                    {
                        Text = c.ToString(),
                        IsPlaceholder = false
                    });
                }
            }

            return chars;
        }

        #endregion

        #region Helper Methods

        private string DetermineRating(int score)
        {
            // Rating bands based on score
            if (score >= 9000) return "Cryptanalyst";
            if (score >= 7500) return "Code Breaker";
            if (score >= 6000) return "Cipher Expert";
            if (score >= 4500) return "Puzzle Master";
            if (score >= 3000) return "Word Wizard";
            if (score >= 1500) return "Novice Decryptor";
            return "Apprentice";
        }

        public int GetFrequency(char letter)
        {
            // Get frequency from dictionary
            if (_gameService.LetterFrequency != null &&
                _gameService.LetterFrequency.TryGetValue(letter, out int frequency))
            {
                return frequency;
            }

            return 0;
        }

        public bool IsEncryptedSelected(char letter)
        {
            return _gameService.SelectedEncrypted == letter;
        }

        public bool IsLetterGuessed(char letter)
        {
            return _gameService.CorrectlyGuessed.Contains(letter);
        }

        public bool IsIncorrectGuess(char encryptedLetter, char guessedLetter)
        {
            if (_gameService.IncorrectGuesses == null) return false;

            if (_gameService.IncorrectGuesses.TryGetValue(encryptedLetter, out var guesses))
            {
                return guesses.Contains(guessedLetter);
            }

            return false;
        }

        #endregion

        #region Property Changed

        protected bool SetProperty<T>(ref T backingField, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Additional property notifications for derived properties
            switch (propertyName)
            {
                case nameof(IsLoading):
                case nameof(ShowWinCelebration):
                    OnPropertyChanged(nameof(IsLoadingOrCelebrating));
                    break;

                case nameof(_gameService.HasWon):
                case nameof(_gameService.HasLost):
                case nameof(_gameService.IsResetting):
                    OnPropertyChanged(nameof(IsGameActive));
                    OnPropertyChanged(nameof(HasGameEnded));
                    break;
            }
        }

        #endregion
    }

    #region Event Args

    public class CorrectGuessEventArgs : EventArgs
    {
        public char EncryptedLetter { get; }
        public char GuessedLetter { get; }

        public CorrectGuessEventArgs(char encryptedLetter, char guessedLetter)
        {
            EncryptedLetter = encryptedLetter;
            GuessedLetter = guessedLetter;
        }
    }

    public class IncorrectGuessEventArgs : EventArgs
    {
        public char GuessedLetter { get; }

        public IncorrectGuessEventArgs(char guessedLetter)
        {
            GuessedLetter = guessedLetter;
        }