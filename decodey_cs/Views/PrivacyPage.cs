using Microsoft.Maui.Controls;
using System;

namespace Decodey.Views
{
    public class PrivacyPage : ContentPage
    {
        public PrivacyPage()
        {
            Title = "Privacy Policy";

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
                            Text = "Privacy Policy",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        new Label
                        {
                            Text = "Last Updated: May 1, 2024",
                            FontSize = 14,
                            Opacity = 0.7,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        CreatePrivacySection("Information We Collect",
                            "We collect minimal personal information to provide and improve our word puzzle game. This includes:\n\n" +
                            "• Account information (username, email)\n" +
                            "• Game statistics and progress\n" +
                            "• Device information for diagnostics"),

                        CreatePrivacySection("How We Use Your Information",
                            "We use the information we collect to:\n\n" +
                            "• Provide and maintain the game\n" +
                            "• Improve user experience\n" +
                            "• Enable features like leaderboards and saved progress\n" +
                            "• Diagnose technical issues"),

                        CreatePrivacySection("Data Storage",
                            "Your account data is stored securely on our servers. Game progress is stored locally on your device and optionally synchronized with our servers if you create an account."),

                        CreatePrivacySection("Third-Party Services",
                            "We may use third-party services for analytics and diagnostics. These services collect anonymous usage data to help us improve the game."),

                        CreatePrivacySection("Your Rights",
                            "You have the right to:\n\n" +
                            "• Access your personal data\n" +
                            "• Request correction of inaccurate data\n" +
                            "• Request deletion of your account\n" +
                            "• Opt out of promotional communications"),

                        CreatePrivacySection("Contact Us",
                            "If you have questions about our privacy practices, please contact us at:\n\n" +
                            "privacy@decodey.com"),

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

        private Frame CreatePrivacySection(string title, string content)
        {
            return new Frame
            {
                BorderColor = Colors.LightGray,
                CornerRadius = 6,
                Padding = new Thickness(15),
                Margin = new Thickness(0, 5, 0, 10),
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 18
                        },
                        new Label
                        {
                            Text = content,
                            FontSize = 14
                        }
                    }
                }
            };
        }
    }
}