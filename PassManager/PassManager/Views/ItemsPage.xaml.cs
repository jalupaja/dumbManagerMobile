using PassManager.Models;
using PassManager.ViewModels;
using Password_Manager;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PassManager.Views
{
    public partial class ItemsPage : ContentPage
    {
        readonly ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
            ItemsListView.ItemsSource = _viewModel.Items.OrderByDescending(o => o.Id);
            MySBar.Text = "";
            if (_viewModel.Items.Count == 0) emptyLabel.IsVisible = true;
            else emptyLabel.IsVisible = false;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBar = _viewModel.Items.Where(c => c.Name.ToLower().Contains(e.NewTextValue.ToLower()));
            ItemsListView.ItemsSource = searchBar;
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            Constants.pw = "";
            Constants.name = "";
            Constants.con().Close();
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }

}