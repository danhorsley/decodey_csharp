using System.Globalization;
using Decodey.ViewModels;

namespace Decodey.Converters
{
    public class LetterStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string letter || parameter is not GameViewModel viewModel)
                return Colors.Gray;

            // If the letter is selected
            if (letter == viewModel.SelectedEncryptedLetter)
                return Color.FromArgb("#FF5C00"); // Selected orange

            // If the letter is correctly guessed
            if (viewModel.CorrectlyGuessed.Contains(letter))
                return Color.FromArgb("#22BB33"); // Success green

            // If the letter was the last correct guess
            if (letter == viewModel.LastCorrectGuess)
                return Color.FromArgb("#BB33FF"); // Flash purple

            // Default state
            return Color.FromArgb("#007BFF"); // Default blue
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GuessStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string letter || parameter is not GameViewModel viewModel)
                return Colors.Gray;

            // If the letter is already guessed
            if (viewModel.GuessedMappings.Any(m => m.Value == letter))
                return Color.FromArgb("#22BB33"); // Success green

            // If this is an incorrect guess for the currently selected letter
            if (!string.IsNullOrEmpty(viewModel.SelectedEncryptedLetter) &&
                viewModel.IncorrectGuesses.TryGetValue(viewModel.SelectedEncryptedLetter, out var incorrectList) &&
                incorrectList.Contains(letter))
                return Color.FromArgb("#BB2124"); // Error red

            // Default state
            return Color.FromArgb("#6C757D"); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string letter || parameter is not GameViewModel viewModel)
                return "";

            if (viewModel.LetterFrequency.TryGetValue(letter, out int frequency))
                return frequency.ToString();

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RemainingMistakesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int mistakes || parameter is not int maxMistakes)
                return "HINT";

            int remaining = maxMistakes - mistakes;
            return $"HINT TOKENS: {remaining}/{maxMistakes}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}