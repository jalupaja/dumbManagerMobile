using Password_Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PassManager.Views
{
    public partial class PassGenPage : ContentPage
    {
        public PassGenPage()
        {
            InitializeComponent();
            CreatePass_Clicked(null, null);
        }

        private async void CopyPass_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(PasswordOut.Text);
        }

        private void CreatePass_Clicked(object sender, EventArgs e)
        {
            PassLength.HorizontalOptions = LayoutOptions.Fill;
            PassLength.HorizontalOptions = LayoutOptions.StartAndExpand;

            int passlength = 20;
            try
            {
                passlength = Convert.ToInt32(PassLength.Text);
            }
            catch (Exception)
            {
                if (PassLength.Text != "")
                {
                    PassLength.Text = "20";
                }
                return;

            }

            string valid = "";
            if (Lowercase.IsChecked)
            {
                valid += "abcdefghijklmnopqrstuvwxyz";
            }
            if (Uppercase.IsChecked)
            {
                valid += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }
            if (Numbers.IsChecked)
            {
                valid += "1234567890";
            }
            if (SpecChar.IsChecked)
            {
                valid += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            }

            if (valid == "")
            {
                LblPwdCreate.Text = "You have to select at least one checkmark!";
                LblPwdCreate.TextColor = Color.Red;
            }
            else
            {
                LblPwdCreate.Text = "Create a new Password";
                LblPwdCreate.TextColor = Color.White;

                string pwOut = "";
                Random rnd = new Random();
                for (int i = 0; i < passlength; i++)
                {
                    pwOut += valid[rnd.Next(valid.Length)];
                }

                PasswordOut.Text = pwOut;
            }
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            Constants.retNPass = "";
            await Shell.Current.GoToAsync("..");
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {

            Constants.retNPass = PasswordOut.Text;
            await Shell.Current.GoToAsync("..");
        }
    }
}