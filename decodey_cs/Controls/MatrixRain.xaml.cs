using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace Decodey.Controls
{
    public partial class MatrixRain : ContentView
    {
        private double _screenWidth;
        private double _screenHeight;
        private double _previousWidth;
        private double _previousHeight;
        private Random _random = new Random();
        private List<MatrixColumn> _columns = new List<MatrixColumn>();
        private bool _isActive = false;
        private Color _color = Colors.Green;
        private string _message = string.Empty;
        private int _density = 50;
        private double _speed = 1.0;

        public static readonly BindableProperty IsActiveProperty =
            BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(MatrixRain), false, propertyChanged: OnIsActiveChanged);

        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color), typeof(Color), typeof(MatrixRain), Colors.Green);

        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(MatrixRain), string.Empty);

        public static readonly BindableProperty DensityProperty =
            BindableProperty.Create(nameof(Density), typeof(int), typeof(MatrixRain), 50);

        public static readonly BindableProperty SpeedProperty =
            BindableProperty.Create(nameof(Speed), typeof(double), typeof(MatrixRain), 1.0);

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public int Density
        {
            get => (int)GetValue(DensityProperty);
            set => SetValue(DensityProperty, value);
        }

        public double Speed
        {
            get => (double)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        private static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MatrixRain)bindable;
            bool isActive = (bool)newValue;

            if (isActive)
            {
                control.StartAnimation();
            }
            else
            {
                control.StopAnimation();
            }
        }

        public MatrixRain()
        {
            InitializeComponent();

            // Setup the container
            MainContainer.BackgroundColor = Colors.Black;

            // Subscribe to size changed event
            SizeChanged += OnMatrixRainSizeChanged;
        }

        private void OnMatrixRainSizeChanged(object sender, EventArgs e)
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

            // Reset columns if active
            if (IsActive)
            {
                ResetColumns();
            }
        }

        private void StartAnimation()
        {
            if (_isActive) return;
            _isActive = true;

            // Initialize columns
            ResetColumns();

            // Start animation loop
            StartAnimationLoop();
        }

        private void StopAnimation()
        {
            _isActive = false;
            MainContainer.Children.Clear();
            _columns.Clear();
        }

        private void ResetColumns()
        {
            // Clear existing columns
            MainContainer.Children.Clear();
            _columns.Clear();

            // Calculate number of columns based on width and density
            int numColumns = (int)(_screenWidth * _density / 1000);

            // Create columns
            for (int i = 0; i < numColumns; i++)
            {
                // Calculate random x position
                double x = _random.NextDouble() * _screenWidth;

                // Create column
                var column = new MatrixColumn
                {
                    X = x,
                    Speed = (1 + _random.NextDouble()) * _speed,
                    Characters = GenerateRandomCharacters(10 + _random.Next(15)),
                    StartDelay = _random.Next(100)
                };

                _columns.Add(column);
            }
        }

        private char[] GenerateRandomCharacters(int length)
        {
            char[] chars = new char[length];
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+[]{}|;:,.<>?";

            for (int i = 0; i < length; i++)
            {
                chars[i] = charset[_random.Next(charset.Length)];
            }

            return chars;
        }

        private async void StartAnimationLoop()
        {
            while (_isActive)
            {
                // Update positions
                UpdateColumns();

                // Redraw columns
                DrawColumns();

                // Wait for next frame
                await Task.Delay(50);
            }
        }

        private void UpdateColumns()
        {
            // Update column positions
            foreach (var column in _columns)
            {
                if (column.StartDelay > 0)
                {
                    column.StartDelay--;
                    continue;
                }

                column.Y += column.Speed;

                // Reset column if it's off screen
                if (column.Y > _screenHeight + column.Characters.Length * 20)
                {
                    column.Y = -column.Characters.Length * 20;
                    column.X = _random.NextDouble() * _screenWidth;
                    column.Speed = (1 + _random.NextDouble()) * _speed;
                    column.Characters = GenerateRandomCharacters(10 + _random.Next(15));
                }
            }
        }

        private void DrawColumns()
        {
            // Clear container
            MainContainer.Children.Clear();

            // Draw each column
            foreach (var column in _columns)
            {
                if (column.StartDelay > 0) continue;

                // Draw each character in the column
                for (int i = 0; i < column.Characters.Length; i++)
                {
                    double y = column.Y + i * 20;

                    // Skip if offscreen
                    if (y < -20 || y > _screenHeight) continue;

                    // Calculate opacity (fade out towards the end)
                    double opacity = 1 - (double)i / column.Characters.Length;

                    // Create label for character
                    var label = new Label
                    {
                        Text = column.Characters[i].ToString(),
                        TextColor = _color.WithAlpha((float)opacity),
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Start
                    };

                    // Set position
                    AbsoluteLayout.SetLayoutBounds(label, new Rect(column.X, y, 20, 20));
                    AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.None);

                    // Add to container
                    MainContainer.Children.Add(label);
                }
            }

            // Add message if provided
            if (!string.IsNullOrEmpty(_message))
            {
                var messageLabel = new Label
                {
                    Text = _message,
                    TextColor = _color,
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                };

                // Set position
                AbsoluteLayout.SetLayoutBounds(messageLabel, new Rect(0, _screenHeight / 2 - 50, _screenWidth, 100));
                AbsoluteLayout.SetLayoutFlags(messageLabel, AbsoluteLayoutFlags.None);

                // Add to container
                MainContainer.Children.Add(messageLabel);
            }
        }

        // Helper class for matrix columns
        private class MatrixColumn
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public char[] Characters { get; set; }
            public int StartDelay { get; set; }
        }
    }
}