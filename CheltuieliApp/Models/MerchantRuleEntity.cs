using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheltuieliApp.Models
{
    public class MerchantRuleEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Keyword { get; set; } = "";
        public int CategoryId { get; set; }

        public string Bank { get; set; } = ""; // optional
    }
}
