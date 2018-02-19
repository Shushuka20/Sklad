using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Sklad.Models;

namespace Sklad.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        SkladContext db = new SkladContext();

        public ActionResult Index()
        {
            var infos = db.Stocks
                .Where(s => s.Track == true)
                .GroupJoin(
                    db.Sales.Where(i => i.PayForTerminal != true),
                    stock => stock.Id,
                    info => info.Stock.Id,
                    (stock, sale) => new
                    {
                        Id = stock.Id,
                        Name = stock.Name,
                        Prefix = stock.Prefix,
                        Track = stock.Track,
                        Sum = sale.Sum(i => i.AddMoney)
                    })
                .GroupJoin(
                    db.Sales.Where(sale => sale.Dealer == null && sale.Remain > 0 && sale.Confirmed == true),
                    obj => obj.Id,
                    sale => sale.Stock.Id,
                    (obj, sales) => new Info
                    {
                        Id = obj.Id,
                        Name = obj.Name,
                        Prefix = obj.Prefix,
                        Sum = obj.Sum,
                        Track = obj.Track,
                        Debt = sales.Sum(s => s.Remain)
                    });

            return View(infos);
        }

        public ActionResult Users()
        {
            IEnumerable<User> users = db.Users;

            return View(users);
        }

        [HttpGet]
        public ActionResult UserNew(int? id)
        {
            if (id != null)
            {
                User user = db.Users.FirstOrDefault(u => u.Id == id);

                return View(user);
            }
            else
            {
                return RedirectToAction("Users", "Admin");
            }
        }

        [HttpPost]
        public ActionResult UserNew(int? id, string login, string password, string role, string stock)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            user.Login = login;
            user.Password = password;
            user.Role = role;
            Stock s1 = null;
            s1 = db.Stocks.FirstOrDefault(s => s.Name == stock);
            if (s1 != null)
            {
                user.Stock = s1;
            }
            db.SaveChanges();

            return RedirectToAction("Users", "Admin");
        }

        public ActionResult UserDelete(int? id)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Users", "Admin");
        }

        public ActionResult Stocks()
        {
            var infos = db.Stocks
                .GroupJoin(
                    db.Sales.Where(i => i.PayForTerminal != true),
                    stock => stock.Id,
                    info => info.Stock.Id,
                    (stock, sale) => new
                    {
                        Id = stock.Id,
                        Name = stock.Name,
                        Prefix = stock.Prefix,
                        Track = stock.Track,
                        Sum = sale.Sum(i => i.AddMoney)
                    })
                .GroupJoin(
                    db.Sales.Where(sale => sale.Dealer == null && sale.Remain > 0 && sale.Confirmed == true),
                    obj => obj.Id,
                    sale => sale.Stock.Id,
                    (obj, sales) => new Info
                    {
                        Id = obj.Id,
                        Name = obj.Name,
                        Prefix = obj.Prefix,
                        Sum = obj.Sum,
                        Track = obj.Track,
                        Debt = sales.Sum(s => s.Remain)
                    });

            return View(infos);
        }

        [HttpGet]
        public ActionResult Stock(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);

            return View(stock);
        }

        [HttpPost]
        public ActionResult Stock(int? id, string name, string prefix, string color)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            stock.Name = name;
            stock.Prefix = prefix;
            stock.BackgroundColor = color;
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult StockDelete(int? id)
        {
            Stock stock = db.Stocks
                 .Include(s => s.Packs)
                 .Include(s => s.Greenhouses)
                 .FirstOrDefault(s => s.Id == id);
            db.Stocks.Remove(stock);
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public ActionResult StockAdd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StockAdd(string name, string pref, string color)
        {
            Stock stock = new Stock() { Name = name, Prefix = pref, Sales = 599, BackgroundColor = color };
            db.Stocks.Add(stock);
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult PackList(int? id)
        {
            Stock stock = db.Stocks.Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            ViewBag.Stock = stock;
            IEnumerable<Pack> packs = stock.Packs.OrderBy(p => p.Group);

            /* HttpCookie cookieReq = Request.Cookies["Packs"];
             int packId = 0;
             if (cookieReq != null)
             {
                 packId = Convert.ToInt32(cookieReq["id"]);
             }
             ViewBag.PackId = packId;*/

            return View(packs);
        }

        [HttpGet]
        public ActionResult PackAdd(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);

            return View(stock);
        }

        [HttpPost]
        public ActionResult PackAdd(int? id, string name, decimal amount, decimal cost, int group)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            stock.Packs.Add(new Pack { Name = name, Amount = amount, Cost = cost, Group = group });
            db.SaveChanges();

            return RedirectToAction("PackList", "Admin", new { id = id });
        }

        [HttpGet]
        public ActionResult Pack(int? id)
        {
            Pack pack = db.Packs.FirstOrDefault(p => p.Id == id);

            return View(pack);
        }

        [HttpPost]
        public ActionResult Pack(int? id, string name, decimal amount, decimal cost, int group, int costMin)
        {
            Pack pack = db.Packs.Include(p => p.Stock).FirstOrDefault(p => p.Id == id);

            IEnumerable<Greenhouse> gh1 = db.Greenhouses
                .Include(g => g.PacksForGH)
                .Include(g => g.Stock)
                .Where(g => g.Stock.Id == pack.Stock.Id);

            foreach (var g in gh1)
            {
                PackForGH p1 = null;
                p1 = g.PacksForGH.FirstOrDefault(p => p.Name == pack.Name);
                if (p1 != null)
                {
                    p1.Name = name;
                    p1.Cost = cost;
                }
            }

            pack.Name = name;
            pack.Amount = amount;
            pack.Cost = cost;
            pack.Group = group;
            pack.CostMin = costMin;

            db.SaveChanges();

            /*HttpCookie cookie = new HttpCookie("Packs");
            cookie["id"] = Convert.ToString(pack.Id);
            Response.Cookies.Add(cookie);*/

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult PackDelete(int? id)
        {
            Pack pack = db.Packs.Include(p => p.Stock).FirstOrDefault(p => p.Id == id);

            IEnumerable<Greenhouse> gh1 = db.Greenhouses
                .Include(g => g.PacksForGH)
                .Include(g => g.Stock)
                .Where(g => g.Stock.Id == pack.Stock.Id);

            foreach (var g in gh1)
            {
                PackForGH p1 = null;
                p1 = g.PacksForGH.FirstOrDefault(p => p.Name == pack.Name);
                if (p1 != null)
                {
                    db.PacksForGh.Remove(p1);
                }
            }

            db.Packs.Remove(pack);
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult GreenhouseList(int? id)
        {
            Stock stock = db.Stocks.Include(s => s.Greenhouses).Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            ViewBag.Stock = stock;
            IEnumerable<Greenhouse> greenhouses = db.Greenhouses
                .Include(g => g.PacksForGH)
                .Include(g => g.Stock)
                .Include(g => g.GetGreenhouseCategory)
                .Where(g => g.Stock.Id == stock.Id);

            return View(greenhouses);
        }

        [HttpGet]
        public ActionResult GreenhouseAdd(int? id)
        {
            Stock stock = db.Stocks.Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            ViewBag.Packs = stock.Packs;
            ViewBag.Categories = db.GreenhouseCategories.Include(s => s.Stock).Where(c => c.Stock.Id == stock.Id).ToList();

            return View(stock);
        }

        [HttpPost]
        public ActionResult GreenhouseAdd(int? id, Greenhouse model, int? categoryId)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            GreenhouseCategory category = db.GreenhouseCategories.FirstOrDefault(c => c.Id == categoryId);



            List<PackForGH> packsForGH = new List<PackForGH>();
            List<Pack> packs = new List<Pack>();
            foreach (var p in model.Packs)
            {
                if (p.Amount != 0)
                {
                    Pack pack = db.Packs.FirstOrDefault(pck => pck.Id == p.Id);
                    packsForGH.Add(new PackForGH { Name = pack.Name, Cost = pack.Cost, Amount = p.Amount });
                    packs.Add(pack);
                }
            }

            Greenhouse g1 = new Greenhouse()
            {
                Name = model.Name,
                GetGreenhouseCategory = category,
                Packs = packs,
                PacksForGH = packsForGH,
                Bonus = model.Bonus,
                CostPrice = model.CostPrice
            };
            stock.Greenhouses.Add(g1);
            db.SaveChanges();

            return RedirectToAction("GreenhouseList", "Admin", new { id = id });
        }

        [HttpGet]
        public ActionResult Greenhouse(int? id)
        {
            Greenhouse greenhouse = db.Greenhouses.Include(g => g.Stock).Include(g => g.GetGreenhouseCategory).Include(g => g.PacksForGH).FirstOrDefault(g => g.Id == id);
            Stock stock = greenhouse.Stock;
            IEnumerable<Pack> packs = db.Packs.Include(p => p.Stock).Where(p => p.Stock.Id == stock.Id);
            List<Pack> usepacks = new List<Pack>();
            foreach (var p in packs)
            {
                usepacks.Add(p);
            }

            foreach (var p in packs)
            {
                foreach (var pfg in greenhouse.PacksForGH)
                {
                    if (p.Name == pfg.Name)
                    {
                        var pp = usepacks.FirstOrDefault(u => u.Name == p.Name);
                        usepacks.Remove(pp);
                    }
                }
            }
            ViewBag.Categories = db.GreenhouseCategories.Include(s => s.Stock).Where(c => c.Stock.Id == stock.Id).ToList();
            ViewBag.Packs = usepacks;

            return View(greenhouse);
        }

        [HttpPost]
        public ActionResult GreenHouse(int? id, Greenhouse model, int getgreenhousecategory)
        {
            Greenhouse greenhouse = db.Greenhouses.Include(g => g.PacksForGH).Include(g => g.GetGreenhouseCategory).FirstOrDefault(g => g.Id == id);
            GreenhouseCategory category = db.GreenhouseCategories.FirstOrDefault(c => c.Id == getgreenhousecategory);
            greenhouse.Name = model.Name;
            greenhouse.GetGreenhouseCategory = category;
            greenhouse.Bonus = model.Bonus;
            greenhouse.CostPrice = model.CostPrice;
            foreach (var p in greenhouse.PacksForGH)
            {
                foreach (var pp in model.PacksForGH)
                {
                    if (p.Id == pp.Id)
                    {
                        p.Amount = pp.Amount;
                    }
                }
            }
            foreach (var p in model.PacksForGH)
            {
                if (p.Amount == 0)
                {
                    var i = greenhouse.PacksForGH.FirstOrDefault(pf => pf.Id == p.Id);
                    greenhouse.PacksForGH.Remove(i);
                }
            }
            foreach (var p in model.Packs)
            {
                if (p.Amount != 0)
                {
                    Pack pack = db.Packs.FirstOrDefault(pck => pck.Id == p.Id);
                    greenhouse.PacksForGH.Add(new PackForGH { Name = pack.Name, Cost = pack.Cost, Amount = p.Amount });
                }
            }
            db.SaveChanges();           

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult GreenhouseDelete(int? id)
        {
            Greenhouse greenhouse = db.Greenhouses.FirstOrDefault(g => g.Id == id);
            IEnumerable<PackForGH> packs = db.PacksForGh.Where(p => p.GreenHouse.Id == id);
            foreach (var p in packs)
            {
                //var pp = db.PacksForGh.FirstOrDefault(pfg => pfg.Id == p.Id);
                db.PacksForGh.Remove(p);
            }

            db.Greenhouses.Remove(greenhouse);
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Dealers()
        {
            IEnumerable<Dealer> dealers = db.Dealers;

            return View(dealers);
        }

        [HttpGet]
        public ActionResult DealerAdd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DealerAdd(Dealer model)
        {
            Dealer dealer = new Dealer()
            {
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                Debt = model.Debt,
                CostRecommend = model.CostRecommend,
                EnabledForSale = model.EnabledForSale
            };
            db.Dealers.Add(dealer);
            db.SaveChanges();

            return RedirectToAction("Dealers", "Admin");
        }

        [HttpGet]
        public ActionResult Dealer(int? id)
        {
            Dealer dealer = db.Dealers.FirstOrDefault(d => d.Id == id);

            return View(dealer);
        }

        [HttpPost]
        public ActionResult Dealer(int? id, Dealer model)
        {
            Dealer dealer = db.Dealers.FirstOrDefault(d => d.Id == id);
            dealer.Name = model.Name;
            dealer.Phone = model.Phone;
            dealer.Address = model.Address;
            dealer.CostRecommend = model.CostRecommend;
            //dealer.Debt = model.Debt;
            dealer.EnabledForSale = model.EnabledForSale;
            db.SaveChanges();

            return RedirectToAction("Dealers", "Admin");
        }

        [HttpGet]
        public ActionResult DealerAddMoney(int? id)
        {
            Dealer d1 = db.Dealers.FirstOrDefault(d => d.Id == id);

            return View(d1);
        }

        [HttpPost]
        public ActionResult DealerAddMoney(int? id, int cost)
        {
            Dealer d1 = db.Dealers.FirstOrDefault(d => d.Id == id);
            d1.Debt -= cost;

            HistoryMoney hm1 = new HistoryMoney()
            {
                Date = DateTime.UtcNow.AddHours(6),
                Dealer = d1,
                Cost = cost,
                Who = User.Identity.Name
            };
            db.HistoryMoneys.Add(hm1);

            db.SaveChanges();

            return RedirectToAction("Dealers");
        }

        public ActionResult DealerDelete(int? id)
        {
            Dealer dealer = db.Dealers.FirstOrDefault(d => d.Id == id);
            db.Dealers.Remove(dealer);
            db.SaveChanges();

            return RedirectToAction("Dealers", "Admin");
        }

        public ActionResult Realizations(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Buyer)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed == true)
                .OrderByDescending(s => s.Id);
            ViewBag.Id = id;

            return View(sales);
        }

        [HttpGet]
        public ActionResult RealizationsFiltr(int? id, DateTime date1, DateTime date2)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && (s.Date >= date1 && s.Date <= date2) && s.Confirmed == true)
                .OrderByDescending(s => s.Id);
            ViewBag.Id = id;

            return View(sales);
        }

        [HttpGet]
        public ActionResult Realization(int? id)
        {
            Sale sale = db.Sales.FirstOrDefault(s => s.Id == id);

            return View(sale);
        }

        [HttpPost]
        public ActionResult Realization(Sale model)
        {
            Sale sale = db.Sales.Include(d => d.Stock).FirstOrDefault(s => s.Id == model.Id);
            sale.DeliveryCost = model.DeliveryCost;
            sale.Comment = model.Comment;
            sale.PayForTerminal = model.PayForTerminal;

            Sale newSale = new Sale()
            {
                Stock = sale.Stock,
                Date = DateTime.UtcNow.AddHours(6),
                Number = sale.Number,
                Inspect = true,
                Confirmed = true,
                Description = "Изменение способа оплаты"
            };

            db.Sales.Add(newSale);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult RealizationDelete(int? id)
        {
            Sale sale = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.GreenhouseForSales)
                .FirstOrDefault(s => s.Id == id);
            Stock stock = db.Stocks
                .Include(s => s.Packs)
                .FirstOrDefault(s => s.Id == sale.Stock.Id);

            /*чистим бабки
            var infoMoneys = db.InfoMoneys
                .Include(i => i.Sale)
                .Include(i => i.Stock)
                .Where(i => i.Sale.Id == sale.Id && i.Stock.Id == stock.Id)
                .ToList();
            foreach (var im in infoMoneys)
            {
                db.InfoMoneys.Remove(im);
                db.SaveChanges();
            }*/

            //чистим пакеты
            if (sale.Shipment == true)
            {
                foreach (var g in sale.GreenhouseForSales)
                {
                    Greenhouse g1 = db.Greenhouses
                        .Include(gh => gh.PacksForGH)
                        .Include(gh => gh.Stock)
                        .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == sale.Stock.Id);

                    foreach (var p in g1.PacksForGH)
                    {
                        Pack p1 = db.Packs
                            .Include(pck => pck.Stock)
                            .FirstOrDefault(pck => pck.Name == p.Name && pck.Stock.Id == sale.Stock.Id);
                        p1.Amount += 1 * p.Amount * g.Amount;

                        HistoryPack hp1 = new HistoryPack()
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
                        db.HistoryPacks.Add(hp1);
                    }
                }
            }
            db.SaveChanges();
            sale.GreenhouseForSales = null;


            /*IEnumerable<Pack> packss = db.Packs
                  .Include(p => p.Stock)
                  .Where(p => p.Stock.Id == stock.Id);
              ICollection<Pack> packs = new List<Pack>();
              foreach (var p in packss)
              {
                  packs.Add(p);
              }*/
              var needInfoMoney = db.InfoMoneys.Include(s => s.Sale).OrderByDescending(d => d.Date).ToList();

              Sale needSale = db.Sales.Include(p => p.GreenhouseForSales).FirstOrDefault(n => n.Number == sale.Number);
              foreach (var n in needInfoMoney)
              {   
                  if(sale.Date.Second == needSale.Date.Second && needSale.Date.Minute == needSale.Date.Minute && sale.Date.Hour == needSale.Date.Hour && sale.Date.Day == needSale.Date.Day && sale.Date.Month == needSale.Date.Month && sale.Date.Year == needSale.Date.Year)
                  {
                    var InfoMoneys = db.InfoMoneys
                        .Include(i => i.Sale)
                        .Include(s => s.Stock)
                        .Where(h => h.Sale.Id == needSale.Id && h.Stock.Id == stock.Id)
                        .ToList();
                        foreach (var im in InfoMoneys)
                        {
                            db.InfoMoneys.Remove(im);
                            db.SaveChanges();
                        }
                    var DependencySales = db.Sales.Where(s => s.Number == needSale.Number).ToList();
                        foreach(var ds in DependencySales)
                        {                       
                            var HisPacks = db.HistoryPacks.Include(s => s.Sale).Where(h => h.Sale.Id == ds.Id).ToList();
                            foreach (var hp in HisPacks)
                            {
                                hp.Sale = null;
                            }
                            db.SaveChanges();
                            db.Sales.Remove(ds);
                            db.SaveChanges();
                        }
                  }
                  else if (sale.Date.Second == n.Date.Second && sale.Date.Minute == n.Date.Minute && sale.Date.Hour == n.Date.Hour && sale.Date.Day == n.Date.Day && sale.Date.Month == n.Date.Month && sale.Date.Year == n.Date.Year)
                  {
                      InfoMoney test = n;
                      needSale.Remain += sale.AddMoney;
                      db.InfoMoneys.Remove(test);
                      if(sale.Shipment == true) { 
                          foreach (var g in needSale.GreenhouseForSales)
                          {
                              Greenhouse g1 = db.Greenhouses
                                  .Include(gh => gh.PacksForGH)
                                  .Include(gh => gh.Stock)
                                  .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == sale.Stock.Id);

                              foreach (var p in g1.PacksForGH)
                              {
                                  Pack p1 = db.Packs
                                      .Include(pck => pck.Stock)
                                      .FirstOrDefault(pck => pck.Name == p.Name && pck.Stock.Id == sale.Stock.Id);
                                  p1.Amount += 1 * p.Amount * g.Amount;

                                  HistoryPack hp1 = new HistoryPack()
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
                                  db.HistoryPacks.Add(hp1);
                              }
                          }
                          needSale.Shipment = false;
                      }
                    db.Sales.Remove(sale);
                    db.SaveChanges();
                    break;
                  }
              }
              db.SaveChanges();


            /*foreach (var hp in db.HistoryPacks.Where(h => h.Sale.Id == sale.Id))
            {
                hp.Sale = null;
                hp.Stock = null;
                Pack pack = null;
                pack = packs.FirstOrDefault(p => p.Name == hp.Name);
                if (pack != null)
                    pack.Amount -= hp.Amount;
            }
            db.SaveChanges();
            db.Sales.Remove(sale);
            db.SaveChanges();*/
            var HPacks = db.HistoryPacks.Include(s => s.Sale).Where(h => h.Sale.Id == sale.Id).ToList();
            foreach(var hp in HPacks)
            {
                hp.Sale = null;
            }
            db.SaveChanges();

            return RedirectToAction("Realizations", "Admin", new { id = stock.Id });
        }

        /*public ActionResult Outgos(int? id)
        {
            IEnumerable<Sale> sales = db.Sales.Where(s => s.Stock.Id == id && s.Inspect == true && s.Outgo > 0);

            return View(sales);
        }*/

        public ActionResult DealerHistory(int? id)
        {
            Dealer d1 = db.Dealers
                .Include(d => d.HistoryMoneys)
                .FirstOrDefault(d => d.Id == id);

            IEnumerable<HistoryMoney> hm1 = d1.HistoryMoneys;

            return View(hm1);
        }

        public ActionResult DealerRealizations(int? id)
        {
            Dealer d1 = db.Dealers.FirstOrDefault(d => d.Id == id);
            IEnumerable<Sale> sales = db.Sales.Where(s => s.Dealer.Id == d1.Id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed);

            return View(sales);
        }

        public ActionResult HistoryPacks(int? id)
        {
            IEnumerable<HistoryPack> hp1 = db.HistoryPacks.Include(h => h.Stock).Where(h => h.Stock.Id == id && h.Sale == null);

            return View(hp1);
        }

        public ActionResult DealerHistoryDelete(int? id)
        {
            HistoryMoney hm1 = db.HistoryMoneys.Include(h => h.Dealer).FirstOrDefault(h => h.Id == id);
            int dealerId = hm1.Dealer.Id;
            db.HistoryMoneys.Remove(hm1);
            db.SaveChanges();

            return RedirectToAction("DealerHistory", "Admin", new { id = dealerId });
        }

        public ActionResult Statistic()
        {
            IEnumerable<Stock> stocks = db.Stocks;


            IEnumerable<HistoryPack> hpsIE = db.HistoryPacks
                .Include(h => h.Pack)
                .Where(h => h.ForHistory == false);

            ICollection<HistoryPack> hpsIC = new List<HistoryPack>();
            bool x = false;
            foreach (var h in hpsIE)
            {
                foreach (var hp in hpsIC)
                {
                    if (h.Name == hp.Name)
                    {
                        hp.Amount += h.Amount;
                        x = true;
                    }
                }
                if (x == false)
                {
                    hpsIC.Add(h);
                }
                x = false;
            }

            ViewBag.Hps = hpsIC;
            ViewBag.Stocks = stocks;

            return View();
        }

        [HttpGet]
        public ActionResult StatisticFiltr(DateTime? date1, DateTime? date2, int? stockId)
        {
            IEnumerable<HistoryPack> hpsIE = null;

            if (date1 != null && date2 != null && stockId != 0)
            {
                hpsIE = db.HistoryPacks
               .Include(h => h.Pack)
               .Include(h => h.Stock)
               .Where(h => h.Date > date1 && h.Date < date2 && h.ForHistory == false && h.Stock.Id == stockId);
            }
            else if (stockId != 0)
            {
                hpsIE = db.HistoryPacks
                .Include(h => h.Pack)
                .Where(h => h.Stock.Id == stockId && h.ForHistory == false);
            }
            else
            {
                hpsIE = db.HistoryPacks
                .Include(h => h.Pack)
                .Where(h => h.Date > date1 && h.Date < date2 && h.ForHistory == false);
            }

            ICollection<HistoryPack> hpsIC = new List<HistoryPack>();
            bool x = false;
            foreach (var h in hpsIE)
            {
                foreach (var hp in hpsIC)
                {
                    if (h.Name == hp.Name)
                    {
                        hp.Amount += h.Amount;
                        x = true;
                    }
                }
                if (x == false)
                {
                    hpsIC.Add(h);
                }
                x = false;
            }

            ViewBag.Hps = hpsIC;

            return View();
        }

        public ActionResult Montazniks()
        {
            IEnumerable<Montaznik> m1 = db.Montazniks;

            return View(m1);
        }

        [HttpGet]
        public ActionResult AddMontazniks()
        {
            ViewBag.Stocks = db.Stocks.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult AddMontazniks(Montaznik mont, int? stockId)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == stockId);
            Montaznik m1 = new Montaznik()
            {
                FIO = mont.FIO,
                PasportNum = mont.PasportNum,
                WhoIssued = mont.WhoIssued,
                WhereIssued = mont.WhereIssued,
                Address = mont.Address,
                Phone = mont.Phone,
                MarkAuto = mont.MarkAuto,
                NumberAuto = mont.NumberAuto,
                INN = mont.INN,
                Snils = mont.Snils,
                Stock = stock
            };
            db.Montazniks.Add(m1);
            db.SaveChanges();

            return RedirectToAction("Montazniks");
        }

        [HttpGet]
        public ActionResult Montaznik(int? id)
        {
            Montaznik m1 = db.Montazniks.FirstOrDefault(m => m.Id == id);

            return View(m1);
        }

        [HttpPost]
        public ActionResult Montaznik(Montaznik mont, int? stockId)
        {
            Montaznik m1 = db.Montazniks.FirstOrDefault(m => m.Id == mont.Id);
            m1.FIO = mont.FIO;
            m1.MarkAuto = mont.MarkAuto;
            m1.NumberAuto = mont.NumberAuto;
            m1.PasportNum = mont.PasportNum;
            m1.Phone = mont.Phone;
            m1.WhereIssued = mont.WhereIssued;
            m1.WhoIssued = mont.WhoIssued;
            m1.Address = mont.Address;
            m1.INN = mont.INN;
            m1.Snils = mont.Snils;

            if(stockId != null)
            {
                Stock stock = db.Stocks.FirstOrDefault(s => s.Id == stockId);
                m1.Stock = stock;
            }          
            db.SaveChanges();

            return RedirectToAction("Montazniks");
        }

        [HttpGet]
        public ActionResult DeleteMontaznik(int? id)
        {
            Montaznik m1 = db.Montazniks.FirstOrDefault(m => m.Id == id);
            db.Montazniks.Remove(m1);
            db.SaveChanges();

            return RedirectToAction("Montazniks");
        }

        public ActionResult Debtors(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed == true && s.Remain > 0)
                .OrderByDescending(s => s.Id);

            return View(sales);
        }

        public ActionResult BonusDelete(int? id)
        {
            Sale sale = db.Sales
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Id == id);
            sale.SumBonuses = 0;
            db.SaveChanges();

            return RedirectToAction("Realizations", "Admin", new { id = sale.Stock.Id });
        }

        public ActionResult Claims()
        {
            IEnumerable<Claim> claims = db.Claims.Include(c => c.Stock).Where(c => c.Confirm == true).OrderByDescending(c => c.Date);

            return View(claims);
        }

        public ActionResult TargetStocks()
        {
            IEnumerable<Stock> stocks = db.Stocks;

            return View(stocks);
        }

        [HttpGet]
        public ActionResult AddClaim(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);

            /*IEnumerable<Dealer> dealers = db.Dealers;
            ViewBag.Dealers = dealers;
            IEnumerable<Greenhouse> ghs1 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 1).OrderBy(g => g.Position);
            ViewBag.Ghs1 = ghs1;
            IEnumerable<Greenhouse> ghs2 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 2).OrderBy(g => g.Position);
            ViewBag.Ghs2 = ghs2;
            IEnumerable<Greenhouse> ghs3 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 3).OrderBy(g => g.Position);
            ViewBag.Ghs3 = ghs3;
            IEnumerable<Greenhouse> ghs4 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 4).OrderBy(g => g.Position);
            ViewBag.Ghs4 = ghs4;
            IEnumerable<Greenhouse> ghs5 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 5).OrderBy(g => g.Position);
            ViewBag.Ghs5 = ghs5;
            IEnumerable<Greenhouse> ghs6 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 6).OrderBy(g => g.Position);
            ViewBag.Ghs6 = ghs6;
            IEnumerable<Greenhouse> ghs7 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 7).OrderBy(g => g.Position);
            ViewBag.Ghs7 = ghs7;
            IEnumerable<Greenhouse> ghs8 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 8).OrderBy(g => g.Position);
            ViewBag.Ghs8 = ghs8;*/

            ViewBag.Categories = db.GreenhouseCategories.Include(g => g.Greenhouses);

            HttpCookie cookieReq = Request.Cookies["Greenhouses"];
            string str = "";
            ViewBag.ListForRealization = null;
            if (cookieReq != null)
            {
                str = cookieReq["ListForRealization"];
                if (str != "")
                    ViewBag.ListForRealization = str;
            }

            return View(stock);
        }

        [HttpPost]
        public ActionResult AddClaim(int? id, int? dealer, string[] ghName, int[] ghAmount)
        {
            Stock stock = db.Stocks.Include(s => s.Greenhouses).FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;
            Claim claim = null;
            string str = "";
            string strForCookie = "";

            for (var i = 0; i < ghName.Length; i++)
            {
                if (ghAmount[i] > 0)
                {
                    str += ghName[i] + " (" + ghAmount[i] + " шт.); ";
                    strForCookie += $"{ghName[i]}:{ghAmount[i]},";
                }
            }

            HttpCookie cookie = new HttpCookie("Greenhouses");
            cookie["ListForRealization"] = null;
            cookie["ListForRealization"] = strForCookie;
            Response.Cookies.Add(cookie);

            claim = new Claim()
            {
                Stock = stock,
                Confirm = false,
                Date = DateTime.UtcNow.AddHours(6),
                Description = str
            };

            for (var i = 0; i < ghName.Length; i++)
            {
                foreach (var g in stock.Greenhouses)
                {
                    if (g.Name == ghName[i])
                    {
                        if (ghAmount[i] > 0)
                        {
                            g.Amount = ghAmount[i];
                            db.GreenhouseForSales.Add(new GreenhouseForSale
                            {
                                Name = g.Name,
                                Group = g.Group,
                                Claim = claim,
                                Stock = stock,
                                Amount = ghAmount[i],
                                Packs = g.Packs
                            });
                        }
                    }
                }
            }
            db.SaveChanges();

            return RedirectToAction("ContinuationAddClaim", "Admin", new { id = id, idC = claim.Id });
        }

        [HttpGet]
        public ActionResult ContinuationAddClaim(int? id, int? idC)
        {
            Stock stock = db.Stocks.Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            Claim claim = db.Claims
                .Include(s => s.GreenhouseForSales)
                .FirstOrDefault(s => s.Id == idC);
            List<GreenhouseForSale> greenhouses = new List<GreenhouseForSale>();
            List<Greenhouse> ngreenhouses = new List<Greenhouse>();
            List<PackForGH> packs = new List<PackForGH>();
            List<GreenHouseForList> lala = new List<GreenHouseForList>();

            foreach (var gh in claim.GreenhouseForSales)
            {
                greenhouses.Add(gh);
            }
            foreach (var gh in greenhouses)
            {
                Greenhouse g1 = db.Greenhouses.Include(g => g.PacksForGH).FirstOrDefault(g => g.Name == gh.Name && g.Stock.Id == stock.Id);
                ngreenhouses.Add(new Greenhouse()
                {
                    Name = g1.Name,
                    Group = g1.Group,
                    Amount = gh.Amount,
                    PacksForGH = g1.PacksForGH
                });
            }

            ViewBag.Stock = stock;
            return View(claim);
        }

        [HttpPost]
        public ActionResult ContinuationAddClaim(int? id, Claim model)
        {
            Claim claim = db.Claims.FirstOrDefault(c => c.Id == id);
            claim.Confirm = true;
            claim.Address = model.Address;
            claim.Client = model.Client;
            claim.Comment = model.Comment;
            claim.Status = "В работе";
            claim.Text = model.Text;
            claim.Date = DateTime.UtcNow.AddHours(6);
            claim.StockName = "Админ";

            db.SaveChanges();

            HttpCookie cookie = Request.Cookies["Greenhouses"];
            cookie["ListForRealization"] = null;
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult EditClaim(int? id)
        {
            Claim c1 = db.Claims.Include(c => c.Stock).FirstOrDefault(c => c.Id == id);

            return View(c1);
        }

        [HttpPost]
        public ActionResult EditClaim(Claim model)
        {
            Claim c1 = db.Claims.Include(c => c.Stock).FirstOrDefault(c => c.Id == model.Id);
            c1.Status = model.Status;
            c1.Text = model.Text;
            c1.Comment = model.Comment;
            db.SaveChanges();

            return RedirectToAction("Claims");
        }

        [HttpPost]
        public ActionResult DeleteClaim(int? id)
        {
            Claim c1 = db.Claims.Include(c => c.Stock).FirstOrDefault(c => c.Id == id);
            db.Claims.Remove(c1);
            db.SaveChanges();

            return RedirectToAction("Claims");
        }

        public ActionResult ListsPositionForGreenhouse(int? id)
        {
            IEnumerable<Greenhouse> ghs1 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 1).OrderBy(g => g.Position);
            ViewBag.Ghs1 = ghs1;
            IEnumerable<Greenhouse> ghs2 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 2).OrderBy(g => g.Position);
            ViewBag.Ghs2 = ghs2;
            IEnumerable<Greenhouse> ghs3 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 3).OrderBy(g => g.Position);
            ViewBag.Ghs3 = ghs3;
            IEnumerable<Greenhouse> ghs4 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 4).OrderBy(g => g.Position);
            ViewBag.Ghs4 = ghs4;
            IEnumerable<Greenhouse> ghs5 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 5).OrderBy(g => g.Position);
            ViewBag.Ghs5 = ghs5;
            IEnumerable<Greenhouse> ghs6 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 6).OrderBy(g => g.Position);
            ViewBag.Ghs6 = ghs6;
            IEnumerable<Greenhouse> ghs7 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 7).OrderBy(g => g.Position);
            ViewBag.Ghs7 = ghs7;
            IEnumerable<Greenhouse> ghs8 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == id && g.Group == 8).OrderBy(g => g.Position);
            ViewBag.Ghs8 = ghs8;

            return View(id);
        }

        [HttpGet]
        public ActionResult EditPositionForGreenhouse(int? id, int? group)
        {
            IEnumerable<Greenhouse> ghs = db.Greenhouses
                .Include(g => g.Stock)
                .Where(g => g.Stock.Id == id && g.Group == group)
                .OrderBy(g => g.Position);

            ViewBag.Id = id;
            ViewBag.Group = group;

            return View(ghs);
        }

        [HttpPost]
        public ActionResult EditPositionForGreenhouse(int? id, int? group, string[] gh)
        {
            for (var i = 0; i < gh.Length; i++)
            {
                string buf = gh[i];
                Greenhouse gh1 = db.Greenhouses.Include(g => g.Stock)
                    .FirstOrDefault(g => g.Stock.Id == id && g.Group == group && g.Name == buf);
                gh1.Position = i + 1;
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DetailInfo(int? id)
        {
             /*
             decimal sumCash = 0;
             decimal sumTerminal = 0;

             decimal sumPay = 0;
             decimal sumCashment = 0;
             decimal sumDelivery = 0;
             decimal sumProcurement = 0;
             decimal sumChancery = 0;
             decimal sumReturn = 0;
             decimal sumArenda = 0;
             decimal sumOthers = 0;
             

             #region ebanina
             foreach (var im in db.InfoMoneys.Where(i => i.Stock.Id == id && i.PayForTerminal != true && i.Cost > 0))
             {
                 sumCash += im.Cost;
             }

             foreach (var im in db.InfoMoneys.Where(i => i.Stock.Id == id && i.PayForTerminal && i.Cost > 0))
             {
                 sumTerminal += im.Cost;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Зарплата" && i.Confirmed == true))
             {
                 sumPay += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Инкасация" && i.Confirmed == true))
             {
                 sumCashment += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Доставка" && i.Confirmed == true))
             {
                 sumDelivery += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Закуп СПК" && i.Confirmed == true))
             {
                 sumProcurement += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Канцелярия" && i.Confirmed == true))
             {
                 sumChancery += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Возврат денежных средств" && i.Confirmed == true))
             {
                 sumReturn += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Аренда" && i.Confirmed == true))
             {
                 sumArenda += sale.Outgo;
             }

             foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Прочее" && i.Confirmed == true))
             {
                 sumOthers += sale.Outgo;
             }
             #endregion
             ---Старый код*/

            ViewBag.SumCash = db.Sales.Where(s => s.Stock.Id == id && s.PayForTerminal != true && s.Confirmed == true).ToList().Sum(am => am.AddMoney);
            ViewBag.SumTerminal = db.Sales.Where(s => s.Stock.Id == id && s.PayForTerminal == true && s.Confirmed == true).ToList().Sum(amt => amt.AddMoney);
            ViewBag.SumPay = db.Sales.Where(s => s.Stock.Id == id && s.OutgoCategory == "Зарплата" && s.Confirmed == true).ToList().Sum(s => s.Outgo); 
            ViewBag.SumCashment = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Инкасация" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumDelivery = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Доставка" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumProcurement = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Закуп СПК" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumChancery = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Канцелярия" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumReturn = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Возврат денежных средств" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumArenda = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Аренда" && i.Confirmed == true).ToList().Sum(s => s.Outgo);
            ViewBag.SumOthers = db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Прочее" && i.Confirmed == true).ToList().Sum(s => s.Outgo);

            ViewBag.Hui = id;


            ViewBag.Profit = db.Sales.Where(s => s.Stock.Id == id && s.Profit >= 0).Sum(p => p.Profit);

            var a = db.GreenhouseForSales.Where(x => x.Stock.Id == id).Include(s => s.Sale).Where(sale => sale.Sale.Confirmed == true)
                .GroupBy(x => x.Name).Select(x => new
                {
                    Name = x.Key,
                    Objects = x.Select(z => z)
                }).ToList();

            var b = new SortedList<string, int>();

            foreach (var q in a)
            {
                var count = 0;
                foreach (var o in q.Objects)
                {
                    count += o.Amount;
                }
                b.Add(q.Name, count);
            }

            ViewBag.GreenHouseList = b;

            return View();
        }

        [HttpGet]
        public ActionResult DetailFiltr(DateTime? date1, DateTime? date2, int? id)
        {
            decimal sumCash = 0;
            decimal sumTerminal = 0;

            decimal sumPay = 0;
            decimal sumCashment = 0;
            decimal sumDelivery = 0;
            decimal sumProcurement = 0;
            decimal sumChancery = 0;
            decimal sumReturn = 0;
            decimal sumArenda = 0;
            decimal sumOthers = 0;

            #region ebanina

            foreach (var im in db.InfoMoneys.Where(i => i.Stock.Id == id && i.PayForTerminal != true && i.Date >= date1 && i.Date <= date2))
            {
                sumCash += im.Cost;
            }

            foreach (var im in db.InfoMoneys.Where(i => i.Stock.Id == id && i.PayForTerminal && i.Date >= date1 && i.Date <= date2))
            {
                sumTerminal += im.Cost;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Зарплата" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumPay += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Инкасация" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumCashment += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Доставка" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumDelivery += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Закуп СПК" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumProcurement += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Канцелярия" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumChancery += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Возврат денежных средств" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumReturn += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Аренда" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumArenda += sale.Outgo;
            }

            foreach (var sale in db.Sales.Where(i => i.Stock.Id == id && i.OutgoCategory == "Прочее" && i.Confirmed == true && i.Date >= date1 && i.Date <= date2))
            {
                sumOthers += sale.Outgo;
            }
            #endregion

            ViewBag.SumCash = sumCash;
            ViewBag.SumTerminal = sumTerminal;
            ViewBag.SumPay = sumPay;
            ViewBag.SumCashment = sumCashment;
            ViewBag.SumDelivery = sumDelivery;
            ViewBag.SumProcurement = sumProcurement;
            ViewBag.SumChancery = sumChancery;
            ViewBag.SumReturn = sumReturn;
            ViewBag.SumArenda = sumArenda;
            ViewBag.SumOthers = sumOthers;

            ViewBag.Profit = (sumCash + sumTerminal) - (
            sumPay - sumCashment - sumDelivery - sumProcurement
            - sumChancery - sumReturn - sumArenda - sumOthers);

            return View();
        }
        [HttpGet]
        public ActionResult ChangeTrack(int id)
        {
            Stock stock = db.Stocks.FirstOrDefault(i => i.Id == id);
            if(stock.Track == true)
            {
                stock.Track = false;
            }
            else if(stock.Track == false)
            {
                stock.Track = true;
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public ActionResult Categories()
        {
            IEnumerable<GreenhouseCategory> categories = db.GreenhouseCategories.Include(s => s.Stock);

            return View(categories);
        }

        [HttpGet]
        public ActionResult CategoryEdit(int? id)
        {
            ViewBag.Stocks = db.Stocks;
            if (id != null)
            {
                GreenhouseCategory category = db.GreenhouseCategories.Include(s => s.Stock).FirstOrDefault(c => c.Id == id);

                return View(category);
            }
            
            return View();
        }

        [HttpPost]
        public ActionResult CategoryEdit(int? id, string name, int stockId)
        {
            if(id != null)
            {
                GreenhouseCategory category = db.GreenhouseCategories.Include(s => s.Stock).FirstOrDefault(c => c.Id == id);
                Stock categoryStock = db.Stocks.FirstOrDefault(s => s.Id == stockId);

                category.Name = name;
                category.Stock = categoryStock;
                db.SaveChanges();

                return RedirectToAction("Categories", "Admin");
            }
            Stock newCategoryStock = db.Stocks.FirstOrDefault(s => s.Id == stockId);
            GreenhouseCategory newCategory = new GreenhouseCategory()
            {
                Name = name,
                Stock = newCategoryStock
            };

            db.GreenhouseCategories.Add(newCategory);
            db.SaveChanges();

            return RedirectToAction("Categories", "Admin");
        }

        [HttpGet]
        public ActionResult CategoryDelete(int id)
        {
            GreenhouseCategory category = db.GreenhouseCategories.FirstOrDefault(c => c.Id == id);

            IEnumerable<Greenhouse> greenhouse = db.Greenhouses.Where(g => g.GetGreenhouseCategory.Id == category.Id);

            foreach(var gr in greenhouse)
            {
                var green = gr;
                category.Greenhouses.Remove(green);
            }

            db.GreenhouseCategories.Remove(category);
            db.SaveChanges();

            return RedirectToAction("Categories", "Admin");
        }
    }

    public class Info
    {
        public string Name;
        public string Prefix;
        public int Id;
        public decimal? Sum;
        public decimal? Debt;
        public bool Track { get; set; }

        public Info(string a, string b, int c, decimal d, decimal e, bool f)
        {
            Name = a;
            Prefix = b;
            Id = c;
            Sum = d;
            Debt = e;
            Track = f;
        }

        public Info() { }
    }
}