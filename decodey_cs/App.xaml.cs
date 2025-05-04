using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;

namespace Decodey.Controls
{
    /// <summary>
    /// Matrix Rain effect custom control
    /// Translated from MatrixRainLoading.js component
    /// </summary>
    public class MatrixRain : GraphicsView
    {
        private readonly Random _random = new Random();
        private readonly List<RainDrop> _rainDrops = new List<RainDrop>();
        private readonly MatrixRainDrawable _drawable;
        private bool _isActive;

        #region Bindable Properties

        // Active state - controls if animation is running
        public static readonly BindableProperty IsActiveProperty =
            BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(MatrixRain), false,
                propertyChanged: OnIsActiveChanged);

        // Matrix character color
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color), typeof(Color), typeof(MatrixRain), Colors.LimeGreen,
                propertyChanged: OnColorChanged);

        // Overlay message to display
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(MatrixRain), string.Empty,
                propertyChanged: OnMessageChanged);

        // Density of raindrops (higher = more drops)
        public static readonly BindableProperty DensityProperty =
            BindableProperty.Create(nameof(Density), typeof(double), typeof(MatrixRain), 50.0,
                propertyChanged: OnDensityChanged);

        // Speed of animation (higher = faster)
        public static readonly BindableProperty SpeedProperty =
            BindableProperty.Create(nameof(Speed), typeof(double), typeof(MatrixRain), 1.0,
                propertyChanged: OnSpeedChanged);

        #endregion

        #region Properties

        // Whether the animation is active
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        // Color of the matrix characters
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        // Overlay message
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        // Density of raindrops
        public double Density
        {
            get => (double)GetValue(DensityProperty);
            set => SetValue(DensityProperty, value);
        }

        // Speed of animation
        public double Speed
        {
            get => (double)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        #endregion

        #region Constructor

        public MatrixRain()
        {
            // Create and set the drawable
            _drawable = new MatrixRainDrawable(_rainDrops, this);
            Drawable = _drawable;

            // Set default background
            BackgroundColor = Colors.Black;

            // When this control is added to the visual tree
            HandlerChanged += OnHandlerChanged;
        }

        #endregion

        #region Lifecycle Methods

        private void OnHandlerChanged(object sender, EventArgs e)
        {
            if (Handler != null)
            {
                // Control has been added to the visual tree
                if (IsActive)
                {
                    StartAnimation();
                }
            }
            else
            {
                // Control has been removed from the visual tree
                StopAnimation();
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Reset raindrops when size changes
            _rainDrops.Clear();
            if (IsActive)
            {
                InitializeRainDrops();
            }
        }

        #endregion

        #region Property Changed Handlers

        private static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MatrixRain control)
            {
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
        }

        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MatrixRain control)
            {
                control._drawable.CharColor = (Color)newValue;
                control.Invalidate();
            }
        }

        private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MatrixRain control)
            {
                control._drawable.Message = (string)newValue;
                control.Invalidate();
            }
        }

        private static void OnDensityChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MatrixRain control && control._isActive)
            {
                // Reset raindrops with new density
                control._rainDrops.Clear();
                control.InitializeRainDrops();
            }
        }

        private static void OnSpeedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MatrixRain control)
            {
                control._drawable.Speed = (double)newValue;
            }
        }

        #endregion

        #region Animation Control

        // Start the matrix rain animation
        private void StartAnimation()
        {
            if (_isActive) return;

            _isActive = true;
            InitializeRainDrops();

            // Start the animation loop
            Task.Run(AnimationLoop);
        }

        // Stop the matrix rain animation
        private void StopAnimation()
        {
            _isActive = false;
        }

        // Initialize the raindrops based on control size and density
        private void InitializeRainDrops()
        {
            // Calculate how many columns we need based on width and density
            int width = (int)Width;
            int height = (int)Height;

            if (width <= 0 || height <= 0) return;

            // Calculate number of columns (density is raindrops per 300px width)
            int columnCount = Math.Max(5, (int)(width * (Density / 300.0)));
            double columnWidth = width / (double)columnCount;

            // Create raindrops
            for (int i = 0; i < columnCount; i++)
            {
                // Calculate x position (center of column)
                double x = i * columnWidth + (columnWidth / 2);

                // Randomly distribute starting y positions
                double y = _random.NextDouble() * height;

                // Create raindrop with random properties
                var raindrop = new RainDrop
                {
                    X = x,
                    Y = y,
                    Speed = (0.5 + _random.NextDouble()) * 2,
                    Length = _random.Next(5, 15),
                    Chars = GenerateRandomChars(_random.Next(5, 15)),
                    HeadOpacity = 1.0,
                    TailOpacity = 0.2
                };

                _rainDrops.Add(raindrop);
            }
        }

        // Animation loop that runs until the control is deactivated
        private async Task AnimationLoop()
        {
            while (_isActive)
            {
                // Update all raindrops
                UpdateRaindrops();

                // Invalidate the control to trigger redraw
                MainThread.BeginInvokeOnMainThread(Invalidate);

                // Wait for next frame
                await Task.Delay(TimeSpan.FromMilliseconds(1000 / 30)); // 30 FPS
            }
        }

        // Update raindrops positions and properties
        private void UpdateRaindrops()
        {
            int height = (int)Height;
            if (height <= 0) return;

            foreach (var drop in _rainDrops)
            {
                // Move the raindrop down
                drop.Y += drop.Speed * Speed;

                // If raindrop is off the bottom, reset to top
                if (drop.Y > height + (drop.Length * 20))
                {
                    drop.Y = -drop.Length * 20;
                    drop.X = _random.NextDouble() * Width;
                    drop.Speed = (0.5 + _random.NextDouble()) * 2;
                    drop.Chars = GenerateRandomChars(_random.Next(5, 15));
                }

                // Occasionally change characters
                if (_random.NextDouble() < 0.05)
                {
                    int changeIndex = _random.Next(drop.Chars.Count);
                    drop.Chars[changeIndex] = GetRandomMatrixChar();
                }
            }
        }

        // Generate a list of random matrix characters
        private List<string> GenerateRandomChars(int count)
        {
            var chars = new List<string>();
            for (int i = 0; i < count; i++)
            {
                chars.Add(GetRandomMatrixChar());
            }
            return chars;
        }

        // Get a random matrix character
        private string GetRandomMatrixChar()
        {
            // Set of Matrix-like characters
            string[] matrixChars = {
                "0", "1", "ｱ", "ｲ", "ｳ", "ｴ", "ｵ", "ｶ", "ｷ", "ｸ", "ｹ", "ｺ", "ｻ", "ｼ", "ｽ", "ｾ", "ｿ",
                "ﾀ", "ﾁ", "ﾂ", "ﾃ", "ﾄ", "ﾅ", "ﾆ", "ﾇ", "ﾈ", "ﾉ", "♠", "♥", "♦", "♣", "★", "☆", "§", "†"
            };

            return matrixChars[_random.Next(matrixChars.Length)];
        }

        #endregion

        #region MatrixRainDrops Class

        /// <summary>
        /// Represents a single column of falling characters in the matrix rain
        /// </summary>
        public class RainDrop
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public int Length { get; set; }
            public List<string> Chars { get; set; }
            public double HeadOpacity { get; set; }
            public double TailOpacity { get; set; }
        }

        #endregion
    }

    #region Drawable Implementation

    /// <summary>
    /// Drawable implementation for the Matrix Rain effect
    /// </summary>
    public class MatrixRainDrawable : IDrawable
    {
        private readonly List<MatrixRain.RainDrop> _rainDrops;
        private readonly MatrixRain _owner;

        public Color CharColor { get; set; } = Colors.LimeGreen;
        public string Message { get; set; } = string.Empty;
        public double Speed { get; set; } = 1.0;

        public MatrixRainDrawable(List<MatrixRain.RainDrop> rainDrops, MatrixRain owner)
        {
            _rainDrops = rainDrops;
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Clear the canvas with black
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            // Draw each raindrop
            foreach (var drop in _rainDrops)
            {
                DrawRaindrop(canvas, drop);
            }

            // Draw message if provided
            if (!string.IsNullOrEmpty(Message))
            {
                DrawMessage(canvas, dirtyRect);
            }
        }

        private void DrawRaindrop(ICanvas canvas, MatrixRain.RainDrop drop)
        {
            // Skip if no characters
            if (drop.Chars == null || drop.Chars.Count == 0) return;

            // Character spacing
            float spacing = 20;

            // Draw characters from top to bottom
            for (int i = 0; i < drop.Chars.Count; i++)
            {
                float y = (float)drop.Y + (i * spacing);

                // Skip if off-screen
                if (y < -spacing || y > _owner.Height) continue;

                // Calculate opacity based on position in the drop
                // Head is brightest, tail fades out
                double opacity;
                if (i == 0)
                {
                    opacity = drop.HeadOpacity;
                }
                else
                {
                    // Linear fade from head to tail
                    double fadeRatio = 1.0 - (i / (double)drop.Chars.Count);
                    opacity = drop.HeadOpacity * fadeRatio + drop.TailOpacity * (1 - fadeRatio);
                }

                // Set drawing color with opacity
                canvas.FontColor = CharColor.WithAlpha((float)opacity);

                // Draw the character
                canvas.Font = new Font("Courier New");
                canvas.FontSize = 16;

                // Draw centered on the drop's X position
                var textSize = canvas.GetStringSize(drop.Chars[i], new Font("Courier New"), 16);
                float x = (float)drop.X - ((float)textSize.Width / 2);

                canvas.DrawString(drop.Chars[i], x, y, HorizontalAlignment.Left);
            }
        }

        private void DrawMessage(ICanvas canvas, RectF dirtyRect)
        {
            // Draw a semi-transparent background for better readability
            canvas.FillColor = Colors.Black.WithAlpha(0.7f);
            canvas.FillRoundedRectangle(
                dirtyRect.X + dirtyRect.Width * 0.1f,
                dirtyRect.Y + dirtyRect.Height * 0.4f,
                dirtyRect.Width * 0.8f,
                dirtyRect.Height * 0.2f,
                10);

            // Draw the message text
            canvas.FontColor = CharColor;
            canvas.Font = new Font("Courier New", FontWeight.Bold);
            canvas.FontSize = 24;

            // Center the text
            canvas.DrawString(
                Message,
                dirtyRect.X + (dirtyRect.Width / 2),
                dirtyRect.Y + (dirtyRect.Height / 2),
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    #endregion
}