using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Decodey.ViewModels;

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
            // If authentication state changed
            if (e.PropertyName == nameof(SignupViewModel.IsAuthenticated) && _viewModel.IsAuthenticated)
            {
                // Set result to true
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
            _resultCompletionSource.TrySetResult(_viewModel.IsAuthenticated);
        }

        public Task<bool> GetResultAsync()
        {
            return _resultCompletionSource.Task;
        }
    }
}