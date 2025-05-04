using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Decodey.Services;
using Decodey.Views;

namespace Decodey.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _rememberMe = true;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your username and password";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var result = await _authService.Login(Username, Password, RememberMe);

                if (result.Success)
                {
                    // Store preferences
                    Preferences.Set("uncrypt-remember-me", RememberMe.ToString().ToLower());

                    // Close login page and return to game
                    await _navigationService.GoBack();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Login failed. Please check your credentials.";
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
        private async Task ForgotPassword()
        {
            string email = await Application.Current.MainPage.DisplayPromptAsync(
                "Forgot Password",
                "Please enter your email address:",
                "Submit",
                "Cancel");

            if (string.IsNullOrWhiteSpace(email))
                return;

            try
            {
                IsBusy = true;
                HasError = false;

                var result = await _authService.ForgotPassword(email);

                await Application.Current.MainPage.DisplayAlert(
                    "Password Reset",
                    result.Message ?? "If an account exists with this email, a reset link will be sent.",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to process password reset request. Please try again later.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateAccount()
        {
            await _navigationService.NavigateTo<SignupDialog>();
        }

        [RelayCommand]
        private async Task Close()
        {
            await _navigationService.GoBack();
        }

        public string LoginButtonText => IsBusy ? "Logging in..." : "Login";

        public bool IsNotBusy => !IsBusy;
    }
}