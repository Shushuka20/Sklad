using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class GreenhouseCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Stock Stock { get; set; }
        public ICollection<Greenhouse> Greenhouses { get; set; }

        public GreenhouseCategory()
        {
            Greenhouses = new List<Greenhouse>();
        }
    }
}