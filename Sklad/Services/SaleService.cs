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

        public void SaleDelete(int? id)
        {
            Sale sale = _db.Sales
               .Include(s => s.Stock)
               .FirstOrDefault(s => s.Id == id);

            var saleFirst = _db.Sales.Where(s => s.Number == sale.Number).OrderBy(s => s.Date).First();

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