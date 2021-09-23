using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PassManager.Models;
using SQLite;

namespace Password_Manager
{
    public static class Constants
    {

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

        public static string pw = "abc";//!!!
        public static string name = "abc";//!!!

        private static SQLiteConnection nCon(string nName = "", string nPw = "")//!!!
        {
            if (nName == "" && nPw == "")
            {
                nName = name;
                nPw = pw;
            }
            else
            {
                name = nName;
                pw = nPw;
            }
            var nc = new SQLiteConnection(new SQLiteConnectionString(Path.Combine(basePath, HashIt(nName)), Flags, true, key: nPw));
            nc.CreateTable<Item>();
            return nc;
        }

        public static SQLiteConnection con()
        {
            if (c == null)
                c = nCon();
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

    }
}
