using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class HistoryMoney
    {
        public int Id { get; set; }
        public decimal Cost { get; set; }
        public Dealer Dealer { get; set; }
        public Stock Stock { get; set; }
        public bool PayForTerminal { get; set; }
        public DateTime Date { get; set; }
        public string Who { get; set; }
    }
}