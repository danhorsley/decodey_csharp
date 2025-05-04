using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Decodey.Converters
{
    /// <summary>
    /// Converter that checks if an encrypted letter is selected
    /// </summary>
    public class LetterStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle the case where we're checking if a letter is selected
            if (value is string letter && parameter is string selectedLetter)
            {
                return letter == selectedLetter;
            }

            // Handle the case where we're checking if a letter is guessed
            if (value is string letterToCheck && parameter is IEnumerable<string> correctlyGuessed)
            {
                return correctlyGuessed.Contains(letterToCheck);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for frequency of a letter in the text
    /// </summary>
    public class FrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string letter && parameter is Decodey.ViewModels.GameViewModel viewModel)
            {
                return viewModel.GetFrequency(letter[0]);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter that checks if a guess letter has been incorrectly guessed for a selected encrypted letter
    /// </summary>
    public class GuessStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string letter && parameter is Decodey.ViewModels.GameViewModel viewModel)
            {
                // Check if this is an incorrect guess for the currently selected letter
                if (!string.IsNullOrEmpty(viewModel.SelectedEncryptedLetter))
                {
                    return viewModel.IsIncorrectGuess(viewModel.SelectedEncryptedLetter, letter);
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for remaining mistakes
    /// </summary>
    public class RemainingMistakesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int mistakes && parameter is int maxMistakes)
            {
                return string.Format("{0}/{1}", mistakes, maxMistakes);
            }

            return "0/0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}