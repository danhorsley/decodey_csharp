using Decodey.ViewModels;

namespace Decodey.Views
{
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;

        public GamePage(GameViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // TODO: Implement slide menu or navigation
            await DisplayAlert("Menu", "Menu functionality will be implemented later", "OK");
        }

        private async void OnAboutButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AboutDialog());
        }
    }
}