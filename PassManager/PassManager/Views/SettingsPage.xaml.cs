using Dropbox.Api;
using Dropbox.Api.Files;
using PassManager.Models;
using PassManager.Services;
using PassManager.Views;
using Password_Manager;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PassManager.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private static readonly string APIKEY = "qy5zl04iap7sbhn"; //CHANGE DROPBOX APP KEY HERE!
        //Full Access  Team App: qy5zl04iap7sbhn
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        public SettingsPage()
        {
            InitializeComponent();
        }
        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
        }
        private async void Export_Clicked(object sender, EventArgs e)
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (permissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                if (permissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return;
            }
            File.Copy(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name) + ".db"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Constants.HashIt(Constants.name) + ".db"));
            await App.Current.MainPage.DisplayAlert("", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "k");
        }

        private async void Import_Clicked(object sender, EventArgs e)
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (permissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                if (permissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return;
            }

            var dest = await FilePicker.PickAsync();
            var x = dest.FileName.Split('.');
            if (x[x.Length - 1] == "db")
            {
                if (dest.FileName.Length < 1)
                    return;
                else if (File.Exists(Path.Combine(Constants.basePath, dest.FileName)))
                {
                    if (await App.Current.MainPage.DisplayAlert("File already exists", "Do you want to overwrite the existing file?", "Yes", "No"))
                    {
                        try
                        {
                            File.Delete(Path.Combine(Constants.basePath, dest.FileName));
                        }
                        catch (Exception)
                        {
                            await App.Current.MainPage.DisplayAlert("cant access file", "Couln't overwrite file.\nPlease try again later", "OK");
                            return;
                        }
                    }
                    else
                        return;
                }
                File.Copy(dest.FullPath, Path.Combine(Constants.basePath, dest.FileName));
                return;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("file not valid", "You have to select a valid file", "OK");
                return;
            }
        }

        private void Sync_Clicked(object sender, EventArgs e)
        {
            Sync();
        }
        private async void Sync()
        {
            int failCount = 0;
            string accesstoken = "";
            TxtSyncRes.Text = string.Empty;

            #region first DropNet
            bool pass = true;
            try
            {
                accesstoken = Constants.con().Table<Item>().Where(x => x.Name.ToLower().Equals("Dropbox(Sync)".ToLower())).First().Note.Split('|')[1];
                _ = new DropboxClient(accesstoken);
            }
            catch (Exception)
            {
                pass = false;
            }
            if (pass)
            {
                //check internet Connection
                addSyncResponse("Checking Internet Connection");
                addSyncResponse("stopping cuz Dropbox doesn't like me");//!!!
                return;//!!!
                try
                {
                    using (var client = new System.Net.WebClient())
                    using (var stream = client.OpenRead("https://www.duckduckgo.com")) { }
                }
                catch
                {
                    setSyncResponse("ERROR:" + Environment.NewLine + "You are not connected to the Internet");
                    return;
                }
                try
                {
                    _ = new DropboxClient(accesstoken);
                }
                catch (Exception)
                {
                    setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem logging into Dropbox!" + Environment.NewLine + "If this keeps happening, please delete the notes of Dropbox(Sync)!");
                    return;
                }

                var _client = new DropboxClient(accesstoken);

                string tmpFolder = Path.Combine(Constants.basePath, "dumbManagerSync");
                try { Directory.CreateDirectory(tmpFolder); } catch (Exception) { }

                addSyncResponse("Checking if file exists");
                try
                {
                    //Download file into temp Folder

                    //var resp = _client.Files.DownloadAsync("/" + Constants.HashIt(Constants.name) + ".db"); //resp.Result crashes
                    var response = _client.Files.DownloadAsync("/" + Constants.HashIt(Constants.name) + ".db"); //await crashes
                    response.Start();
                    var result = await response.Result.GetContentAsStreamAsync();
                    using (var dwl = File.Create(Path.Combine(tmpFolder, Constants.HashIt(Constants.name) + ".db")))
                    {
                        result.CopyTo(dwl);
                    }
                }
                catch (Exception)
                {
                    addSyncResponse("Uploading local file");
                    try
                    {
                        using (var upl = new FileStream(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".db", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var response = _client.Files.UploadAsync("/" + Constants.HashIt(Constants.name) + ".db", WriteMode.Overwrite.Instance, body: upl);
                            var rest = response.Result;
                        }
                    }
                    catch (Exception)
                    {
                        setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem uploading your file!");
                        return;
                    }

                    addSyncResponse($"Deleting {Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)))}");
                    File.Delete(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));
                    setSyncResponse("SUCCESS:" + Environment.NewLine + "Uploaded local file.");
                    return;
                }

                /*!!! download possible other files, starting with same name(e.g encrypted files)
                var onlineFiles = _client.Search(HashIt(TxtFileIn.Text) + "*");

                foreach (var onlineFile in onlineFiles)
                {
                    parent.addSyncResponse($"Downloading {onlineFile.Name}");
                    try
                    {
                        var fileBytes = _client.GetFile(onlineFile.Path);
                        using (var fs = new FileStream(Path.Combine(tmpFolder, onlineFile.Name), FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                    catch (Exception)
                    {
                        Cleanup(tmpFolder);
                        //parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been problem downloading the files!");
                        parent.finishedSyncing();
                        return;
                    }
                }
                */
                SQLiteConnection con = null;

                //Read local update file and update the downloaded File
                if (File.Exists(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name))))
                {
                    addSyncResponse("Updating Password Manager");
                    try
                    {
                        con = new SQLiteConnection(new SQLiteConnectionString(Path.Combine(tmpFolder, Constants.HashIt(Constants.name) + ".db"), Constants.Flags, true, key: Constants.pw));
                        con.CreateTable<Item>();
                    }
                    catch (Exception)
                    {
                        setSyncResponse("ERROR: Your local and online password dont seem to match!");
                        Cleanup(tmpFolder);
                        return;
                    }
                    con.BeginTransaction();

                    try
                    {
                        string line;
                        var n = new Item();
                        SymmetricAlgorithm crypt = Aes.Create();
                        HashAlgorithm hash = SHA256.Create();
                        crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(Constants.pw));
                        crypt.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
                        using (var str = new FileStream(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader file = new StreamReader(str))
                            {
                                while ((line = file.ReadLine()) != null)
                                {
                                    byte[] bytes = Convert.FromBase64String(line);
                                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                                    {
                                        using (CryptoStream cryptoStream =
                                        new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                                        {
                                            byte[] decryptedBytes = new byte[bytes.Length];
                                            cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                                            string decLine = Encoding.Unicode.GetString(decryptedBytes);
                                            //!!! apply lines to downloaded file
                                            string[] linePart = decLine.Split(',', ',', ',');
                                            switch (linePart[0])
                                            {
                                                case "INSERT":
                                                    n = new Item();
                                                    n.Name = linePart[1];
                                                    n.Username = linePart[2];
                                                    n.Password = linePart[3];
                                                    n.Url = linePart[4];
                                                    n.TwoFA = linePart[5];
                                                    n.Note = linePart[6];
                                                    addSyncResponse($"Adding {linePart[1]}");
                                                    try
                                                    {
                                                        con.Insert(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        addSyncResponse("ERROR:" + Environment.NewLine + $"{linePart[1]} could not be added");
                                                        failCount++;
                                                    }
                                                    n = null;
                                                    break;
                                                case "UPDATE":
                                                    n = new Item();
                                                    n.Id = Convert.ToInt32(linePart[1]);
                                                    n.Name = linePart[2];
                                                    n.Username = linePart[3];
                                                    n.Password = linePart[4];
                                                    n.Url = linePart[5];
                                                    n.TwoFA = linePart[6];
                                                    n.Note = linePart[7];
                                                    addSyncResponse($"Updating {linePart[2]}");
                                                    try
                                                    {
                                                        con.Update(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        addSyncResponse($"ERROR: {linePart[2]} could not be updated");
                                                        failCount++;
                                                    }
                                                    n = null;
                                                    break;
                                                case "DELETE":
                                                    n = new Item();
                                                    n.Id = Convert.ToInt32(linePart[1]);
                                                    n.Name = linePart[2];
                                                    n.Username = linePart[3];
                                                    n.Url = linePart[4];
                                                    addSyncResponse($"Deleting {linePart[2]}");
                                                    try
                                                    {
                                                        con.Delete(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        addSyncResponse($"ERROR: {linePart[2]} could not be Deleted");
                                                        failCount++;
                                                    }
                                                    n = null;
                                                    break;
                                                default:
                                                    addSyncResponse($"ERROR: Could not Resolve Command");
                                                    failCount++;
                                                    break;
                                            }
                                        }
                                    }
                                }
                                con.Commit();
                                con.Close();
                            }
                        }

                    }
                    catch (Exception)
                    {
                        con.Close();
                        Cleanup(tmpFolder);
                        setSyncResponse("ERROR:" + Environment.NewLine + "There has been an unknown error while updating local file!");
                        return;
                    }
                    if (failCount > 0)
                    {
                        addSyncResponse("Error:" + Environment.NewLine + $"There have been {failCount} Errors while Syncing!");
                        if (!await App.Current.MainPage.DisplayAlert($"There have been {failCount} Errors while Syncing!", "Are you sure that you want to continue?", "Yes", "No"))
                        {
                            Cleanup(tmpFolder);
                            //parent.SafeSyncFile(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".log");
                            //parent.setSyncResponse("You can find the log file at" + Environment.NewLine + Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".log");
                            return;
                        }
                    }
                }

                //delete online file
                try
                {
                    var folders = _client.Files.DeleteV2Async("/" + Constants.HashIt(Constants.name) + ".db");
                    var result = folders.Result;
                }
                catch (Exception)
                {
                    setSyncResponse($"ERROR: Failed to delete online file");
                    Cleanup(tmpFolder);
                    return;
                }

                //upload local file
                try
                {
                    using (var upl = new FileStream(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".db", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var response = _client.Files.UploadAsync("/" + Constants.HashIt(Constants.name) + ".db", WriteMode.Overwrite.Instance, body: upl);
                        var rest = response.Result;
                    }
                }
                catch (Exception e)
                {
                    setSyncResponse("Error:" + Environment.NewLine + "There has been a problem updating the online file!" + e.Message);//!!!
                    Cleanup(tmpFolder);
                    return;
                }

                //Move all files and overwrite only Password Manager
                string[] files = Directory.GetFiles(tmpFolder);
                string localFolder = Path.GetDirectoryName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));

                foreach (string file in files)
                {
                    addSyncResponse($"Moving {Path.GetFileName(file)} to local Folder");
                    if (Path.Combine(localFolder, Path.GetFileName(file)) == Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".db") //!!!
                    {
                        try
                        {
                            File.Copy(file, Path.Combine(localFolder, Path.GetFileName(file) + ".tmp"), false);
                        }
                        catch (Exception)
                        {
                            setSyncResponse("Error:" + Environment.NewLine + "There has been a problem copying the new updated file!");
                            Cleanup(tmpFolder);
                            return;
                        }
                    }
                    else
                        File.Move(file, Path.Combine(localFolder, Path.GetFileName(file)));
                }

                Cleanup(tmpFolder);
                try
                {
                    addSyncResponse($"Deleting {Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)))}");
                    File.Delete(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));
                }
                catch (Exception)
                {
                    addSyncResponse($"ERROR: Failed to delete " + Environment.NewLine + Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name))));
                }

                setSyncResponse("SUCCESS:" + Environment.NewLine + "" + Environment.NewLine + "You have to restart in Order for the changes to take affect!");

                //!!! restart
                //!!! Process.Start(Application.ExecutablePath);
                //!!! parent.TrayExit(null, null);
                return;
            }
            else
            {
                setSyncResponse("You have to create an Account on dropbox.com authorize the App!");
                Uri Url = DropboxOAuth2Helper.GetAuthorizeUri(APIKEY);
                string tmp = await App.Current.MainPage.DisplayPromptAsync("Please open this Link in your browser and paste the Access Code below!", Url.AbsoluteUri, placeholder: "Paste Access Code Here");
                //new FrmLittleBox("Please open this Link in your browser and paste the Access Code below!", Url.AbsoluteUri, "Paste Access Code here");
                if (tmp == null || tmp == "")
                {
                    setSyncResponse("ERROR:" + Environment.NewLine + "You have to enter an Access Code!");
                    return;
                }
                Item newItem = new Item()
                {
                    Name = "Dropbox(Sync)",
                    Username = "",
                    Password = "",
                    Url = "https://www.dropbox.com",
                    TwoFA = "",
                    Note = $"|{tmp}|"
                };
                Constants.con().Insert(newItem);

                var result = Constants.con().Table<Item>().ToList();
                foreach (var item in result)
                {
                    newItem.Id = item.Id;
                }

                await DataStore.AddItemAsync(newItem);

                Sync();
                /* //!!!
                var _client = new DropboxClient(tmp.ret);
                tmp.Dispose();
                try
                {
                    using (var upl = new FileStream(fpath + ".db", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var response = _client.Files.UploadAsync("/" + HashIt(TxtFileIn.Text) + ".db", WriteMode.Overwrite.Instance, body: upl);
                        var rest = response.Result;
                    }
                        
                }
                catch (Exception)
                {
                    _client.Dispose();
                    //parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem uploading your file!");
                    parent.finishedSyncing();
                    return;
                }

                File.Delete(fpath);
                _client.Dispose();
                parent.finishedSyncing();
                return;
                */
            }
            #endregion first DropNet

        }
        private void Cleanup(string folderpath)
        {
            string[] files = Directory.GetFiles(folderpath);
            addSyncResponse("Cleaning temporary files:");
            foreach (string file in files)
            {
                try
                {
                    if (Path.GetFileName(file) != Constants.HashIt(Constants.name))
                    {
                        addSyncResponse($"Deleting {Path.GetFileName(file)}");
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    addSyncResponse("ERROR: Failed to delete" + Environment.NewLine + Path.GetFileName(file));
                }
            }
        }
        private void addSyncResponse(string line)
        {
            TxtSyncRes.Text += line + Environment.NewLine;
        }
        private void setSyncResponse(string line)
        {
            TxtSyncRes.Text += Environment.NewLine + Environment.NewLine + Environment.NewLine + line + Environment.NewLine;
        }
    }
}