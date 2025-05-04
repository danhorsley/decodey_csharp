using Decodey.ViewModels;
using Decodey.Services;
using Decodey.Views.Dialogs;

namespace Decodey.Views
{
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly IDialogService _dialogService;

        public GamePage(GameViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Get dialog service
            _dialogService = ServiceProvider.GetService<IDialogService>();
        }

        private async void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // TODO: Implement slide menu or navigation
            await DisplayAlert("Menu", "Menu functionality will be implemented later", "OK");
        }

        private async void OnAboutButtonClicked(object sender, EventArgs e)
        {
            // Show the about dialog using the dialog service
            if (_dialogService != null)
            {
                await _dialogService.ShowAboutDialog();
            }
            else
            {
                // Fallback if service is not available
                var aboutDialog = new AboutDialog();
                await Navigation.PushModalAsync(aboutDialog);
            }
        }
    }