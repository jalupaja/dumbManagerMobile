using PassManager.Models;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using SQLite;
using Password_Manager;
using System.Threading.Tasks;
using TwoStepsAuthenticator;
using System.Reflection;

namespace PassManager.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private int itemId;
        private string name;
        private string username;
        private string password;
        private string url;
        private string twoFA;
        private string note;

        public int Id { get; set; }

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

        public int ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
                LoadItemId(value);
            }
        }

        public async void LoadItemId(int itemId)
        {
            await Task.FromResult(true);
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                Name = item.Name;
                Username = item.Username;
                Password = item.Password;
                Url = item.Url;
                TwoFA = item.TwoFA;
                Note = item.Note;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }


        internal void DeleteItemCommand()
        {
            Constants.con().Delete<Item>(Id);
            DataStore.DeleteItemAsync(ItemId);

            Shell.Current.GoToAsync("..");
        }

        public void Updateİnfo()
        {
            Item updateItem = new Item()
            {
                Id = Id,
                Name = Name,
                Username = Username,
                Password = Password,
                Url = Url,
                TwoFA = TwoFA,
                Note = Note
            };

            Constants.con().Update(updateItem);

            DataStore.UpdateItemAsync(updateItem);
        }

    }
}
