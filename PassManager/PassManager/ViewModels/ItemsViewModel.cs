using Dropbox.Api;
using Dropbox.Api.Files;
using PassManager.Models;
using PassManager.Views;
using Password_Manager;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PassManager.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private static readonly string APIKEY = "qy5zl04iap7sbhn"; //CHANGE DROPBOX APP KEY HERE!
        //Full Access  Team App: qy5zl04iap7sbhn

        private Item _selectedItem;

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command SyncCommand { get; }
        public Command<Item> ItemTapped { get; }
        //public Command<Item> DeleteItemCommand { get; }


        public ItemsViewModel()
        {

            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);

            SyncCommand = new Command(Sync);

            //DeleteItemCommand = new Command<Item>(DeleteItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }

        private void Sync()
        {
            int failCount = 0;
            parent.setSyncResponse(string.Empty);
            #region first DropNet
            bool pass = true;
            try
            {
                _ = new DropboxClient(Constants.con().Table<Item>().Where(x => x.Name.ToLower().Equals("Dropbox(Sync)".ToLower())).First().Note.Split('|')[1]);
            }
            catch (Exception)
            {
                pass = false;
            }
            if (pass)
            {
                //check internet Connection
                parent.addSyncResponse("Checking Internet Connection");
                try
                {
                    using (var client = new System.Net.WebClient())
                    using (var stream = client.OpenRead("https://www.duckduckgo.com")) { }
                }
                catch
                {
                    parent.setSyncResponse("ERROR:" + Environment.NewLine + "You are not connected to the Internet");
                    parent.finishedSyncing();
                    return;
                }
                try
                {
                    _ = new DropboxClient(Constants.con().Table<Item>().Where(x => x.Name.ToLower().Equals("Dropbox(Sync)".ToLower())).First().Note.Split('|')[1]);
                }
                catch (Exception)
                {
                    parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem logging into Dropbox!" + Environment.NewLine + "If this keeps happening, please delete the notes of Dropbox(Sync)!");
                    parent.finishedSyncing();
                    return;
                }

                var _client = new DropboxClient(Constants.con().Table<Item>().Where(x => x.Name.ToLower().Equals("Dropbox(Sync)".ToLower())).First().Note.Split('|')[1]);

                string tmpFolder = Path.Combine(Constants.basePath, "dumbManagerSync");
                try { Directory.CreateDirectory(tmpFolder); } catch (Exception) { }

                parent.addSyncResponse("Checking if file exists");
                try
                {
                    //Download file into temp Folder
                    var response = _client.Files.DownloadAsync("/" + Constants.HashIt(Constants.name) + ".db");
                    var result = response.Result.GetContentAsStreamAsync();
                    using (var dwl = File.Create(Path.Combine(tmpFolder, Constants.HashIt(Constants.name) + ".db")))
                    {
                        result.Result.CopyTo(dwl);
                    }
                }
                catch (Exception)
                {
                    parent.addSyncResponse("Uploading local file");
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
                        parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem uploading your file!");
                        parent.finishedSyncing();
                        return;
                    }

                    parent.addSyncResponse($"Deleting {Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)))}");
                    File.Delete(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));
                    parent.setSyncResponse("SUCCESS:" + Environment.NewLine + "Uploaded local file.");
                    parent.finishedSyncing();
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
                        parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been problem downloading the files!");
                        parent.finishedSyncing();
                        return;
                    }
                }
                */
                SQLiteConnection con = null;

                //Read local update file and update the downloaded File
                if (File.Exists(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name))))
                {
                    parent.addSyncResponse("Updating Password Manager");
                    try
                    {
                        con = new SQLiteConnection(new SQLiteConnectionString(Path.Combine(tmpFolder, Constants.HashIt(Constants.name) + ".db"), Constants.Flags, true, key: Constants.pw));
                        con.CreateTable<Item>();
                    }
                    catch (Exception)
                    {
                        parent.setSyncResponse("ERROR: Your local and online password dont seem to match!");
                        Cleanup(tmpFolder);
                        parent.finishedSyncing();
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
                                                    parent.addSyncResponse($"Adding {linePart[1]}");
                                                    try
                                                    {
                                                        con.Insert(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        parent.addSyncResponse("ERROR:" + Environment.NewLine + $"{linePart[1]} could not be added");
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
                                                    parent.addSyncResponse($"Updating {linePart[2]}");
                                                    try
                                                    {
                                                        con.Update(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        parent.addSyncResponse($"ERROR: {linePart[2]} could not be updated");
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
                                                    parent.addSyncResponse($"Deleting {linePart[2]}");
                                                    try
                                                    {
                                                        con.Delete(n);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        parent.addSyncResponse($"ERROR: {linePart[2]} could not be Deleted");
                                                        failCount++;
                                                    }
                                                    n = null;
                                                    break;
                                                default:
                                                    parent.addSyncResponse($"ERROR: Could not Resolve Command");
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
                        parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been an unknown error while updating local file!");
                        parent.finishedSyncing();
                        return;
                    }
                    if (failCount > 0)
                    {
                        parent.addSyncResponse("Error:" + Environment.NewLine + $"There have been {failCount} Errors while Syncing!");
                        new FrmLittleBox($"There have been {failCount} Errors while Syncing!", parent.getSyncResponse()).Show();
                        if (MessageBox.Show($"There have been {failCount} Errors while Syncing!" + Environment.NewLine + "Are you sure that you want to continue?", $"Encountered {failCount} Errors", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            Cleanup(tmpFolder);
                            parent.SafeSyncFile(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".log");
                            parent.setSyncResponse("You can find the log file at" + Environment.NewLine + Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".log");
                            parent.finishedSyncing();
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
                    parent.setSyncResponse($"ERROR: Failed to delete online file");
                    Cleanup(tmpFolder);
                    parent.finishedSyncing();
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
                    parent.setSyncResponse("Error:" + Environment.NewLine + "There has been a problem updating the online file!" + e.Message);//!!!
                    Cleanup(tmpFolder);
                    parent.finishedSyncing();
                    return;
                }

                //Move all files and overwrite only Password Manager
                string[] files = Directory.GetFiles(tmpFolder);
                string localFolder = Path.GetDirectoryName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));

                foreach (string file in files)
                {
                    parent.addSyncResponse($"Moving {Path.GetFileName(file)} to local Folder");
                    if (Path.Combine(localFolder, Path.GetFileName(file)) == Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)) + ".db") //!!!
                    {
                        try
                        {
                            File.Copy(file, Path.Combine(localFolder, Path.GetFileName(file) + ".tmp"), false);
                        }
                        catch (Exception)
                        {
                            parent.setSyncResponse("Error:" + Environment.NewLine + "There has been a problem copying the new updated file!");
                            Cleanup(tmpFolder);
                            parent.finishedSyncing();
                            return;
                        }
                    }
                    else
                        File.Move(file, Path.Combine(localFolder, Path.GetFileName(file)));
                }

                Cleanup(tmpFolder);
                try
                {
                    parent.addSyncResponse($"Deleting {Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)))}");
                    File.Delete(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name)));
                }
                catch (Exception)
                {
                    parent.addSyncResponse($"ERROR: Failed to delete " + Environment.NewLine + Path.GetFileName(Path.Combine(Constants.basePath, Constants.HashIt(Constants.name))));
                }

                parent.setSyncResponse("SUCCESS:" + Environment.NewLine + "" + Environment.NewLine + "You have to restart in Order for the changes to take affect!");

                //!!! restart
                //!!! Process.Start(Application.ExecutablePath);
                //!!! parent.TrayExit(null, null);
                parent.finishedSyncing();
                return;
            }
            else
            {
                parent.setSyncResponse("You have to create an Account on dropbox.com authorize the App!");
                Uri Url = DropboxOAuth2Helper.GetAuthorizeUri(APIKEY);
                var tmp = new FrmLittleBox("Please open this Link in your browser and paste the Access Code below!", Url.AbsoluteUri, "Paste Access Code here");
                tmp.ShowDialog();
                if (tmp.ret == "" || tmp.DialogResult != DialogResult.OK)
                {
                    parent.setSyncResponse("ERROR:" + Environment.NewLine + "You have to enter an Access Code!");
                    parent.finishedSyncing();
                    return;
                }
                parent.CreateDropStuff(tmp.ret);

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
                    parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been a problem uploading your file!");
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
            parent.addSyncResponse("Cleaning temporary files:");
            foreach (string file in files)
            {
                try
                {
                    if (Path.GetFileName(file) != Constants.HashIt(Constants.name))
                    {
                        parent.addSyncResponse($"Deleting {Path.GetFileName(file)}");
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    parent.addSyncResponse("ERROR: Failed to delete" + Environment.NewLine + Path.GetFileName(file));
                }
            }
        }
    }
}