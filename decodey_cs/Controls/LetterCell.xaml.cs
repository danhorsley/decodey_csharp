using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Decodey.Controls
{
    public partial class LetterCell : ContentView
    {
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

        public LetterCell()
        {
            InitializeComponent();

            // Add tap gesture
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnCellTapped;
            GestureRecognizers.Add(tapGesture);

            // Initialize UI
            UpdateLetterText();
            UpdateFrequencyText();
            UpdateVisualState();
        }

        #region Event Handlers

        private static void OnLetterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell letterCell)
            {
                letterCell.UpdateLetterText();
            }
        }

        private static void OnFrequencyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell letterCell)
            {
                letterCell.UpdateFrequencyText();
            }
        }

        private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell letterCell)
            {
                letterCell.UpdateVisualState();
            }
        }

        private static void OnGuessedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell letterCell)
            {
                letterCell.UpdateVisualState();
            }
        }

        private static void OnPreviouslyGuessedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LetterCell letterCell)
            {
                letterCell.UpdateVisualState();
            }
        }

        private void OnCellTapped(object sender, EventArgs e)
        {
            // Only execute command if not already guessed
            if (!IsGuessed && Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        #endregion

        #region UI Update Methods

        private void UpdateLetterText()
        {
            LetterLabel.Text = Letter;
        }

        private void UpdateFrequencyText()
        {
            if (Frequency > 0 && LetterType == LetterCellType.Encrypted)
            {
                FrequencyLabel.Text = Frequency.ToString();
                FrequencyLabel.IsVisible = true;
            }
            else
            {
                FrequencyLabel.IsVisible = false;
            }
        }

        private void UpdateVisualState()
        {
            // Reset styles
            CellFrame.Style = (Style)GetValue(FrameStyleProperty);
            LetterLabel.Style = (Style)GetValue(LabelStyleProperty);

            // Apply appropriate style based on state
            if (IsSelected)
            {
                CellFrame.Style = (Style)GetValue(SelectedFrameStyleProperty);
                LetterLabel.Style = (Style)GetValue(SelectedLabelStyleProperty);
                CellFrame.Scale = 1.05;
            }
            else if (IsGuessed)
            {
                CellFrame.Style = (Style)GetValue(GuessedFrameStyleProperty);
                LetterLabel.Style = (Style)GetValue(GuessedLabelStyleProperty);
                CellFrame.Scale = 1.0;
            }
            else if (IsPreviouslyGuessed)
            {
                CellFrame.Style = (Style)GetValue(PreviouslyGuessedFrameStyleProperty);
                LetterLabel.TextColor = Colors.Red;
                CellFrame.Scale = 1.0;
                CellFrame.Opacity = 0.6;
            }
            else
            {
                CellFrame.Scale = 1.0;
                CellFrame.Opacity = 1.0;
            }

            // Check for encrypted vs guess type
            if (LetterType == LetterCellType.Encrypted)
            {
                // Set encrypted-specific styles if needed
            }
            else if (LetterType == LetterCellType.Guess)
            {
                // Set guess-specific styles if needed
            }
        }

        #endregion

        #region Animation Methods

        public async Task AnimateCorrectGuessAsync()
        {
            // Store original styles
            var originalFrameStyle = CellFrame.Style;
            var originalLabelStyle = LetterLabel.Style;
            var originalScale = CellFrame.Scale;

            // Apply flash style
            CellFrame.BackgroundColor = Colors.Green;
            LetterLabel.TextColor = Colors.White;

            // Scale up
            await CellFrame.ScaleTo(1.1, 150, Easing.SpringOut);

            // Scale down
            await CellFrame.ScaleTo(1.0, 150, Easing.SpringIn);

            // Transition to guessed style
            CellFrame.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#222222")
                : Color.FromArgb("#e9ecef");

            LetterLabel.TextColor = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#aaaaaa")
                : Color.FromArgb("#495057");

            // Update state
            IsGuessed = true;
            IsSelected = false;
        }

        public async Task AnimateIncorrectGuessAsync()
        {
            // Store original colors
            var originalBgColor = CellFrame.BackgroundColor;
            var originalTextColor = LetterLabel.TextColor;

            // Flash animation
            CellFrame.BackgroundColor = Colors.Red;
            LetterLabel.TextColor = Colors.White;

            // Shake animation
            uint shakeDuration = 50;
            for (int i = 0; i < 3; i++)
            {
                await CellFrame.TranslateTo(-5, 0, shakeDuration);
                await CellFrame.TranslateTo(5, 0, shakeDuration);
            }

            await CellFrame.TranslateTo(0, 0, shakeDuration);

            // Return to original colors
            CellFrame.BackgroundColor = originalBgColor;
            LetterLabel.TextColor = originalTextColor;

            // Update state
            IsPreviouslyGuessed = true;
        }

        #endregion

        #region Style Resources

        // These would normally be in ResourceDictionary, but we're defining here for simplicity
        private static Style FrameStyleProperty = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = BackgroundColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#333333") : Colors.White },
                new Setter { Property = BorderColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#444444") : Color.FromArgb("#dee2e6") },
                new Setter { Property = CornerRadiusProperty, Value = 4 },
                new Setter { Property = PaddingProperty, Value = new Thickness(0) },
                new Setter { Property = HasShadowProperty, Value = true },
                new Setter { Property = WidthRequestProperty, Value = 38 },
                new Setter { Property = HeightRequestProperty, Value = 38 },
                new Setter { Property = HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = VerticalOptionsProperty, Value = LayoutOptions.Center }
            }
        };

        private static Style LabelStyleProperty = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = VerticalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = FontAttributesProperty, Value = FontAttributes.Bold },
                new Setter { Property = FontSizeProperty, Value = 20 },
                new Setter { Property = HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
                new Setter { Property = VerticalTextAlignmentProperty, Value = TextAlignment.Center },
                new Setter { Property = TextColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#4cc9f0") : Color.FromArgb("#212529") }
            }
        };

        private static Style SelectedFrameStyleProperty = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = BackgroundColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#4cc9f0") : Color.FromArgb("#007bff") },
                new Setter { Property = BorderColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#3db8df") : Color.FromArgb("#0062cc") }
            }
        };

        private static Style SelectedLabelStyleProperty = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = TextColorProperty, Value = Colors.White }
            }
        };

        private static Style GuessedFrameStyleProperty = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = BackgroundColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#222222") : Color.FromArgb("#e9ecef") },
                new Setter { Property = BorderColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#7a7a7a") : Color.FromArgb("#ced4da") }
            }
        };

        private static Style GuessedLabelStyleProperty = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = TextColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#aaaaaa") : Color.FromArgb("#495057") }
            }
        };

        private static Style PreviouslyGuessedFrameStyleProperty = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = BackgroundColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#330000") : Color.FromArgb("#fff5f5") },
                new Setter { Property = BorderColorProperty, Value = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#aa0000") : Color.FromArgb("#ffcccc") }
            }
        };

        private static Style FlashCorrectFrameStyleProperty = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = BackgroundColorProperty, Value = Color.FromArgb("#28a745") },
                new Setter { Property = BorderColorProperty, Value = Color.FromArgb("#218838") }
            }
        };

        private static Style FlashLabelStyleProperty = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = TextColorProperty, Value = Colors.White }
            }
        };

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