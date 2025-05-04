// Controls/MatrixRain.xaml.cs
using Microsoft.Maui.Controls;
using System;

namespace Decodey.Controls
{
    public partial class MatrixRain : ContentView
    {
        public MatrixRain()
        {
            InitializeComponent();
            // Initialize your matrix rain here
        }

        // Move all your MatrixRain-specific methods here
        // Make sure to rename them if they're conflicting with MainPage methods

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            // Your size changed logic
        }

        private void UpdateLayoutForScreenSize()
        {
            // Your layout updating logic
        }

        // Continue with the rest of your methods, ensuring they're properly
        // refactored to work within a ContentView instead of MainPage
    }
}