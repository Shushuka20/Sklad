using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int Sales { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string BackgroundColor { get; set; }
        public bool Track { get; set; }
        public ICollection<Pack> Packs { get; set; }
        public ICollection<Greenhouse> Greenhouses { get; set; }
        public ICollection<GreenhouseForStock> GreenhouseForStocks { get; set; }
        public ICollection<Montaznik> Montazniks { get; set; }
        public ICollection<Claim> Claims { get; set; }

        public Stock()
        {
            Packs = new List<Pack>();
            Greenhouses = new List<Greenhouse>();
            GreenhouseForStocks = new List<GreenhouseForStock>();
            Montazniks = new List<Montaznik>();
            Claims = new List<Claim>();
        }
    }
}