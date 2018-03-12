using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklad.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string FIO { get; set; }
        public string Phone { get; set; }
        public string AddressInstallation { get; set; }
        public string OutgoDescription { get; set; }
        public string OutgoCategory { get; set; }
        public string Comment { get; set; }
        public int Outgo { get; set; }
        public int DeliveryCost { get; set; }
        public decimal SumWithD { get; set; }
        public decimal SumWithoutD { get; set; }
        public decimal Discount { get; set; }
        public decimal AddMoney { get; set; }
        public decimal Remain { get; set; } //остаток
        public decimal SumBonuses { get; set; }
        public decimal SumCostPrice { get; set; }
        public decimal Profit { get; set; }
        public bool Shipment { get; set; } //отгрузка
        public bool Delivery { get; set; }
        public bool PayForTerminal { get; set; }
        public bool Inspect { get; set; }
        public bool Payment { get; set; }
        public bool Confirmed { get; set; }
        public bool DeliveryConfirm { get; set; }
        public Buyer Buyer { get; set; }
        public Dealer Dealer { get; set; }
        public Stock Stock { get; set; }
        public Montaznik Montaznik { get; set; }
        public ICollection<GreenhouseForSale> GreenhouseForSales { get; set; }

        public Sale()
        {
            GreenhouseForSales = new List<GreenhouseForSale>();
        }
    }
}