using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Decodey.ViewModels;
using Decodey.Services; // Add this for INavigationService

namespace Decodey.Views.Dialogs
{
    public partial class LoginDialog : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        private TaskCompletionSource<bool> _resultCompletionSource;

        public LoginDialog()
        {
            InitializeComponent();

            // Get view model
            _viewModel = ServiceProvider.GetService<LoginViewModel>() ?? new LoginViewModel(
                ServiceProvider.GetService<IAuthService>(),
                ServiceProvider.GetService<INavigationService>());

            BindingContext = _viewModel;

            // Initialize result completion source
            _resultCompletionSource = new TaskCompletionSource<bool>();

            // Subscribe to view model events
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Check for successful login
            if (e.PropertyName == "HasError" && !_viewModel.HasError && _viewModel.IsNotBusy)
            {
                // Assume successful login if no error and not busy
                _resultCompletionSource.TrySetResult(true);

                // Close dialog
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Make sure we complete the task
            // Determine result based on whether user logged in
            _resultCompletionSource.TrySetResult(!_viewModel.HasError && _viewModel.IsNotBusy);
        }

        public Task<bool> GetResultAsync()
        {
            return _resultCompletionSource.Task;
        }
    }
}