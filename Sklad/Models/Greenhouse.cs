using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Greenhouse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Group { get; set; }
        public int Amount { get; set; }
        public int Position { get; set; }
        public decimal Bonus { get; set; }
        public decimal CostPrice { get; set; }
        public Stock Stock { get; set; }
        public ICollection<PackForGH> PacksForGH { get; set; }
        public ICollection<Pack> Packs { get; set; }

        public Greenhouse()
        {
            Packs = new List<Pack>();
            PacksForGH = new List<PackForGH>();
        }
    }
}