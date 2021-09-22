using PassManager.Models;
using System;
using Xamarin.Forms;
using SQLite;
using Password_Manager;

namespace PassManager.ViewModels
{
    public class NewItemViewModel : BaseViewModel
    {
        private string name;
        private string username;
        private string password;
        private string url;
        private string twoFA;
        private string note;

        public NewItemViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(name)
                || !String.IsNullOrWhiteSpace(username)
                || !String.IsNullOrWhiteSpace(password)
                || !String.IsNullOrWhiteSpace(url) 
                || !String.IsNullOrWhiteSpace(twoFA)
                || !String.IsNullOrWhiteSpace(note);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string Url
        {
            get => url;
            set => SetProperty(ref url, value);
        }

        public string TwoFA
        {
            get => twoFA;
            set => SetProperty(ref twoFA, value);
        }

        public string Note
        {
            get => note;
            set => SetProperty(ref note, value);
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            Item newItem = new Item()
            {
                Name = Name,
                Username = Username,
                Password = Password,
                Url = Url,
                TwoFA = TwoFA,
                Note = Note
            };
            Constants.con().Insert(newItem);

            var result = Constants.con().Table<Item>().ToList();
            foreach (var item in result)
            {
                newItem.Id = item.Id;
            }

            await DataStore.AddItemAsync(newItem);

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}
