using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Decodey.Services;
using Decodey.Views;
using System.Text.RegularExpressions;

namespace Decodey.ViewModels
{
    public partial class SignupViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private CancellationTokenSource _usernameCheckCts;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private bool _emailConsent;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _usernameStatus = "";

        [ObservableProperty]
        private bool _isUsernameChecking;

        [ObservableProperty]
        private bool _isUsernameAvailable;

        public SignupViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            // Set up username property changed to check availability
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Username))
                {
                    CheckUsername();
                }
            };
        }

        [RelayCommand]
        private async Task Signup()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Please fill in all fields";
                HasError = true;
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address";
                HasError = true;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                HasError = true;
                return;
            }

            if (Username.Length < 3)
            {
                ErrorMessage = "Username must be at least 3 characters";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var result = await _authService.Signup(new SignupRequest
                {
                    Email = Email,
                    Username = Username,
                    Password = Password,
                    EmailConsent = EmailConsent
                });

                if (result.Success)
                {
                    // Try to auto-login after signup
                    try
                    {
                        var loginResult = await _authService.Login(Username, Password, true);

                        if (loginResult.Success)
                        {
                            // Show welcome toast
                            ToastService.ShowToast("Welcome! Your account has been created.");

                            // Close signup page and return to game
                            await _navigationService.GoBack();
                        }
                        else
                        {
                            // Account created but couldn't auto-login
                            await Application.Current.MainPage.DisplayAlert(
                                "Account Created",
                                "Your account was created successfully! Please log in.",
                                "OK");

                            // Navigate back to login page
                            await _navigationService.NavigateTo<LoginDialog>();
                        }
                    }
                    catch
                    {
                        // If auto-login fails, just show success message
                        await Application.Current.MainPage.DisplayAlert(
                            "Account Created",
                            "Your account was created successfully! Please log in.",
                            "OK");

                        // Navigate back to login page
                        await _navigationService.NavigateTo<LoginDialog>();
                    }
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Failed to create account. Please try again.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task BackToLogin()
        {
            await _navigationService.NavigateTo<LoginDialog>();
        }

        [RelayCommand]
        private async Task Close()
        {
            await _navigationService.GoBack();
        }

        private bool IsValidEmail(string email)
        {
            // Simple regex to validate email format
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private async void CheckUsername()
        {
            // Cancel any previous check
            _usernameCheckCts?.Cancel();
            _usernameCheckCts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
            {
                UsernameStatus = Username.Length > 0 ? "Username must be at least 3 characters" : "";
                IsUsernameAvailable = false;
                return;
            }

            IsUsernameChecking = true;
            UsernameStatus = "Checking...";

            try
            {
                // Add a small delay to avoid too many API calls
                await Task.Delay(500, _usernameCheckCts.Token);

                var result = await _authService.CheckUsernameAvailability(Username);

                if (_usernameCheckCts.Token.IsCancellationRequested)
                    return;

                IsUsernameAvailable = result.Available;
                UsernameStatus = result.Available ? "Username available!" : "Username already taken";
            }
            catch (Exception)
            {
                UsernameStatus = "Error checking username";
                IsUsernameAvailable = false;
            }
            finally
            {
                IsUsernameChecking = false;
            }
        }

        public Color UsernameColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UsernameStatus))
                    return Application.Current.Resources["PrimaryTextColor"] as Color;

                return IsUsernameAvailable
                    ? Colors.Green
                    : Colors.Red;
            }
        }

        public string SignupButtonText => IsBusy ? "Creating Account..." : "Create Account";

        public bool IsNotBusy => !IsBusy;
    }
}