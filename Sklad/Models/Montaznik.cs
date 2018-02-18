using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Montaznik
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public string PasportNum { get; set; }
        public string WhoIssued { get; set; }
        public string WhereIssued { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string MarkAuto { get; set; }
        public string NumberAuto { get; set; }
        public string INN { get; set; }
        public string Snils { get; set; }
        public Stock Stock { get; set; }
        public virtual ICollection<Installment> Installments { get; set; }
        public ICollection<Sale> Sales { get; set; }

        public Montaznik()
        {
            Installments = new List<Installment>();
            Sales = new List<Sale>();
        }
    }
}