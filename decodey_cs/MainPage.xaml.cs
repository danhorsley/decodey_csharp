using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Decodey.ViewModels;
using Decodey.Controls;
using Decodey.Services;

namespace Decodey
{
    public partial class MainPage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly AnimationService _animationService;
        private double _screenWidth;
        private double _screenHeight;
        private double _previousWidth;
        private double _previousHeight;
        private DeviceOrientation _currentOrientation;

        // Tutorial positions for various elements
        private readonly string[] _tutorialTargets = {
            "TextDisplay",
            "EncryptedGrid",
            "HintButtonFrame",
            "GuessGrid"
        };

        // Keep track of letters on the grid
        private LetterCell[,] _encryptedCells;
        private LetterCell[,] _guessCells;

        public MainPage()
        {
            InitializeComponent();

            // Get the view model
            _viewModel = BindingContext as GameViewModel;

            // Initialize animation service
            _animationService = App.GetService<IAnimationService>() ?? new AnimationService();

            // Subscribe to view model events
            SubscribeToViewModelEvents();

            // Initialize menu animation
            InitializeMenuAnimation();

            // Size changed event
            SizeChanged += OnSizeChanged;
        }

        private void SubscribeToViewModelEvents()
        {
            if (_viewModel == null) return;

            // Layout updates
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameViewModel.IsMenuOpen))
                {
                    UpdateMenuVisibility();
                }
                else if (e.PropertyName == nameof(GameViewModel.EncryptedLetterRows) ||
                         e.PropertyName == nameof(GameViewModel.GuessLetterRows))
                {
                    // Refresh the letter grids
                    RefreshLetterGrids();
                }
                else if (e.PropertyName == nameof(GameViewModel.IsTutorialActive))
                {
                    if (_viewModel.IsTutorialActive)
                    {
                        StartTutorial();
                    }
                }
                else if (e.PropertyName == nameof(GameViewModel.CurrentTutorialStep))
                {
                    UpdateTutorialStep();
                }
            };

            // Game events
            _viewModel.CorrectGuessAnimationRequested += OnCorrectGuessAnimationRequested;
            _viewModel.IncorrectGuessAnimationRequested += OnIncorrectGuessAnimationRequested;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            _screenWidth = Width;
            _screenHeight = Height;

            // Skip if size hasn't actually changed
            if (Math.Abs(_screenWidth - _previousWidth) < 1 &&
                Math.Abs(_screenHeight - _previousHeight) < 1)
            {
                return;
            }

            _previousWidth = _screenWidth;
            _previousHeight = _screenHeight;

            // Update layout based on orientation
            UpdateLayoutForScreenSize();

            // Update the view model
            if (_viewModel != null)
            {
                _viewModel.UpdateScreenSize(_screenWidth, _screenHeight, _currentOrientation);
            }
        }

        private void UpdateLayoutForScreenSize()
        {
            // Determine orientation
            _currentOrientation = _screenWidth > _screenHeight ?
                DeviceOrientation.Landscape : DeviceOrientation.Portrait;

            // Apply appropriate layout to the game dashboard
            if (_currentOrientation == DeviceOrientation.Portrait)
            {
                // Portrait layout - vertical arrangement
                GameDashboardGrid.ColumnDefinitions.Clear();
                GameDashboardGrid.RowDefinitions.Clear();

                // Add rows
                GameDashboardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                GameDashboardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                GameDashboardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Position elements
                Grid.SetRow(EncryptedGrid, 0);
                Grid.SetColumn(EncryptedGrid, 0);
                Grid.SetRowSpan(EncryptedGrid, 1);
                Grid.SetColumnSpan(EncryptedGrid, 1);

                Grid.SetRow(HintButtonFrame, 1);
                Grid.SetColumn(HintButtonFrame, 0);
                Grid.SetRowSpan(HintButtonFrame, 1);
                Grid.SetColumnSpan(HintButtonFrame, 1);

                Grid.SetRow(GuessGrid, 2);
                Grid.SetColumn(GuessGrid, 0);
                Grid.SetRowSpan(GuessGrid, 1);
                Grid.SetColumnSpan(GuessGrid, 1);
            }
            else
            {
                // Landscape layout - horizontal arrangement
                GameDashboardGrid.RowDefinitions.Clear();
                GameDashboardGrid.ColumnDefinitions.Clear();

                // Add columns
                GameDashboardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                GameDashboardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                GameDashboardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                // Position elements
                Grid.SetRow(EncryptedGrid, 0);
                Grid.SetColumn(EncryptedGrid, 0);
                Grid.SetRowSpan(EncryptedGrid, 1);
                Grid.SetColumnSpan(EncryptedGrid, 1);

                Grid.SetRow(HintButtonFrame, 0);
                Grid.SetColumn(HintButtonFrame, 1);
                Grid.SetRowSpan(HintButtonFrame, 1);
                Grid.SetColumnSpan(HintButtonFrame, 1);

                Grid.SetRow(GuessGrid, 0);
                Grid.SetColumn(GuessGrid, 2);
                Grid.SetRowSpan(GuessGrid, 1);
                Grid.SetColumnSpan(GuessGrid, 1);
            }

            // Also update the letter grids to ensure they're properly sized
            RefreshLetterGrids();
        }

        #region Menu Animation

        private void InitializeMenuAnimation()
        {
            // Menu starts off-screen
            SideMenu.TranslationX = -SideMenu.WidthRequest;
        }

        private void UpdateMenuVisibility()
        {
            var translation = _viewModel.IsMenuOpen ? 0 : -SideMenu.WidthRequest;

            // Animate menu sliding in/out
            SideMenu.TranslateTo(translation, 0, 250, Easing.CubicOut);
        }

        #endregion

        #region Letter Grids

        /// <summary>
        /// Refreshes the letter grids with current data from the view model
        /// </summary>
        private void RefreshLetterGrids()
        {
            // Skip if view model is null or has no data
            if (_viewModel == null ||
                _viewModel.EncryptedLetterRows == null ||
                _viewModel.GuessLetterRows == null)
            {
                return;
            }

            // Calculate grid sizes
            int encryptedRows = _viewModel.EncryptedLetterRows.Count;
            int encryptedCols = encryptedRows > 0 ? _viewModel.EncryptedLetterRows[0].Count : 0;

            int guessRows = _viewModel.GuessLetterRows.Count;
            int guessCols = guessRows > 0 ? _viewModel.GuessLetterRows[0].Count : 0;

            // Clear existing grids
            EncryptedGrid.Children.Clear();
            EncryptedGrid.RowDefinitions.Clear();
            EncryptedGrid.ColumnDefinitions.Clear();

            GuessGrid.Children.Clear();
            GuessGrid.RowDefinitions.Clear();
            GuessGrid.ColumnDefinitions.Clear();

            // Skip if no data
            if (encryptedRows == 0 || guessRows == 0)
            {
                return;
            }

            // Create new cell arrays
            _encryptedCells = new LetterCell[encryptedRows, encryptedCols];
            _guessCells = new LetterCell[guessRows, guessCols];

            // Set up encrypted grid
            SetupGrid(EncryptedGrid, encryptedRows, encryptedCols);

            // Set up guess grid
            SetupGrid(GuessGrid, guessRows, guessCols);

            // Populate encrypted grid
            for (int row = 0; row < encryptedRows; row++)
            {
                for (int col = 0; col < encryptedCols; col++)
                {
                    char letter = row < _viewModel.EncryptedLetterRows.Count &&
                                 col < _viewModel.EncryptedLetterRows[row].Count ?
                                 _viewModel.EncryptedLetterRows[row][col] : ' ';

                    // Skip spaces
                    if (letter == ' ') continue;

                    // Create the cell
                    var cell = new LetterCell
                    {
                        Letter = letter.ToString(),
                        LetterType = LetterCellType.Encrypted,
                        Frequency = _viewModel.GetFrequency(letter),
                        IsSelected = _viewModel.IsEncryptedSelected(letter),
                        IsGuessed = _viewModel.IsLetterGuessed(letter),
                        Command = _viewModel.EncryptedSelectCommand,
                        CommandParameter = letter
                    };

                    // Add to grid
                    EncryptedGrid.Children.Add(cell);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    // Store reference
                    _encryptedCells[row, col] = cell;
                }
            }

            // Populate guess grid
            for (int row = 0; row < guessRows; row++)
            {
                for (int col = 0; col < guessCols; col++)
                {
                    char letter = row < _viewModel.GuessLetterRows.Count &&
                                 col < _viewModel.GuessLetterRows[row].Count ?
                                 _viewModel.GuessLetterRows[row][col] : ' ';

                    // Skip spaces
                    if (letter == ' ') continue;

                    // Create the cell
                    var cell = new LetterCell
                    {
                        Letter = letter.ToString(),
                        LetterType = LetterCellType.Guess,
                        IsPreviouslyGuessed = _viewModel.IsIncorrectGuess(
                            _viewModel.SelectedEncrypted ?? ' ', letter),
                        Command = _viewModel.SubmitGuessCommand,
                        CommandParameter = letter
                    };

                    // Add to grid
                    GuessGrid.Children.Add(cell);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    // Store reference
                    _guessCells[row, col] = cell;
                }
            }
        }

        private void SetupGrid(Grid grid, int rows, int cols)
        {
            // Add row definitions
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add column definitions
            for (int i = 0; i < cols; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
        }

        #endregion

        #region Animation Handlers

        private async void OnCorrectGuessAnimationRequested(object sender, CorrectGuessEventArgs e)
        {
            // Find the encrypted cell for this letter
            var encryptedCell = FindEncryptedCell(e.EncryptedLetter);

            // Find the guess cell for this letter
            var guessCell = FindGuessCell(e.GuessedLetter);

            if (encryptedCell != null)
            {
                // Animate correct guess
                await encryptedCell.AnimateCorrectGuessAsync();

                // Update selected state
                encryptedCell.IsGuessed = true;
                encryptedCell.IsSelected = false;
            }

            if (guessCell != null)
            {
                // Animate correct guess
                await guessCell.AnimateCorrectGuessAsync();
            }
        }

        private async void OnIncorrectGuessAnimationRequested(object sender, IncorrectGuessEventArgs e)
        {
            // Find the guess cell for this letter
            var guessCell = FindGuessCell(e.GuessedLetter);

            if (guessCell != null)
            {
                // Animate incorrect guess
                await guessCell.AnimateIncorrectGuessAsync();

                // Mark as previously guessed
                guessCell.IsPreviouslyGuessed = true;
            }
        }

        private LetterCell FindEncryptedCell(char letter)
        {
            if (_encryptedCells == null) return null;

            // Search all cells
            for (int row = 0; row < _encryptedCells.GetLength(0); row++)
            {
                for (int col = 0; col < _encryptedCells.GetLength(1); col++)
                {
                    var cell = _encryptedCells[row, col];
                    if (cell != null && cell.Letter == letter.ToString())
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        private LetterCell FindGuessCell(char letter)
        {
            if (_guessCells == null) return null;

            // Search all cells
            for (int row = 0; row < _guessCells.GetLength(0); row++)
            {
                for (int col = 0; col < _guessCells.GetLength(1); col++)
                {
                    var cell = _guessCells[row, col];
                    if (cell != null && cell.Letter == letter.ToString())
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Tutorial

        private void StartTutorial()
        {
            // Set initial tutorial step
            _viewModel.CurrentTutorialStep = 0;
            UpdateTutorialStep();
        }

        private void UpdateTutorialStep()
        {
            int step = _viewModel.CurrentTutorialStep;

            // Update tutorial content
            switch (step)
            {
                case 0: // Welcome
                    TutorialTitle.Text = "Welcome to Decodey!";
                    TutorialDescription.Text = "This game challenges you to decode a hidden quote by solving a letter substitution cipher. Let's learn how to play!";
                    TutorialHighlight.IsVisible = false;
                    break;

                case 1: // Text Display
                    TutorialTitle.Text = "The Encrypted Quote";
                    TutorialDescription.Text = "This is the encrypted quote you need to solve. Each letter has been substituted with another letter consistently throughout the text.";
                    PositionTutorialHighlight("TextDisplay");
                    break;

                case 2: // Encrypted Grid
                    TutorialTitle.Text = "Available Letters";
                    TutorialDescription.Text = "These are all the encrypted letters that appear in the quote. Tap one to select it for guessing.";
                    PositionTutorialHighlight("EncryptedGrid");
                    break;

                case 3: // Hint Button
                    TutorialTitle.Text = "Hint Button";
                    TutorialDescription.Text = "Need help? Tap this button to get a hint. The number shows how many hints you have available.";
                    PositionTutorialHighlight("HintButtonFrame");
                    break;

                case 4: // Guess Grid
                    TutorialTitle.Text = "Make Your Guess";
                    TutorialDescription.Text = "After selecting an encrypted letter, tap a letter here to guess what it stands for in the original quote.";
                    PositionTutorialHighlight("GuessGrid");
                    break;

                case 5: // Completion
                    TutorialTitle.Text = "You're Ready!";
                    TutorialDescription.Text = "That's it! Solve the puzzle with as few mistakes as possible to earn a high score. Good luck!";
                    TutorialHighlight.IsVisible = false;
                    break;
            }
        }

        private void PositionTutorialHighlight(string targetName)
        {
            View targetElement = null;

            // Find the target element based on name
            switch (targetName)
            {
                case "TextDisplay":
                    // First Frame in Row 1
                    targetElement = RootGrid.Children.FirstOrDefault(c =>
                        Grid.GetRow(c) == 1 && c is Frame) as View;
                    break;

                case "EncryptedGrid":
                    targetElement = EncryptedGrid;
                    break;

                case "HintButtonFrame":
                    targetElement = HintButtonFrame;
                    break;

                case "GuessGrid":
                    targetElement = GuessGrid;
                    break;
            }

            if (targetElement != null)
            {
                // Position highlight over the target
                var bounds = GetBoundsRelativeTo(targetElement, RootGrid);

                // Make highlight visible and set its position
                TutorialHighlight.IsVisible = true;

                // Position the highlight
                AbsoluteLayout.SetLayoutBounds(TutorialHighlight, bounds);

                // Position the tutorial box to not overlap with the highlight
                PositionTutorialBox(bounds);
            }
            else
            {
                TutorialHighlight.IsVisible = false;
            }
        }

        private Rect GetBoundsRelativeTo(View view, View relativeTo)
        {
            // Get the bounds of the view in its own coordinate space
            var bounds = new Rect(0, 0, view.Width, view.Height);

            // Transform to screen coordinates
            var screenBounds = view.GetAbsoluteBounds();
            var relativeBounds = relativeTo.GetAbsoluteBounds();

            // Calculate bounds relative to the container
            return new Rect(
                screenBounds.X - relativeBounds.X,
                screenBounds.Y - relativeBounds.Y,
                screenBounds.Width,
                screenBounds.Height);
        }

        private void PositionTutorialBox(Rect highlightBounds)
        {
            // Calculate where to position the tutorial box
            double centerX = highlightBounds.X + (highlightBounds.Width / 2);
            double centerY = highlightBounds.Y + (highlightBounds.Height / 2);

            // Try to position below first
            double boxY = highlightBounds.Y + highlightBounds.Height + 20;

            // If that would be off-screen, try above
            if (boxY + TutorialBox.Height > Height)
            {
                boxY = highlightBounds.Y - TutorialBox.Height - 20;
            }

            // If still off-screen, position in the center
            if (boxY < 0 || boxY + TutorialBox.Height > Height)
            {
                boxY = (Height - TutorialBox.Height) / 2;
            }

            // Center horizontally, but ensure it's on screen
            double boxX = centerX - (TutorialBox.Width / 2);
            boxX = Math.Max(10, Math.Min(boxX, Width - TutorialBox.Width - 10));

            // Set the position
            AbsoluteLayout.SetLayoutBounds(TutorialBox, new Rect(boxX, boxY, TutorialBox.Width, TutorialBox.Height));
        }

        #endregion
    }

    // Enum for device orientation
    public enum DeviceOrientation
    {
        Portrait,
        Landscape
    }
}