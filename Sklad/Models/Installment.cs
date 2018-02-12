using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Installment
    {
        public int Id { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ForDate { get; set; }
        public bool Light { get; set; }
        public string Comment { get; set; }
        public string Color { get; set; }
        public virtual ICollection<Montaznik> Montazniks { get; set; }

        public Sale Sale { get; set; }

        public Installment()
        {
            Montazniks = new List<Montaznik>();
        }
    }
}