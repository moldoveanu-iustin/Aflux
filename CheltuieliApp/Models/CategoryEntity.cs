using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheltuieliApp.Models
{
    public class CategoryEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Type { get; set; } = ""; // Expense / Income / Both
        public string ColorHex { get; set; } = "#3F51B5";
        public string Icon { get; set; } = "";
    }
}
