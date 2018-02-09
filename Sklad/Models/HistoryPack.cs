using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class HistoryPack
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Pack Pack { get; set; }
        public DateTime Date { get; set; }
        public Stock Stock { get; set; }
        public Sale Sale { get; set; }
        public bool ForHistory { get; set; }
    }
}