using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Decodey.Helpers
{
    /// <summary>
    /// Extension methods for View elements
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Gets the absolute bounds of a view within the application window
        /// </summary>
        public static Rect GetAbsoluteBounds(this View view)
        {
            // Start with the view's relative bounds
            var x = view.X;
            var y = view.Y;
            var width = view.Width;
            var height = view.Height;

            // Traverse up the visual tree to get absolute coordinates
            var parent = view.Parent as View;
            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;

                // Account for padding if the parent is a Layout
                if (parent is Layout layout)
                {
                    x += layout.Padding.Left;
                    y += layout.Padding.Top;
                }

                parent = parent.Parent as View;
            }

            return new Rect(x, y, width, height);
        }
    }

    /// <summary>
    /// Helper class for status bar management (stub implementation)
    /// </summary>
    public static class StatusBar
    {
        /// <summary>
        /// Set status bar background color - stub implementation
        /// </summary>
        public static void SetColor(Color color)
        {
            // Implementation removed due to missing API
            Console.WriteLine($"StatusBar.SetColor called with color {color}");
        }

        /// <summary>
        /// Set status bar style (light or dark content) - stub implementation
        /// </summary>
        public static void SetStyle(StatusBarStyle style)
        {
            // Implementation removed due to missing API
            Console.WriteLine($"StatusBar.SetStyle called with style {style}");
        }
    }

    /// <summary>
    /// Status bar style options
    /// </summary>
    public enum StatusBarStyle
    {
        /// <summary>
        /// Light content for dark backgrounds
        /// </summary>
        LightContent,

        /// <summary>
        /// Dark content for light backgrounds
        /// </summary>
        DarkContent
    }
}