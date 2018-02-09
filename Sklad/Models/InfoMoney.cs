using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class InfoMoney
    {
        public int Id { get; set; }
        public Stock Stock { get; set; }
        public DateTime Date { get; set; }
        public Sale Sale { get; set; }
        public string Number { get; set; }
        public Dealer Dealer { get; set; }
        public decimal Cost { get; set; }
        public bool PayForTerminal { get; set; }
    }
}