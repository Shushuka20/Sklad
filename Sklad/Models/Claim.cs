using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string Text { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string CommentProd { get; set; }
        public string StockName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public bool Confirm { get; set; }
        public bool Edited { get; set; }
        public DateTime Date { get; set; }
        public Stock Stock { get; set; }
        public ICollection<GreenhouseForSale> GreenhouseForSales { get; set; }

        public Claim()
        {
            GreenhouseForSales = new List<GreenhouseForSale>();
        }
    }
}