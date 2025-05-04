using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for animation service
    /// </summary>
    public interface IAnimationService
    {
        /// <summary>
        /// Animate a correct guess on a letter cell
        /// </summary>
        Task AnimateCorrectGuessAsync(VisualElement element);

        /// <summary>
        /// Animate an incorrect guess on a letter cell
        /// </summary>
        Task AnimateIncorrectGuessAsync(VisualElement element);

        /// <summary>
        /// Create and run a flash animation
        /// </summary>
        Task FlashElementAsync(VisualElement element, Color flashColor, uint duration = 500);

        /// <summary>
        /// Create and run a shake animation
        /// </summary>
        Task ShakeElementAsync(VisualElement element, uint duration = 500);

        /// <summary>
        /// Create and run a pulse animation
        /// </summary>
        Task PulseElementAsync(VisualElement element, uint duration = 500);

        /// <summary>
        /// Create and run a fade animation
        /// </summary>
        Task FadeElementAsync(VisualElement element, double targetOpacity, uint duration = 500);

        /// <summary>
        /// Create and run a scale animation
        /// </summary>
        Task ScaleElementAsync(VisualElement element, double targetScale, uint duration = 500);

        /// <summary>
        /// Create and run a slide animation
        /// </summary>
        Task SlideElementAsync(VisualElement element, double targetX, double targetY, uint duration = 500);
    }

    /// <summary>
    /// Service for handling animations in the UI
    /// </summary>
    public class AnimationService : IAnimationService
    {
        /// <summary>
        /// Animate a correct guess on a letter cell
        /// </summary>
        public async Task AnimateCorrectGuessAsync(VisualElement element)
        {
            if (element == null) return;

            // Store original background color
            var originalBackgroundColor = element.BackgroundColor;

            // Flash animation
            await FlashElementAsync(element, Colors.Green);

            // Return to original color with a guessed style
            if (Application.Current.RequestedTheme == AppTheme.Dark)
            {
                element.BackgroundColor = Color.FromArgb("#222222");
            }
            else
            {
                element.BackgroundColor = Color.FromArgb("#e9ecef");
            }
        }

        /// <summary>
        /// Animate an incorrect guess on a letter cell
        /// </summary>
        public async Task AnimateIncorrectGuessAsync(VisualElement element)
        {
            if (element == null) return;

            // Store original background color
            var originalBackgroundColor = element.BackgroundColor;

            // Shake animation
            var shakeTask = ShakeElementAsync(element);

            // Flash animation (red)
            await FlashElementAsync(element, Colors.Red);

            // Ensure shake completes
            await shakeTask;

            // Return to original color
            element.BackgroundColor = originalBackgroundColor;
        }

        /// <summary>
        /// Create and run a flash animation
        /// </summary>
        public async Task FlashElementAsync(VisualElement element, Color flashColor, uint duration = 500)
        {
            if (element == null) return;

            // Store original background color
            var originalBackgroundColor = element.BackgroundColor;

            // Create animation
            var flashAnimation = new Animation(v => element.BackgroundColor = new Color(
                flashColor.Red * v + originalBackgroundColor.Red * (1 - v),
                flashColor.Green * v + originalBackgroundColor.Green * (1 - v),
                flashColor.Blue * v + originalBackgroundColor.Blue * (1 - v),
                flashColor.Alpha * v + originalBackgroundColor.Alpha * (1 - v)
            ));

            // Add keyframes
            flashAnimation.Add(0, 0.5, new Animation(v => element.BackgroundColor = new Color(
                flashColor.Red * v + originalBackgroundColor.Red * (1 - v),
                flashColor.Green * v + originalBackgroundColor.Green * (1 - v),
                flashColor.Blue * v + originalBackgroundColor.Blue * (1 - v),
                flashColor.Alpha * v + originalBackgroundColor.Alpha * (1 - v)
            )));

            flashAnimation.Add(0.5, 1, new Animation(v => element.BackgroundColor = new Color(
                flashColor.Red * (1 - v) + originalBackgroundColor.Red * v,
                flashColor.Green * (1 - v) + originalBackgroundColor.Green * v,
                flashColor.Blue * (1 - v) + originalBackgroundColor.Blue * v,
                flashColor.Alpha * (1 - v) + originalBackgroundColor.Alpha * v
            )));

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            flashAnimation.Commit(element, "FlashAnimation", length: duration,
                finished: (v, c) => tcs.SetResult(true));

            // Wait for animation to complete
            await tcs.Task;
        }

        /// <summary>
        /// Create and run a shake animation
        /// </summary>
        public async Task ShakeElementAsync(VisualElement element, uint duration = 500)
        {
            if (element == null) return;

            // Store original translation
            var originalTranslationX = element.TranslationX;

            // Create animation
            var shakeAnimation = new Animation();

            // Add keyframes
            shakeAnimation.Add(0.0, 0.2, new Animation(v => element.TranslationX = originalTranslationX - 10 * v));
            shakeAnimation.Add(0.2, 0.4, new Animation(v => element.TranslationX = originalTranslationX - 10 * (1 - v) + 10 * v));
            shakeAnimation.Add(0.4, 0.6, new Animation(v => element.TranslationX = originalTranslationX + 10 * (1 - v) - 5 * v));
            shakeAnimation.Add(0.6, 0.8, new Animation(v => element.TranslationX = originalTranslationX - 5 * (1 - v) + 5 * v));
            shakeAnimation.Add(0.8, 1.0, new Animation(v => element.TranslationX = originalTranslationX + 5 * (1 - v)));

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            shakeAnimation.Commit(element, "ShakeAnimation", length: duration,
                finished: (v, c) =>
                {
                    element.TranslationX = originalTranslationX;
                    tcs.SetResult(true);
                });

            // Wait for animation to complete
            await tcs.Task;
        }

        /// <summary>
        /// Create and run a pulse animation
        /// </summary>
        public async Task PulseElementAsync(VisualElement element, uint duration = 500)
        {
            if (element == null) return;

            // Store original scale
            var originalScaleX = element.ScaleX;
            var originalScaleY = element.ScaleY;

            // Create animation
            var pulseAnimation = new Animation();

            // Add keyframes
            pulseAnimation.Add(0.0, 0.5, new Animation(v =>
            {
                element.ScaleX = originalScaleX * (1 + 0.2 * v);
                element.ScaleY = originalScaleY * (1 + 0.2 * v);
            }));

            pulseAnimation.Add(0.5, 1.0, new Animation(v =>
            {
                element.ScaleX = originalScaleX * (1.2 - 0.2 * v);
                element.ScaleY = originalScaleY * (1.2 - 0.2 * v);
            }));

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            pulseAnimation.Commit(element, "PulseAnimation", length: duration,
                finished: (v, c) =>
                {
                    element.ScaleX = originalScaleX;
                    element.ScaleY = originalScaleY;
                    tcs.SetResult(true);
                });

            // Wait for animation to complete
            await tcs.Task;
        }

        /// <summary>
        /// Create and run a fade animation
        /// </summary>
        public async Task FadeElementAsync(VisualElement element, double targetOpacity, uint duration = 500)
        {
            if (element == null) return;

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            element.FadeTo(targetOpacity, duration, Easing.CubicInOut)
                .ContinueWith(_ => tcs.SetResult(true), TaskScheduler.FromCurrentSynchronizationContext());

            // Wait for animation to complete
            await tcs.Task;
        }

        /// <summary>
        /// Create and run a scale animation
        /// </summary>
        public async Task ScaleElementAsync(VisualElement element, double targetScale, uint duration = 500)
        {
            if (element == null) return;

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            element.ScaleTo(targetScale, duration, Easing.CubicInOut)
                .ContinueWith(_ => tcs.SetResult(true), TaskScheduler.FromCurrentSynchronizationContext());

            // Wait for animation to complete
            await tcs.Task;
        }

        /// <summary>
        /// Create and run a slide animation
        /// </summary>
        public async Task SlideElementAsync(VisualElement element, double targetX, double targetY, uint duration = 500)
        {
            if (element == null) return;

            // Create a task completion source
            var tcs = new TaskCompletionSource<bool>();

            // Start animation
            element.TranslateTo(targetX, targetY, duration, Easing.CubicInOut)
                .ContinueWith(_ => tcs.SetResult(true), TaskScheduler.FromCurrentSynchronizationContext());

            // Wait for animation to complete
            await tcs.Task;
        }
    }
}