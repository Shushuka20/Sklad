using System.Linq;
using Sklad.Models;

namespace Sklad.Services
{
    //TO DO: разобраться с инклюдами, чтобы инкапсулировать склад сюды в инит, а не тянуть его постоянно
    public class StatisticService
    {
        private readonly SkladContext _db;

        public StatisticService(SkladContext db)
        {
            _db = db;
        }

        //Денег на кассе
        public decimal CashOnHand(int? id)
        {
            Stock stock = _db.Stocks.FirstOrDefault(s => s.Id == id);
            if (stock == null)
                return 0;

            decimal sum = 0;

            foreach (var im in _db.InfoMoneys
                .Where(i => i.Stock.Id == stock.Id && i.PayForTerminal != true))
            {
                sum += im.Cost;
            }

            return sum;
        }

        //Задолжность клиентов на рознице
        public decimal Debt(int? id)
        {
            Stock stock = _db.Stocks.FirstOrDefault(s => s.Id == id);
            if (stock == null)
                return 0;

            decimal debt = 0;

            foreach (var s in _db.Sales
                .Where(s => s.Stock.Id == stock.Id && s.Dealer == null && s.Remain > 0 && s.Confirmed == true))
            {
                debt += s.Remain;
            }

            return debt;
        }
    }
}