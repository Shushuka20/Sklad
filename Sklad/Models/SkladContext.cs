using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Sklad.Models
{
    public class SkladContext : DbContext
    {
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Greenhouse> Greenhouses { get; set; }
        public DbSet<Pack> Packs { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PackForGH> PacksForGh { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<GreenhouseForSale> GreenhouseForSales { get; set; }
        public DbSet<GreenhouseForStock> GreenhouseForStocks { get; set; }
        public DbSet<HistoryMoney> HistoryMoneys { get; set; }
        public DbSet<HistoryPack> HistoryPacks { get; set; }
        public DbSet<InfoMoney> InfoMoneys { get; set; }
        public DbSet<Montaznik> Montazniks { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Installment> Installments { get; set; }
    }
}