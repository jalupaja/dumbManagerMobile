using PassManager.Models;
using PassManager.ViewModels;
using Password_Manager;
using System;
using Xamarin.Forms;

namespace PassManager.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Constants.retNPass != "" && Constants.retNPass != null)
            {
                lPassword.Text = Constants.retNPass;
                Constants.retNPass = "";
            }
        }
        private async void NewPassword_Clicked(object sender, EventArgs e)
        {
            if (!(lPassword.Text == null || lPassword.Text == "" || lPassword.Text == string.Empty))
            {
                if (!await App.Current.MainPage.DisplayAlert("overwrite password", "Do you want to overwrite the existing Password?", "Yes", "No"))
                    return;
            }
            await Shell.Current.GoToAsync(nameof(PassGenPage));
        }
    }
}