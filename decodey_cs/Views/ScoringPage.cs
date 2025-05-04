using Microsoft.Maui.Controls;
using System;

namespace Decodey.Views
{
    public class ScoringPage : ContentPage
    {
        public ScoringPage()
        {
            Title = "Scoring System";

            // Create UI
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    Padding = new Thickness(20),
                    Children =
                    {
                        new Label
                        {
                            Text = "Scoring System",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        new Label
                        {
                            Text = "Points are calculated based on the following factors:",
                            FontSize = 16
                        },
                        CreateScoringFactorView("Speed", "The faster you solve the puzzle, the more points you earn"),
                        CreateScoringFactorView("Mistakes", "Fewer mistakes means more points"),
                        CreateScoringFactorView("Difficulty", "Higher difficulty settings yield more points"),
                        CreateScoringFactorView("Quote Length", "Longer quotes provide more potential points"),
                        CreateScoringFactorView("Hardcore Mode", "Playing with spaces and punctuation hidden gives a 2x multiplier"),
                        new Label
                        {
                            Text = "Formula",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            Margin = new Thickness(0, 20, 0, 10)
                        },
                        new Frame
                        {
                            BorderColor = Colors.LightGray,
                            CornerRadius = 8,
                            Padding = new Thickness(15),
                            Content = new Label
                            {
                                Text = "Base Score = (Letters in Quote × Difficulty Multiplier)\n\nSpeed Bonus = Base Score × (1 + (Max Time - Your Time) ÷ Max Time)\n\nMistakes Penalty = Base Score × (1 - Mistakes ÷ Max Mistakes)\n\nFinal Score = (Base Score + Speed Bonus - Mistakes Penalty) × Mode Multiplier",
                                FontFamily = "Courier New"
                            }
                        },
                        new Button
                        {
                            Text = "Back",
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 20, 0, 0),
                            Command = new Command(async () => await Navigation.PopAsync())
                        }
                    }
                }
            };
        }

        private Frame CreateScoringFactorView(string title, string description)
        {
            return new Frame
            {
                BorderColor = Colors.LightGray,
                CornerRadius = 6,
                Padding = new Thickness(12),
                Margin = new Thickness(0, 5),
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 16
                        },
                        new Label
                        {
                            Text = description,
                            FontSize = 14,
                            Opacity = 0.8
                        }
                    }
                }
            };
        }
    }
}