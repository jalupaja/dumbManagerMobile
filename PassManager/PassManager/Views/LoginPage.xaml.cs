using PassManager.Models;
using PassManager.ViewModels;
using Password_Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PassManager.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            FileName_TextChanged(null, null);
        }

        private void FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filepath = Path.Combine(Constants.basePath, Constants.HashIt(FileName.Text) + ".db");

            if (File.Exists(filepath))
            {
                SelFile.Text = $"login to {FileName.Text}";
                FilePass.Placeholder = "password";
                PassConfirm.IsVisible = false;
                SelFile.IsEnabled = true;
                if (FilePass.IsVisible)
                {
                    LblLogin.Text = "Please input the password to your account!";
                    LblLogin.TextColor = Color.White;
                }
            }
            else
            {
                if (FilePass.Text != PassConfirm.Text)
                    SelFile.IsEnabled = false;
                PassConfirm.IsVisible = true;
                SelFile.Text = "create new user";
                FilePass.Placeholder = "new password";
                if (FilePass.IsVisible)
                {
                    LblLogin.Text = "Please create a secure password \nThe password can not be recovered!";
                    LblLogin.TextColor = Color.White;
                    FilePass.Placeholder = "new password";
                }
            }
        }
        private void FilePass_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filepath = Path.Combine(Constants.basePath, Constants.HashIt(FileName.Text) + ".db");

            if (File.Exists(filepath))
            {
                SelFile.IsEnabled = true;
            }
            else
            {
                if (FilePass.Text == PassConfirm.Text)
                    SelFile.IsEnabled = true;
                else
                    SelFile.IsEnabled = false;
            }
        }
        private void PassConfirm_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filepath = Path.Combine(Constants.basePath, Constants.HashIt(FileName.Text) + ".db");

            if (FilePass.Text == PassConfirm.Text || File.Exists(filepath))
                SelFile.IsEnabled = true;
            else
                SelFile.IsEnabled = false;
        }

        private async void SelFile_Clicked(object sender, EventArgs e)
        {
            if (Constants.TryLogin(FileName.Text, FilePass.Text))
            {
                await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
                FilePass.Text = "";
                PassConfirm.Text = "";
                SelFile.Text = $"login to {FileName.Text}";
                FilePass.Placeholder = "password";
                LblLogin.Text = "Please input the password to your account!";
                LblLogin.TextColor = Color.White;
            }
            else
            {
                FilePass.Text = "";
                PassConfirm.Text = "";
                LblLogin.Text = "Incorrect password.";
                LblLogin.TextColor = Color.Red;
            }
        }
    }
}