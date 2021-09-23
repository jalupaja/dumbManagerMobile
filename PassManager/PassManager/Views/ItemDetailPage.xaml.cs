using PassManager.ViewModels;
using System;
using TwoStepsAuthenticator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PassManager.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public string Name { get; set; }
        private string oldName;
        private string oldUsername;
        private string oldPassword;
        private string oldUrl;
        private string oldTwoFA;
        private string oldNote;

        readonly ItemDetailViewModel _viewModel;

        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.Url != null && !Equals("", _viewModel.Url)) LinkFrame.IsVisible = true;
            else LinkFrame.IsVisible = false;
            lTwoFA.Text = Get2FACode();
        }

        [Obsolete]
        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            try 
            {
                Device.OpenUri(new Uri(lUrl.Text));
            }
            catch (Exception){}

        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            OnAlertYesNoClicked(sender, e);
            
        }

        async void OnAlertYesNoClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("", "Do you want to delete the item?", "Del", "Cancel");
            if(answer) _viewModel.DeleteItemCommand();
        }

        void OnClipboardContentChanged( string dataName)
        {
            DisplayAlert("", dataName + " has been copied!", "", "OK");
        }

        private void ToolbarItem_Clicked_1(object sender, EventArgs e)
        {
            lName.IsVisible = false;
            oldName = lName.Text;
            NameEditor.IsVisible = true;

            lUsername.IsVisible = false;
            oldUsername = lUsername.Text;
            UsernameEditor.IsVisible = true;

            lPassword.IsVisible = false;
            oldPassword = lPassword.Text;
            PasswordEditor.IsVisible = true;

            lUrl.IsVisible = false;
            oldUrl = lUrl.Text;
            UrlEditor.IsVisible = true;

            lTwoFA.IsVisible = false;
            oldTwoFA = lTwoFA.Text;
            TwoFAEditor.IsVisible = true;

            lNote.IsVisible = false;
            oldNote = lNote.Text;
            NoteEditor.IsVisible = true;

            UsernameCopy.IsVisible = false;
            PasswordCopy.IsVisible = false;
            TwoFACopy.IsVisible = false;

            ChangeBtn.IsEnabled = false;
            BtnLayout.IsVisible = true;
            SaveBtn.IsEnabled = false;

            LinkFrame.IsVisible = true;
        }

        private void CancelBtn_Clicked(object sender, EventArgs e)
        {

            lName.IsVisible = true;
            _viewModel.Name = oldName;
            NameEditor.IsVisible = false;

            lUsername.IsVisible = true;
            _viewModel.Username = oldUsername;
            UsernameEditor.IsVisible = false;

            lPassword.IsVisible = true;
            _viewModel.Password = oldPassword;
            PasswordEditor.IsVisible = false;

            lUrl.IsVisible = true;
            _viewModel.Url = oldUrl;
            UrlEditor.IsVisible = false;

            lTwoFA.IsVisible = true;
            _viewModel.TwoFA = oldTwoFA;
            TwoFAEditor.IsVisible = false;

            lNote.IsVisible = true;
            _viewModel.Note = oldNote;
            NoteEditor.IsVisible = false;

            UsernameCopy.IsVisible = true;
            PasswordCopy.IsVisible = true;
            TwoFACopy.IsVisible = true;

            ChangeBtn.IsEnabled = true;
            BtnLayout.IsVisible = false;

            if (_viewModel.Url != null && !Equals("", _viewModel.Url)) LinkFrame.IsVisible = true;
            else LinkFrame.IsVisible = false;
        }

        private void SaveBtn_Clicked(object sender, EventArgs e)
        {
            _viewModel.Updateİnfo();

            lName.IsVisible = true;
            NameEditor.IsVisible = false;

            lUsername.IsVisible = true;
            UsernameEditor.IsVisible = false;

            lPassword.IsVisible = true;
            PasswordEditor.IsVisible = false;

            lUrl.IsVisible = true;
            UrlEditor.IsVisible = false;

            lTwoFA.IsVisible = true;
            TwoFAEditor.IsVisible = false;

            lNote.IsVisible = true;
            NoteEditor.IsVisible = false;

            UsernameCopy.IsVisible = true;
            PasswordCopy.IsVisible = true;
            TwoFACopy.IsVisible = true;

            ChangeBtn.IsEnabled = true;
            BtnLayout.IsVisible = false;

            if (_viewModel.Url != null && !Equals("", _viewModel.Url)) LinkFrame.IsVisible = true;
            else LinkFrame.IsVisible = false;
        }

        private void Editors_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!Equals(_viewModel.Name, oldName) || !Equals(_viewModel.Username, oldUsername)
                    || !Equals(_viewModel.Password, oldPassword) || !Equals(_viewModel.Url, oldUrl)
                    || !Equals(_viewModel.TwoFA, oldTwoFA) || !Equals(_viewModel.Note, oldNote)) SaveBtn.IsEnabled = true;
            else SaveBtn.IsEnabled = false;

        }

        private void UsernameCopy_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetTextAsync(lUsername.Text);
            OnClipboardContentChanged("Username");
        }

        private void PasswordCopy_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetTextAsync(lPassword.Text);
            OnClipboardContentChanged("Password");
        }

        private void TwoFACopy_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetTextAsync(Get2FACode());
            OnClipboardContentChanged("2FA Code");
        }
        private string Get2FACode()
        {
            var ret = "";

            try { ret = new TimeAuthenticator().GetCode(_viewModel.TwoFA); } catch (Exception) { }//https://github.com/glacasa/TwoStepsAuthenticator
            return ret;
        }
    }
}