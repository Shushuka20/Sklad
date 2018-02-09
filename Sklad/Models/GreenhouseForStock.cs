using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class GreenhouseForStock
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Group { get; set; }
        public int Amount { get; set; }
        public Stock Stock { get; set; }
        public ICollection<Pack> Packs { get; set; }

        public GreenhouseForStock()
        {
            Packs = new List<Pack>();
        }
    }
}