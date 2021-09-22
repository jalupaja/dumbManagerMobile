using System;
using SQLite;

namespace PassManager.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string TwoFA { get; set; }
        public string Note { get; set; }
    }
}