using System;
using System.Data.Entity;
using System.Linq;
using System.Web.DynamicData;
using Sklad.Models;

namespace Sklad.Services
{
    public class SaleService
    {
        private SkladContext _db;

        public SaleService(SkladContext db)
        {
            _db = db;
        }
        
        //To do возврат остатка на реализацию
        public void SaleDelete(int? id)
        {
            Sale sale = _db.Sales
               .Include(s => s.Stock)
               .FirstOrDefault(s => s.Id == id);

            var saleFirst = _db.Sales.Where(s => s.Number == sale.Number).OrderBy(s => s.Id).First();

            if (sale.Id == saleFirst.Id)
            {
                var sales = _db.Sales.Where(s => s.Number == sale.Number).OrderByDescending(s => s.Date).ToList();
                foreach (var s in sales)
                {
                    SaleDelete(s);
                }
            }
            else if (sale.Id != saleFirst.Id)
            {
                SaleDelete(sale);
            }

            _db.SaveChanges();
        }

        private void SaleDelete(Sale sale)
        {
            var greenhousesForSale = _db.GreenhouseForSales
                    .Include(g => g.Sale)
                    .Where(g => g.Sale.Id == sale.Id).ToList();
            if (sale.Shipment == true)
            {
                foreach (var g in greenhousesForSale)
                {
                    Greenhouse g1 = _db.Greenhouses
                        .Include(gh => gh.PacksForGH)
                        .Include(gh => gh.Stock)
                        .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == sale.Stock.Id);

                    foreach (var p in g1.PacksForGH)
                    {
                        Pack p1 = _db.Packs
                            .Include(pck => pck.Stock)
                            .FirstOrDefault(pck => pck.Name == p.Name && pck.Stock.Id == sale.Stock.Id);
                        p1.Amount += 1 * p.Amount * g.Amount;

                        //хуй знает нужно или нет, чтобы сохранялась в историю при удалении сэйла, потестить
                       /* HistoryPack hp1 = new HistoryPack()
                        {
                            Name = p1.Name,
                            Amount = p.Amount * g.Amount * 1,
                            Date = DateTime.Now,
                            ForHistory = false,
                            Sale = null,
                            Pack = p1,
                            Stock = sale.Stock,
                            Description = sale.Number
                        };
                        _db.HistoryPacks.Add(hp1);*/
                    }
                }
            }

            foreach (var greenhouseForSale in greenhousesForSale)
            {
                greenhouseForSale.Stock = null;
                greenhouseForSale.Stock = null;
                greenhouseForSale.Claim = null;
                _db.Entry(greenhouseForSale).State = EntityState.Modified;
                _db.GreenhouseForSales.Remove(greenhouseForSale);
            }

            var historyPacks = _db.HistoryPacks
                .Include(h => h.Sale)
                .Where(h => h.Sale.Id == sale.Id).ToList();
            foreach (var historyPack in historyPacks)
            {
                historyPack.Stock = null;
                historyPack.Sale = null;
                historyPack.Pack = null;
                _db.Entry(historyPack).State = EntityState.Modified;
                _db.HistoryPacks.Remove(historyPack);
            }

            var infoMoneys = _db.InfoMoneys
                .Include(i => i.Sale)
                .Where(i => i.Sale.Id == sale.Id).ToList();
            foreach (var infoMoney in infoMoneys)
            {
                infoMoney.Sale = null;
                infoMoney.Stock = null;
                infoMoney.Dealer = null;
                _db.Entry(infoMoney).State = EntityState.Modified;
                _db.InfoMoneys.Remove(infoMoney);
            }

            var installments = _db.Installments
                .Include(i => i.Sale)
                .Where(i => i.Sale.Id == sale.Id).ToList();
            foreach (var installment in installments)
            {
                installment.Sale = null;
                _db.Entry(installment).State = EntityState.Modified;
                _db.Installments.Remove(installment);
            }
            _db.Sales.Remove(sale);
        }
    }
}