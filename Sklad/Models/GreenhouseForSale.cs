using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class GreenhouseForSale
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Group { get; set; }
        public int Amount { get; set; }
        public Stock Stock { get; set; }
        public Sale Sale { get; set; }
        public Claim Claim { get; set; }
        public ICollection<Pack> Packs { get; set; }

        public GreenhouseForSale()
        {
            Packs = new List<Pack>();
        }
    }
}