using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Decodey.Controls
{
    /// <summary>
    /// A custom control for the letter cells in the game grid
    /// Translated from the React component with similar functionality
    /// </summary>
    public class LetterCell : ContentView
    {
        #region Fields
        private Frame _frame;
        private Label _letterLabel;
        private Label _frequencyLabel;
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty LetterProperty =
            BindableProperty.Create(nameof(Letter), typeof(string), typeof(LetterCell), string.Empty, propertyChanged: OnLetterChanged);

        public static readonly BindableProperty FrequencyProperty =
            BindableProperty.Create(nameof(Frequency), typeof(int), typeof(LetterCell), 0, propertyChanged: OnFrequencyChanged);

        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(LetterCell), false, propertyChanged: OnSelectionChanged);

        public static readonly BindableProperty IsGuessedProperty =
            BindableProperty.Create(nameof(IsGuessed), typeof(bool), typeof(LetterCell), false, propertyChanged: OnGuessedChanged);

        public static readonly BindableProperty IsPreviouslyGuessedProperty =
            BindableProperty.Create(nameof(IsPreviouslyGuessed), typeof(bool), typeof(LetterCell), false, propertyChanged: OnPreviouslyGuessedChanged);

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(LetterCell), null);

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(LetterCell), null);

        public static readonly BindableProperty LetterTypeProperty =
            BindableProperty.Create(nameof(LetterType), typeof(LetterCellType), typeof(LetterCell), LetterCellType.Encrypted);
        #endregion

        #region Properties
        public string Letter
        {
            get => (string)GetValue(LetterProperty);
            set => SetValue(LetterProperty, value);
        }

        public int Frequency
        {
            get => (int)GetValue(FrequencyProperty);
            set => SetValue(FrequencyProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsGuessed
        {
            get => (bool)GetValue(IsGuessedProperty);
            set => SetValue(IsGuessedProperty, value);
        }

        public bool IsPreviouslyGuessed
        {
            get => (bool)GetValue(IsPreviouslyGuessedProperty);
            set => SetValue(IsPreviouslyGuessedProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public LetterCellType LetterType
        {
            get => (LetterCellType)GetValue(LetterTypeProperty);
            set => SetValue(LetterTypeProperty, value);
        }
        #endregion

        #region Constructor
        public LetterCell()
        {
            Initialize();
            ApplyInitialStyle();

            // Add tap gesture recognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnCellTapped;
            GestureRecognizers.Add(tapGesture);
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            // Create the letter label
            _letterLabel = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center
            };

            // Create the frequency indicator if needed
            _frequencyLabel = new Label
            {
                FontSize = 10,
                Opacity = 0.7,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 2, 1)
            };

            // Create the frame container
            _frame = new Frame
            {
                CornerRadius = 4,
                Padding = 0,
                HasShadow = true,
                WidthRequest = 38,
                HeightRequest = 38,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = new Grid
                {
                    Children =
                    {
                        _letterLabel,
                        _frequencyLabel
                    }
                }
            };

            // Set the frame as the content
            Content = _frame;
        }

        private void ApplyInitialStyle()
        {
            // Apply appropriate styles based on letter type
            switch (LetterType)
            {
                case LetterCellType.Encrypted:
                    _letterLabel.TextColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#4cc9f0") : Color.FromArgb("#212529");
                    break;
                case LetterCellType.Guess:
                    _letterLabel.TextColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#f8f9fa") : Color.FromArgb("#007bff");
                    break;
            }

            // Apply basic frame styles
            _frame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                Color.FromArgb("#333333") : Colors.White;
            _frame.BorderColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                Color.FromArgb("#444444") : Color.FromArgb("#dee2e6");
        }
        #endregion

        #region Event Handlers
        private static void OnLetterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell cell)
            {
                cell._letterLabel.Text = (string)newValue;
            }
        }

        private static void OnFrequencyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell cell)
            {
                int frequency = (int)newValue;

                if (frequency > 0)
                {
                    cell._frequencyLabel.Text = frequency.ToString();
                    cell._frequencyLabel.IsVisible = true;
                }
                else
                {
                    cell._frequencyLabel.IsVisible = false;
                }
            }
        }

        private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell cell)
            {
                bool isSelected = (bool)newValue;

                if (isSelected)
                {
                    // Apply selected style
                    cell._frame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#4cc9f0") : Color.FromArgb("#007bff");
                    cell._letterLabel.TextColor = Colors.White;
                    cell._frame.BorderColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#3db8df") : Color.FromArgb("#0062cc");
                    cell._frame.Scale = 1.05;
                }
                else
                {
                    // Reset to normal or guessed style
                    if (cell.IsGuessed)
                    {
                        OnGuessedChanged(bindable, false, true);
                    }
                    else if (cell.IsPreviouslyGuessed)
                    {
                        OnPreviouslyGuessedChanged(bindable, false, true);
                    }
                    else
                    {
                        // Reset to normal style
                        cell.ApplyInitialStyle();
                        cell._frame.Scale = 1.0;
                    }
                }
            }
        }

        private static void OnGuessedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell cell)
            {
                bool isGuessed = (bool)newValue;

                if (isGuessed)
                {
                    // Apply guessed style
                    cell._frame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#222222") : Color.FromArgb("#e9ecef");
                    cell._letterLabel.TextColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#aaaaaa") : Color.FromArgb("#495057");
                    cell._frame.BorderColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#7a7a7a") : Color.FromArgb("#ced4da");
                    cell._frame.Scale = 1.0;
                }
                else if (!cell.IsSelected && !cell.IsPreviouslyGuessed)
                {
                    // Reset to normal style if not selected and not previously guessed
                    cell.ApplyInitialStyle();
                }
            }
        }

        private static void OnPreviouslyGuessedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell cell)
            {
                bool isPreviouslyGuessed = (bool)newValue;

                if (isPreviouslyGuessed)
                {
                    // Apply previously guessed style
                    cell._frame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#330000") : Color.FromArgb("#fff5f5");
                    cell._frame.BorderColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                        Color.FromArgb("#aa0000") : Color.FromArgb("#ffcccc");
                    cell._frame.Opacity = 0.6;
                }
                else if (!cell.IsSelected && !cell.IsGuessed)
                {
                    // Reset to normal style if not selected and not guessed
                    cell.ApplyInitialStyle();
                    cell._frame.Opacity = 1.0;
                }
            }
        }

        private void OnCellTapped(object sender, EventArgs e)
        {
            // Execute the command if letter is not guessed yet
            if (!IsGuessed && Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Animates a correct guess with visual feedback
        /// </summary>
        public async Task AnimateCorrectGuessAsync()
        {
            // Store original colors
            var originalBgColor = _frame.BackgroundColor;
            var originalTextColor = _letterLabel.TextColor;

            // Flash animation (Green -> Guessed Style)
            _frame.BackgroundColor = Color.FromArgb("#28a745");
            _letterLabel.TextColor = Colors.White;

            await Task.Delay(300);

            // Transition to guessed style
            _frame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                Color.FromArgb("#222222") : Color.FromArgb("#e9ecef");
            _letterLabel.TextColor = Application.Current.RequestedTheme == AppTheme.Dark ?
                Color.FromArgb("#aaaaaa") : Color.FromArgb("#495057");
        }

        /// <summary>
        /// Animates an incorrect guess with visual feedback
        /// </summary>
        public async Task AnimateIncorrectGuessAsync()
        {
            // Store original colors
            var originalBgColor = _frame.BackgroundColor;
            var originalTextColor = _letterLabel.TextColor;

            // Flash animation (Red -> Original)
            _frame.BackgroundColor = Color.FromArgb("#dc3545");
            _letterLabel.TextColor = Colors.White;

            await Task.Delay(300);

            // Return to original state
            _frame.BackgroundColor = originalBgColor;
            _letterLabel.TextColor = originalTextColor;
        }
        #endregion
    }

    /// <summary>
    /// Enum for letter cell types
    /// </summary>
    public enum LetterCellType
    {
        Encrypted,
        Guess
    }
}