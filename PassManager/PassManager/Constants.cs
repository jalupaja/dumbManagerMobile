using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PassManager;
using PassManager.Models;
using SQLite;

namespace Password_Manager
{
    public static class Constants
    {
        public static string retNPass = "";
        public static string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static SQLiteConnection c = null;

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache |
            SQLite.SQLiteOpenFlags.FullMutex;

        public static string pw = "";
        public static string name = "";

        public static bool TryLogin(string name, string pw)
        {
            bool works = true;
            SQLiteConnection nc = null;

            //Check for updated file
            if (File.Exists(Path.Combine(basePath, HashIt(name) + ".db.tmp")))
            {
                App.Current.MainPage.DisplayAlert("exists", "", "c");
                //Delete leftovers from last sync
                Directory.Delete(Path.Combine(Constants.basePath, "dumbManagerSync"), true);

                bool cont = true;
                try
                {
                    File.Move(Path.Combine(basePath, HashIt(name) + ".db"), Path.Combine(basePath, HashIt(name) + ".db") + ".temp");
                }
                catch (Exception)
                {
                    cont = false;
                    //parent.setSyncResponse("ERROR:" + Environment.NewLine + "An updated file has been detected but there has been a problem replacing the old file!");
                }
                if (cont)
                {
                    try
                    {
                        File.Move(Path.Combine(basePath, HashIt(name) + ".db") + ".tmp", Path.Combine(basePath, HashIt(name) + ".db"));
                    }
                    catch (Exception)
                    {
                        cont = false;
                        File.Move(Path.Combine(basePath, HashIt(name) + ".db") + ".temp", Path.Combine(basePath, HashIt(name) + ".db"));
                        //parent.setSyncResponse("ERROR:" + Environment.NewLine + "An updated file has been detected but there has been a problem replacing the old file!");
                    }
                }
                if (cont)
                {
                    try
                    {
                        File.Delete(Path.Combine(basePath, HashIt(name) + ".db") + ".temp");
                    }
                    catch (Exception)
                    {
                        cont = false;
                        //parent.setSyncResponse("ERROR:" + Environment.NewLine + "There has been an error updating your file!");
                    }
                }
                if (cont)
                {
                    //parent.setSyncResponse("SUCCESS:" + Environment.NewLine + "Your file has been successfully updated!");
                }
            }

            try
            {
                nc = new SQLiteConnection(new SQLiteConnectionString(Path.Combine(basePath, HashIt(name) + ".db"), Flags, true, key: pw));
                nc.CreateTable<Item>();
            }
            catch (Exception)
            {
                works = false;
            }
            if (works)
            {
                c = nc;
                Constants.pw = pw;
                Constants.name = name;
            }
            return works;
        }

        public static SQLiteConnection con()
        {
            return c;
        }

        public static string HashIt(String input)
        {
            if (input == null)
                return ".";
            SHA512 sha = new SHA512Managed();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = sha.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static void AddToFile(string line)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(line);
            SymmetricAlgorithm crypt = Aes.Create();
            HashAlgorithm hash = SHA256.Create();
            crypt.BlockSize = 128;
            crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(pw));
            crypt.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                   new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }
                File.AppendAllText(Path.Combine(basePath, HashIt(name)), Convert.ToBase64String(memoryStream.ToArray()) + Environment.NewLine);
            }
        }
    }
}
