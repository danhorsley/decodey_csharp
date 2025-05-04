using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Decodey.ViewModels;
using Decodey.Services;

namespace Decodey.Views.Dialogs
{
    public partial class SignupDialog : ContentPage
    {
        private readonly SignupViewModel _viewModel;
        private TaskCompletionSource<bool> _resultCompletionSource;

        public SignupDialog()
        {
            InitializeComponent();

            // Get view model
            _viewModel = ServiceProvider.GetService<SignupViewModel>() ?? new SignupViewModel(
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
            // Check for successful signup
            if (e.PropertyName == "HasError" && !_viewModel.HasError && _viewModel.IsNotBusy)
            {
                // Assume successful signup if no error and not busy
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
            // Determine result based on whether user signed up
            _resultCompletionSource.TrySetResult(!_viewModel.HasError && _viewModel.IsNotBusy);
        }

        public Task<bool> GetResultAsync()
        {
            return _resultCompletionSource.Task;
        }
    }
}