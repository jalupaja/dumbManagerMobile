using PassManager.Services;
using PassManager.Views;
using System;
using System.Net;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PassManager
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected async override void OnStart()
        {
            int offlineVersion = Int16.Parse(AppInfo.VersionString.Replace(".", "").Replace("v", ""));
            int onlineVersion = 0;
            string uri;
            if (Device.RuntimePlatform == Device.Android)
            {
                try { onlineVersion = Int16.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/jalupaja/dumbManagerMobile/tree/main/PassManager/AndroidVersionNumber.txt")); }catch (Exception) { }
                uri = "";//!!!

            }
            else if (Device.RuntimePlatform == Device.iOS)
            {
                try { onlineVersion = Int16.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/jalupaja/dumbManagerMobile/tree/main/PassManager/IosVersionNumber.txt")); }catch (Exception) { }
                uri = ""; //!!!
            }
            else return;
            if (offlineVersion < onlineVersion)
            {
                if (await App.Current.MainPage.DisplayAlert("update available", "Do you want to download a new update?", "Yes", "No"))
                {
                    await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
