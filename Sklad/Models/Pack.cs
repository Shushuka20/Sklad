using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Pack
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Group { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public int CostMin { get; set; }
        public Stock Stock { get; set; }
        public ICollection<Greenhouse> Greenhouses { get; set; }
        public ICollection<GreenhouseForSale> GreenhouseForSales { get; set; }

        public Pack()
        {
            Greenhouses = new List<Greenhouse>();
            GreenhouseForSales = new List<GreenhouseForSale>();
        }
    }
}