using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Dealer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        //На сколько взял
        public string CostRecommend { get; set; }
        public decimal Debt { get; set; }
        public bool EnabledForSale { get; set; }
        public ICollection<HistoryMoney> HistoryMoneys { get; set; }

        public Dealer()
        {
            HistoryMoneys = new List<HistoryMoney>();
        }
    }
}