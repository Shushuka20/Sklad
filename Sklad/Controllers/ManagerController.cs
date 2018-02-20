using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Sklad.Models;
using System.Diagnostics;
using Sklad.Services;

namespace Sklad.Controllers
{
    public class ManagerController : Controller
    {
        SkladContext db = new SkladContext();

        public ActionResult Index()
        {
            IEnumerable<Stock> stocks = db.Stocks;

            return View(stocks);
        }

        public ActionResult Stock(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;

            return View(stock);
        }

        [HttpGet]
        public ActionResult RealizationStart(int? id, bool? admin)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;

            IEnumerable<Dealer> dealers = db.Dealers;
            ViewBag.Dealers = dealers;/*
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

            ViewBag.Categories = db.GreenhouseCategories.Where(c => c.Stock.Id == id).Include(g => g.Greenhouses);

            HttpCookie cookieReq = Request.Cookies["Greenhouses"];
            string str = "";
            ViewBag.ListForRealization = null;
            if (cookieReq != null)
            {
                str = cookieReq["ListForRealization"];
                if(str != "")
                    ViewBag.ListForRealization = str;
            }
            Uri prevUrl = Request.UrlReferrer;
            if(prevUrl.AbsolutePath != "/Manager/Realization/"+ id)
            {
                ViewBag.ListForRealization = null;
            }

            ViewBag.Admin = admin;
            
            return View(stock);
        }



        [HttpPost]
        public ActionResult RealizationStart(int? id, int? dealer, string[] ghName, int[] ghAmount, bool? admin)
        {
            Stock stock = db.Stocks.Include(s => s.Greenhouses).FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;
            Dealer d1 = null;
            Sale sale = null;
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

            if (dealer != null)
            {
                d1 = db.Dealers.FirstOrDefault(d => d.Id == dealer);
            }
            if (d1 != null)
            {
                sale = new Sale()
                {
                    Date = DateTime.UtcNow.AddHours(6),
                    Stock = stock,
                    Dealer = d1,
                    Description = str
                };
            }
            else
            {
                sale = new Sale()
                {
                    Date = DateTime.UtcNow.AddHours(6),
                    Stock = stock,
                    Description = str
                };
            }

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
                                Sale = sale,
                                Stock = stock,
                                Amount = ghAmount[i],
                                Packs = g.Packs
                            });
                        }
                    }
                }
            }
            db.SaveChanges();

            return RedirectToAction("Realization", "Manager", new { id = id, idS = sale.Id, admin = admin});
        }

        [HttpGet]
        public ActionResult Realization(int? id, int? idS, bool? admin)
        {
            Stock stock = db.Stocks.Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;
            stock.Sales++;
            Sale sale = db.Sales.Include(s => s.Dealer).Include(s => s.GreenhouseForSales).FirstOrDefault(s => s.Id == idS);
            decimal sum = 0;
            List<GreenhouseForSale> greenhouses = new List<GreenhouseForSale>();
            List<Greenhouse> ngreenhouses = new List<Greenhouse>();
            List<PackForGH> packs = new List<PackForGH>();
            List<GreenHouseForList> lala = new List<GreenHouseForList>();

            sale.Number = stock.Prefix + stock.Sales.ToString();

            db.SaveChanges();

            foreach (var gh in sale.GreenhouseForSales)
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
            foreach (var gh in ngreenhouses)
            {
                GreenHouseForList ghfl = new GreenHouseForList() { Name = gh.Name, Amount = gh.Amount };
                foreach (var p in gh.PacksForGH)
                {
                    sum = sum + p.Cost * p.Amount * gh.Amount;
                    packs.Add(p);
                    ghfl.Cost += p.Cost * p.Amount * gh.Amount;
                }
                lala.Add(ghfl);
            }

            Dealer d1 = sale.Dealer;

            if (d1 != null)
            {
                if (d1.CostRecommend != null)
                {
                    ViewBag.CostRecommend = d1.CostRecommend.Split('/');
                }
            }
            ViewBag.Admin = "";
            if(admin == true)
            {
                ViewBag.Admin = "ЗАЯВКА С ОФИСА";
            }
            ViewBag.Sum = sum;
            ViewBag.Stock = stock;
            ViewBag.GreenHouseList = lala;
            return View(sale);
        }

        [HttpPost]
        public ActionResult Realization(int? id, string buyer, Sale model)
        {
            Sale sale = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Dealer)
                .FirstOrDefault(s => s.Id == id);

            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == sale.Stock.Id);
            ViewBag.ColorS = stock.BackgroundColor;

            Buyer b1 = new Buyer() { Number = buyer };
            db.Buyers.Add(b1);
            db.SaveChanges();
            sale.Buyer = b1;

            sale.Discount = model.Discount;
            sale.SumWithoutD = model.SumWithoutD;
            sale.AddMoney = model.AddMoney;
            sale.Shipment = model.Shipment;
            sale.PayForTerminal = model.PayForTerminal;
            sale.Delivery = model.Delivery;
            sale.Address = model.Address;
            sale.DeliveryCost = model.DeliveryCost;
            sale.Date = DateTime.UtcNow.AddHours(6);
            sale.Remain = model.SumWithoutD - model.Discount - model.AddMoney;
            sale.SumWithD = model.SumWithoutD - model.Discount;
            sale.Comment = model.Comment;

            /*if(sale.Remain > 0 && sale.Dealer != null)
            {
                Dealer d1 = db.Dealers.FirstOrDefault(d => d.Name == sale.Dealer.Name);
                d1.Debt += sale.Remain;
            }
            
            if(sale.Shipment == true)
            {
                foreach(var g in sale.GreenhouseForSales)
                {
                    Greenhouse g1 = db.Greenhouses
                        .Include(gh=>gh.PacksForGH)
                        .Include(gh=>gh.Stock)
                        .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == stock.Id);

                    foreach(var p in g1.PacksForGH)
                    {
                        Pack p1 = db.Packs
                            .Include(pck=>pck.Stock)
                            .FirstOrDefault(pck => pck.Name == p.Name && pck.Stock.Id == stock.Id);
                        p1.Amount -= 1 * p.Amount * g.Amount;
                    }
                }
            }

            InfoMoney im1 = null;

            im1 = db.InfoMoneys.Include(i => i.Sale).FirstOrDefault(i => i.Sale.Id == sale.Id);

            if(im1 == null)
            {
                im1 = new InfoMoney()
                {
                    Stock = stock,
                    Sale = sale,
                    Cost = model.AddMoney,
                    Date = DateTime.UtcNow.AddHours(6),
                    PayForTerminal = model.PayForTerminal
                };
                db.InfoMoneys.Add(im1);
            }
            else
            {
                im1.Stock = stock;
                im1.Sale = sale;
                im1.Cost = model.AddMoney;
                im1.Date = DateTime.UtcNow.AddHours(6);
                im1.PayForTerminal = model.PayForTerminal;
            }*/

            db.SaveChanges();

            return RedirectToAction("RealizationConfirm", "Manager", new { id = sale.Id });
        }

        [HttpGet]
        public ActionResult RealizationConfirm(int? id)
        {
            Sale sale = db.Sales.Include(s => s.GreenhouseForSales).FirstOrDefault(s => s.Id == id);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                if(user.Stock != null)
                {
                    ViewBag.ColorS = user.Stock.BackgroundColor;
                }
                else
                {
                    ViewBag.ColorS = "black";
                }
            }

            return View(sale);
        }

        [HttpPost]
        public ActionResult RealizationConfirmM(int? id)
        {
            Sale sale = db.Sales
                .Include(s => s.Dealer)
                .Include(s => s.Stock)
                .Include(s => s.GreenhouseForSales)
                .FirstOrDefault(s => s.Id == id);

            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == sale.Stock.Id);

            sale.Confirmed = true;

            //Увеличивает задолжность дилера, если есть необходимость и дилер
            if (sale.Remain > 0 && sale.Dealer != null)
            {
                Dealer d1 = db.Dealers.FirstOrDefault(d => d.Name == sale.Dealer.Name);
                d1.Debt += sale.Remain;
            }

            //Списывает пакеты со склада, если была отгрузка
            if (sale.Shipment == true)
            {
                foreach (var g in sale.GreenhouseForSales)
                {
                    Greenhouse g1 = db.Greenhouses
                        .Include(gh => gh.PacksForGH)
                        .Include(gh => gh.Stock)
                        .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == stock.Id);

                    foreach (var p in g1.PacksForGH)
                    {
                        Pack p1 = db.Packs
                            .Include(pck => pck.Stock)
                            .FirstOrDefault(pck => pck.Name == p.Name && pck.Stock.Id == stock.Id);
                        p1.Amount -= 1 * p.Amount * g.Amount;

                        HistoryPack hp1 = new HistoryPack()
                        {
                            Name = p1.Name,
                            Amount = p.Amount * g.Amount * -1,
                            Date = DateTime.Now,
                            ForHistory = false,
                            Sale = sale,
                            Pack = p1,
                            Stock = stock,
                            Description = sale.Number
                        };
                        db.HistoryPacks.Add(hp1);
                    }
                }
            }

            foreach (var g in sale.GreenhouseForSales)
            {
                Greenhouse g1 = db.Greenhouses
                    .Include(gh => gh.PacksForGH)
                    .Include(gh => gh.Stock)
                    .FirstOrDefault(gh => gh.Name == g.Name && gh.Stock.Id == stock.Id);
                sale.SumBonuses += (g1.Bonus * g.Amount);
                sale.SumCostPrice += (g1.CostPrice * g.Amount);
                sale.Profit = sale.SumWithD - sale.SumCostPrice; 
            }

            InfoMoney im1 = null;

            im1 = db.InfoMoneys.Include(i => i.Sale).FirstOrDefault(i => i.Sale.Id == sale.Id);

            if (im1 == null)
            {
                im1 = new InfoMoney()
                {
                    Stock = stock,
                    Sale = sale,
                    Cost = sale.AddMoney,
                    Date = DateTime.UtcNow.AddHours(6),
                    PayForTerminal = sale.PayForTerminal
                };
                db.InfoMoneys.Add(im1);
            }
            else
            {
                im1.Stock = stock;
                im1.Sale = sale;
                im1.Cost = sale.AddMoney;
                im1.Date = DateTime.UtcNow.AddHours(6);
                im1.PayForTerminal = sale.PayForTerminal;
            }

            db.SaveChanges();

            HttpCookie cookie = Request.Cookies["Greenhouses"];
            cookie["ListForRealization"] = null;
            Response.Cookies.Add(cookie);

            return RedirectToAction("Ticket", "Manager", new { id = sale.Id });
        }

        [HttpGet]
        public ActionResult PaymentStart()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View();
        }

        [HttpGet]
        public ActionResult Payment(string number)
        {
            Sale sale = db.Sales.Include(s => s.Dealer).FirstOrDefault(s => s.Number == number);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(sale);
        }

        [HttpPost]
        public ActionResult Payment(int? id, decimal sum, bool ship, bool payterm, string comment)
        {
            Sale sale = db.Sales
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .FirstOrDefault(s => s.Id == id);
            sale.Remain -= sum;
            Sale s1 = null;

            //не уверен, что нижняя функция нужна заказчику
            Dealer d1 = sale.Dealer;
            if (d1 != null)
            {
                d1.Debt -= sum;
            }

            if (ship == true)
            {
                sale.Shipment = true;

                s1 = new Sale()
                {
                    Number = sale.Number,
                    Date = DateTime.UtcNow.AddHours(6),
                    SumWithD = sale.SumWithD,
                    SumWithoutD = sale.SumWithoutD,
                    AddMoney = sum,
                    Discount = sale.Discount,
                    Shipment = true,
                    Inspect = true,
                    Stock = sale.Stock,
                    PayForTerminal = payterm,
                    Confirmed = true,
                    Comment = comment
                };
            }
            else
            {
                s1 = new Sale()
                {
                    Number = sale.Number,
                    Date = DateTime.UtcNow.AddHours(6),
                    SumWithD = sale.SumWithD,
                    SumWithoutD = sale.SumWithoutD,
                    AddMoney = sum,
                    Discount = sale.Discount,
                    Shipment = false,
                    Inspect = true,
                    Stock = sale.Stock,
                    PayForTerminal = payterm,
                    Confirmed = true,
                    Comment = comment
                };
            }

            db.Sales.Add(s1);

            InfoMoney im1 = new InfoMoney()
            {
                Stock = sale.Stock,
                Sale = sale,
                Cost = sum,
                Date = DateTime.UtcNow.AddHours(6),
                PayForTerminal = payterm
            };

            if (ship == true)
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
                        p1.Amount -= 1 * p.Amount * g.Amount;
                    }
                }
            }

            db.InfoMoneys.Add(im1);
            db.SaveChanges();

            return RedirectToAction("Ticket", "Manager", new { id = sale.Id, comm = comment });
        }

        public ActionResult Sales(int? id)
        {
            IEnumerable<Sale> sales = db.Sales.Include(s => s.Stock).Where(s => s.Stock.Id == id && s.Inspect != true && s.SumWithoutD != 0 && s.Confirmed == true);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;

            return View(sales);
        }

        [HttpGet]
        public ActionResult Sale(int? id)
        {
            Sale sale = db.Sales.FirstOrDefault(s => s.Id == id);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(sale);
        }

        [HttpPost]
        public ActionResult Sale(int? id, string outgoD, int outgo, string outgoCategory)
        {
            Sale sale = db.Sales.Include(s => s.Stock).FirstOrDefault(s => s.Id == id);
            sale.Outgo = outgo;
            sale.OutgoDescription = outgoD;
            sale.Inspect = true;
            sale.OutgoCategory = outgoCategory;

            InfoMoney im1 = new InfoMoney()
            {
                Stock = sale.Stock,
                Sale = sale,
                Cost = 0 - outgo,
                Date = DateTime.UtcNow.AddHours(6)
            };
            db.InfoMoneys.Add(im1);

            db.SaveChanges();

            return RedirectToAction("Sales", "Manager", new { id = sale.Stock.Id });
        }

        [HttpGet]
        public ActionResult SaleAdd()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View();
        }

        [HttpPost]
        public ActionResult SaleAdd(string outgoD, int outgo, string outgoCategory)
        {
            User user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == user.Stock.Id);
            stock.Sales++;

            Sale s1 = new Sale()
            {
                Number = stock.Prefix + stock.Sales.ToString(),
                Outgo = outgo,
                OutgoDescription = outgoD,
                Stock = stock,
                Date = DateTime.UtcNow.AddHours(6),
                Inspect = true,
                Confirmed = true,
                OutgoCategory = outgoCategory
            };
            db.Sales.Add(s1);

            InfoMoney im1 = new InfoMoney()
            {
                Stock = stock,
                Sale = s1,
                Cost = 0 - outgo,
                Date = DateTime.UtcNow.AddHours(6)
            };
            db.InfoMoneys.Add(im1);

            db.SaveChanges();

            return RedirectToAction("Sales", "Manager", new { id = stock.Id });
        }

        public ActionResult Packs(int? id)
        {
            IEnumerable<Pack> packs = db.Packs.Include(p => p.Stock).Where(p => p.Stock.Id == id).OrderBy(p => p.Group);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;

            HttpCookie cookieReq = Request.Cookies["Packs"];
            int packId = 0;
            if (cookieReq != null)
            {
                packId = Convert.ToInt32(cookieReq["id"]);
            }
            ViewBag.PackId = packId;

            return View(packs);
        }

        [HttpGet]
        public ActionResult AddPacks(int? id)
        {
            Pack pack = db.Packs.FirstOrDefault(p => p.Id == id);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(pack);
        }

        [HttpPost]
        public ActionResult AddPacks(int? id, decimal amount, string description)
        {
            Pack pack = db.Packs.Include(p => p.Stock).FirstOrDefault(p => p.Id == id);
            pack.Amount += amount;

            HistoryPack hp1 = new HistoryPack()
            {
                Amount = 0 + amount,
                Name = pack.Name,
                Date = DateTime.UtcNow.AddHours(6),
                Pack = pack,
                Stock = pack.Stock,
                Description = description
            };
            db.HistoryPacks.Add(hp1);

            db.SaveChanges();

            HttpCookie cookie = new HttpCookie("Packs");
            cookie["id"] = Convert.ToString(pack.Id);
            Response.Cookies.Add(cookie);

            return RedirectToAction("Packs", "Manager", new { id = pack.Stock.Id });
        }

        [HttpGet]
        public ActionResult DeletePacks(int? id)
        {
            Pack pack = db.Packs.FirstOrDefault(p => p.Id == id);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(pack);
        }

        [HttpPost]
        public ActionResult DeletePacks(int? id, decimal amount, string description)
        {
            Pack pack = db.Packs.Include(p => p.Stock).FirstOrDefault(p => p.Id == id);
            pack.Amount -= amount;

            HistoryPack hp1 = new HistoryPack()
            {
                Amount = 0 - amount,
                Name = pack.Name,
                Date = DateTime.UtcNow.AddHours(6),
                Pack = pack,
                Stock = pack.Stock,
                Description = description
            };
            db.HistoryPacks.Add(hp1);
            db.SaveChanges();

            return RedirectToAction("Packs", "Manager", new { id = pack.Stock.Id });
        }

        /*public ActionResult Outgos(int? id)
        {
            IEnumerable<Sale> sales = db.Sales.Where(s => s.Stock.Id == id && s.Inspect == true);

            return View(sales);
        }*/

        public ActionResult Info(int? id)
        {
            Stock stock = db.Stocks
                .Include(s => s.Packs)
                .FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;

            decimal sum = 0;
            decimal debt = 0;

            foreach (var im in db.InfoMoneys.Where(i => i.Stock.Id == stock.Id && i.PayForTerminal != true))
            {
                sum += im.Cost;
            }

            /*foreach(var s in db.Sales.Where(s => s.Stock.Id == stock.Id && s.PayForTerminal != true))
            {
                sum += s.AddMoney;
            }
            foreach(var s in db.Sales.Where(s=>s.Stock.Id==stock.Id && s.Inspect==true && s.Outgo > 0 && s.PayForTerminal != true))
            {
                sum -= s.Outgo;
            }*/
            foreach (var s in db.Sales.Where(s => s.Stock.Id == stock.Id && s.Dealer == null && s.Remain > 0 && s.Confirmed == true))
            {
                debt += s.Remain;
            }

            ViewBag.Sum = sum;
            ViewBag.Debt = debt;

            return View(stock);
        }

        [HttpGet]
        public ActionResult DealerAddMoney()
        {
            IEnumerable<Dealer> dealers = db.Dealers;

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(dealers);
        }

        [HttpPost]
        public ActionResult DealerAddMoney(string dealer, decimal cost)
        {
            Dealer d1 = db.Dealers.FirstOrDefault(d => d.Name == dealer);
            d1.Debt -= cost;
            User user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == user.Stock.Id);

            HistoryMoney hm1 = new HistoryMoney()
            {
                Date = DateTime.UtcNow.AddHours(6),
                Dealer = d1,
                Cost = cost,
                Who = User.Identity.Name
            };

            InfoMoney im1 = new InfoMoney()
            {
                Stock = stock,
                Cost = cost,
                Date = DateTime.UtcNow.AddHours(6),
                Dealer = d1
            };

            Sale s1 = new Sale()
            {
                Stock = stock,
                AddMoney = cost,
                Date = DateTime.UtcNow.AddHours(6),
                Dealer = d1,
                Inspect = true,
                Confirmed = true
            };

            db.Sales.Add(s1);
            db.InfoMoneys.Add(im1);
            db.HistoryMoneys.Add(hm1);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Realizations(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed == true)
                .OrderByDescending(s => s.Id);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;
            ViewBag.Id = id;

            /*foreach(var s in sales)
            {
                s.Confirmed = true;
            }
            db.SaveChanges();*/

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
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;
            ViewBag.Id = id;

            return View(sales);
        }

        public ActionResult HistoryPacks(int? id)
        {
            IEnumerable<HistoryPack> hp1 = db.HistoryPacks
                .Include(h => h.Stock)
                .Include(h => h.Sale)
                .Where(h => h.Stock.Id == id && h.Sale == null);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;

            return View(hp1);
        }

        public ActionResult StocksInfo()
        {
            IEnumerable<Stock> s1 = db.Stocks;

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(s1);
        }

        public ActionResult StockInfo(int? id)
        {
            Stock s1 = db.Stocks.FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = s1.BackgroundColor;

            return View(s1);
        }

        public ActionResult StockInfoPacks(int? id)
        {
            IEnumerable<Pack> packs = db.Packs.Include(p => p.Stock).Where(p => p.Stock.Id == id);

            return View(packs);
        }

        public ActionResult StockInfoOutgos(int? id)
        {
            IEnumerable<Sale> sales = db.Sales.Where(s => s.Stock.Id == id && s.Inspect == true);

            return View(sales);
        }

        public ActionResult StockInfoRealiz(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id);

            return View(sales);
        }

        public ActionResult StockInfoHistory(int? id)
        {
            IEnumerable<HistoryPack> hp1 = db.HistoryPacks.Include(h => h.Stock).Where(h => h.Stock.Id == id);

            return View(hp1);
        }

        public ActionResult Ticket(int? id, string comm, int? sum )
        {
            Sale sale = db.Sales
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Id == id);

            InfoMoney im1 = null;
            try
            {
                im1 = db.InfoMoneys.Where(i => i.Sale.Id == sale.Id).OrderByDescending(i => i.Date).First();
            }
            catch
            {
                Sale sl = db.Sales.Where(x => x.Number == sale.Number).OrderBy(x => x.Date).First();
                string com = sale.Comment;
                //var am = sale.AddMoney;
                sale = db.Sales
                    .Include(s => s.GreenhouseForSales)
                    .Include(s => s.Stock)
                    .FirstOrDefault(s => s.Id == sl.Id);
                sale.Comment = com;
                //sale.AddMoney = am;

                im1 = db.InfoMoneys.Where(i => i.Sale.Id == sale.Id).OrderByDescending(i => i.Date).First();
            }
            if (comm != null)
            {
                sale.Comment = comm;
            }

            ViewBag.AddMoney = im1.Cost;

            List<PackForGH> packs = new List<PackForGH>();

            foreach (var g in sale.GreenhouseForSales)
            {
                Greenhouse prod = db.Greenhouses
                    .Include(x => x.PacksForGH)
                    .Include(x => x.Stock)
                    .FirstOrDefault(x => x.Name == g.Name && x.Stock.Id == sale.Stock.Id);

                foreach (var p in prod.PacksForGH)
                {
                    decimal buf = p.Amount;
                    p.Amount = p.Amount * g.Amount;

                    if (p.Amount > 0)
                    {
                        PackForGH pfg = null;
                        pfg = packs.FirstOrDefault(x => x.Name == p.Name);
                        if (pfg == null)
                        {
                            packs.Add(new PackForGH() {
                                Name = p.Name,
                                Amount = p.Amount,
                                Cost = p.Cost,
                                GreenHouse = p.GreenHouse
                            });
                        }
                        else
                        {
                            pfg.Amount += p.Amount;
                        }
                    }

                    p.Amount = buf;
                }
            }

            if(sum != null)
            {
                ViewBag.Sum = sum;

                Installment installment = db.Installments.FirstOrDefault(i => i.Sale.Id == sale.Id);

                ViewBag.Adress = installment.Adress;
                ViewBag.Phone = installment.Phone;
                ViewBag.Comment = installment.Comment;
            }            
            ViewBag.Packs = packs;

            return View(sale);
        }

        [HttpGet]
        public ActionResult Montazniks(int? id)
        {
            IEnumerable<Montaznik> m1 = db.Montazniks
                .Include(x => x.Stock)
                .Where(x => x.Stock.Id == id);

            return View(m1);
        }

        [HttpGet]
        public ActionResult AddMontazniks()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddMontaznik(Montaznik mont)
        {
            User user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);

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
                Stock = user.Stock
            };
            db.Montazniks.Add(m1);
            db.SaveChanges();

            return RedirectToAction("Montazniks", "Home");
        }

        [HttpGet]
        public ActionResult Montaznik(int? id)
        {
            Montaznik m1 = db.Montazniks.FirstOrDefault(m => m.Id == id);

            return View(m1);
        }

        [HttpPost]
        public ActionResult Montaznik(Montaznik mont)
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
            db.SaveChanges();

            return RedirectToAction("Montazniks", "Home");
        }

        [HttpGet]
        public ActionResult Delivery(string sellNumb)
        {
            User user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);

            IEnumerable<Montaznik> m1 = db.Montazniks
                .Include(x => x.Stock)
                .Where(x => x.Stock.Id == user.Stock.Id);
            ViewBag.Mont = m1;

            ViewBag.SellNumb = null;
            ViewBag.Shipment = null;
            if (sellNumb != null)
            {
                ViewBag.SellNumb = sellNumb;

                ViewBag.Shipment = db.Sales.FirstOrDefault(x => x.Number == sellNumb).Shipment;
            }

            return View();
        }

        [HttpGet]
        public ActionResult DeliveryDoc(string sellNumb, int montId, bool mont, bool deliv, bool? shipment)
        {
            ViewBag.SellNumb = sellNumb;
            Montaznik m1 = db.Montazniks.FirstOrDefault(m => m.Id == montId);
            ViewBag.Montaznik = m1;
            ViewBag.Mont = mont;
            ViewBag.Deliv = deliv;
            ViewBag.Date = DateTime.Now.ToString("dd.MM.yyyy");

            Sale sale = db.Sales
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Number == sellNumb);

            List<Greenhouse> ghfs = new List<Greenhouse>();
            foreach (var g in sale.GreenhouseForSales)
            {
                Greenhouse g1 = db.Greenhouses
                    .Include(x => x.PacksForGH)
                    .Include(x => x.Stock)
                    .FirstOrDefault(x => x.Name == g.Name && x.Stock.Id == sale.Stock.Id);
                g1.Amount = g.Amount;
                ghfs.Add(g1);
            }

            ViewBag.Ghfs = ghfs;

            HttpCookie cookie = new HttpCookie("DogHran");
            cookie["sellnumb"] = sellNumb;
            cookie["montId"] = Convert.ToString(montId);
            Response.Cookies.Add(cookie);

            sale.Montaznik = m1;
            sale.DeliveryConfirm = false;

            Sale s1 = new Models.Sale()
            {
                Number = sale.Number,
                AddMoney = sale.AddMoney,
                Confirmed = sale.Confirmed,
                Date = sale.Date,
                Remain = sale.Remain,
                Comment = "Монтажник: " + m1.FIO + ".",
                Stock = sale.Stock,
                Montaznik = m1,
                Description = sale.Description,
                SumWithD = sale.SumWithD,
                SumWithoutD = sale.SumWithoutD,
                Inspect = sale.Inspect
            };

            if(shipment != null)
                if (shipment == true)
                {
                    sale.Shipment = true;
                    s1.Shipment = true;
                }


            db.Sales.Add(s1);
            db.SaveChanges();

            return View(sale);
        }

        [HttpGet]
        public ActionResult DogHran(string sellNumb)
        {
            ViewBag.SellNumb = null;
            if (sellNumb != null)
                ViewBag.SellNumb = sellNumb;

            return View();
        }

        [HttpGet]
        public ActionResult DogHranPech(string sellNumb, string FIO, string address, string phone)
        {
            ViewBag.SellNumb = sellNumb;
            ViewBag.FIO = FIO;
            ViewBag.Address = address;
            ViewBag.Phone = phone;

            Sale s1 = db.Sales
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Number == sellNumb);
            List<Greenhouse> ghfs = new List<Greenhouse>();
            foreach (var g in s1.GreenhouseForSales)
            {
                Greenhouse g1 = db.Greenhouses
                    .Include(x => x.PacksForGH)
                    .Include(x => x.Stock)
                    .FirstOrDefault(x => x.Name == g.Name && x.Stock.Id == s1.Stock.Id);
                g1.Amount = g.Amount;
                ghfs.Add(g1);
            }

            ViewBag.Ghfs = ghfs;

            return View(s1);
        }

        [HttpGet]
        public ActionResult Act1()
        {
            HttpCookie cookieReq = Request.Cookies["DogHran"];
            string sellNumb = null;
            int montId;
            Montaznik m1 = null;
            if (cookieReq != null)
            {
                sellNumb = cookieReq["sellNumb"];
                montId = Convert.ToInt32(cookieReq["montId"]);

                m1 = db.Montazniks.FirstOrDefault(m => m.Id == montId);
            }

            ViewBag.SellNumb = sellNumb;
            ViewBag.Mont = m1;
            return View();
        }

        public ActionResult Debtors(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed == true && s.Remain > 0)
                .OrderByDescending(s => s.Id);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;
            ViewBag.Id = id;

            return View(sales);
        }

        public ActionResult MontaznikList(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(x => x.Stock)
                .Include(x => x.Montaznik)
                .Where(x => x.Stock.Id == id && x.Montaznik != null && x.DeliveryConfirm == false);

            return View(sales);
        }

        [HttpGet]
        public ActionResult ReceivingMoneyStart()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ReceivingMonet(string number)
        {
            Sale sale = db.Sales
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Number == number);

            return View(sale);
        }

        [HttpPost]
        public ActionResult ReceivingMonet(int? id, decimal sum, int zp)
        {
            Sale sale = db.Sales
                .Include(x => x.Stock)
                .FirstOrDefault(x => x.Id == id);
            //sale.Remain -= sum;
            sale.DeliveryConfirm = true;

            Installment installment = db.Installments.Include(i => i.Sale).FirstOrDefault(i => i.Sale.Number == sale.Number);
            if(installment != null)
            {
                installment.Color = "green";
            }          
            IEnumerable<Sale> sales = db.Sales.Where(s => s.Number == sale.Number);
            foreach (var s in sales)
            {
                s.Remain -= sum;
            }

            Sale s1 = new Sale()
            {
                Number = sale.Number,
                Date = DateTime.UtcNow.AddHours(6),
                SumWithD = sale.SumWithD,
                SumWithoutD = sale.SumWithoutD,
                AddMoney = sum,
                Discount = sale.Discount,
                Shipment = sale.Shipment,
                Inspect = true,
                Stock = sale.Stock,
                Confirmed = true,
                Outgo = zp,
                Comment = "Приём денег."
            };

            InfoMoney im1 = new InfoMoney()
            {
                Stock = sale.Stock,
                Sale = sale,
                Cost = sum - zp,
                Date = DateTime.UtcNow.AddHours(6)
            };

            db.Sales.Add(s1);
            db.InfoMoneys.Add(im1);
            db.SaveChanges();


            return RedirectToAction("Index", "Home");
        }

        public ActionResult Claims(int? id)
        {
            IEnumerable<Claim> claims = db.Claims
                .Include(c => c.Stock)
                .Where(c => c.Stock.Id == id && c.Confirm == true)
                .OrderByDescending(c => c.Date);
            ViewBag.Id = id;

            return View(claims);
        }

        [HttpGet]
        public ActionResult AddClaim(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;

            IEnumerable<Dealer> dealers = db.Dealers;
            ViewBag.Dealers = dealers;
            /*IEnumerable<Greenhouse> ghs1 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 1).OrderBy(g => g.Position);
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

            return RedirectToAction("ContinuationAddClaim", "Manager", new { id = id, idC = claim.Id });
        }

        [HttpGet]
        public ActionResult ContinuationAddClaim(int? id, int? idC)
        {
            Stock stock = db.Stocks.Include(s => s.Packs).FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;
            Claim claim = db.Claims
                .Include(s => s.GreenhouseForSales)
                .FirstOrDefault(s => s.Id == idC);
            List<GreenhouseForSale> greenhouses = new List<GreenhouseForSale>();
            List<Greenhouse> ngreenhouses = new List<Greenhouse>();
            List<PackForGH> packs = new List<PackForGH>();
            List<GreenHouseForList> lala = new List<GreenHouseForList>();

            //db.SaveChanges();

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
            Claim claim = db.Claims.Include(c=>c.Stock).FirstOrDefault(c => c.Id == id);
            claim.Confirm = true;
            claim.Address = model.Address;
            claim.Client = model.Client;
            claim.Comment = model.Comment;
            claim.Status = "В работе";
            claim.Text = model.Text;
            claim.Date = DateTime.UtcNow.AddHours(6);
            claim.StockName = claim.Stock.Name;

            db.SaveChanges();

            HttpCookie cookie = Request.Cookies["Greenhouses"];
            cookie["ListForRealization"] = null;
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult ChangeStatus(int id)
        {
            Sale sale = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.GreenhouseForSales)
                .FirstOrDefault(i => i.Id == id);
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
                            Sale = sale,
                            Pack = p1,
                            Stock = sale.Stock,
                            Description = sale.Number
                        };
                        db.HistoryPacks.Add(hp1);
                    }
                }
                sale.Shipment = false;
            }
            else
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
                        p1.Amount -= 1 * p.Amount * g.Amount;

                        HistoryPack hp1 = new HistoryPack()
                        {
                            Name = p1.Name,
                            Amount = p.Amount * g.Amount * -1,
                            Date = DateTime.Now,
                            ForHistory = false,
                            Sale = sale,
                            Pack = p1,
                            Stock = sale.Stock,
                            Description = sale.Number
                        };
                        db.HistoryPacks.Add(hp1);
                    }
                }
                sale.Shipment = true;
            }

            Sale s1 = new Sale
            {
                Stock = sale.Stock,
                Date = DateTime.UtcNow.AddHours(6),
                Number = sale.Number,
                Shipment = sale.Shipment,
                Inspect = true,
                Confirmed = true,
                Description = "Изменение статуса отгрузки"
            };

            db.Sales.Add(s1);
            db.SaveChanges();

            

            return RedirectToAction("Realizations", "Home");
        }

        [HttpGet]
        public ActionResult EditClaim(int? id)
        {
            Claim claim = db.Claims.Include(s => s.Stock).FirstOrDefault(i => i.Id == id);
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == claim.Stock.Id);

            /*IEnumerable<Greenhouse> ghs1 = db.Greenhouses.Include(g => g.Stock).Where(g => g.Stock.Id == stock.Id && g.Group == 1).OrderBy(g => g.Position);
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

            Dictionary<string, decimal> productWithCount = new Dictionary<string, decimal>();

            if (claim.Description != null)
            {
                string desc = claim.Description;
                string[] productsWithCount = desc.Split(';');
                for (var i = 0; i < productsWithCount.Length - 1; i++)
                {
                    string[] buf = productsWithCount[i].Split('(');
                    buf[1] = buf[1].Replace("шт.)", "");
                    productWithCount.Add(buf[0], Decimal.Parse(buf[1]));
                }
                ViewBag.Descriptions = productWithCount;
            }
            else
                ViewBag.Descriptions = null;

            return View(claim);
        }

        [HttpPost]
        public ActionResult EditClaim(Claim model, string[]ghName, int[] ghAmount)
        {
            Claim claim = db.Claims.FirstOrDefault(i => i.Id == model.Id);
            ClaimService claimService = new ClaimService();

            var str = "";

            for (var i = 0; i < ghName.Length; i++)
            {
                if (ghAmount[i] > 0)
                {
                    str += ghName[i] + "(" + ghAmount[i] + " шт.); ";
                }
            }

            if (claim.Client != model.Client)
            {
                claim.Client = claimService.AddStateForEditProperty(model.Client);
                claim.Edited = true;
            }
            if (claim.Address != model.Address)
            {
                claim.Address = claimService.AddStateForEditProperty(model.Address);
                claim.Edited = true;
            }
            if (claim.Text != model.Text)
            {
                claim.Text = claimService.AddStateForEditProperty(model.Text);
                claim.Edited = true;
            }
            if (claim.Comment != model.Comment)
            {
                claim.Comment = claimService.AddStateForEditProperty(model.Comment);
                claim.Edited = true;
            }
            if (claim.Description != str)
            {
                claim.Description = claimService.AddStateForEditProperty(str);
                claim.Edited = true;
            }

            db.SaveChanges();

            return RedirectToAction("Claims", "Home");
        }

        [HttpGet]
        public ActionResult DogInstallmentStart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DogInstallmentStart(string sellNumb)
        {
            return RedirectToAction("DogInstallment", "Manager", new { sellNumb });
        }

        [HttpGet]
        public ActionResult DogInstallment(string sellNumb)
        {
            ViewBag.SellNumb = sellNumb;
            ViewBag.Sum = null;
            ViewBag.AddMoney = null;
            if (sellNumb != null)

            {
                Sale sale = db.Sales.FirstOrDefault(s => s.Number == sellNumb);
                ViewBag.SellNumb = sellNumb;
                ViewBag.Sum = sale.SumWithD;
                ViewBag.AddMoney = sale.AddMoney;
            }

            return View();
        }

        [HttpGet]
        public ActionResult DogInstallmentForm(string sellNumb, string FIO, string Passport_number,
            string issuedBy, DateTime issuedDate, string adress, string privatePhone,
            string homePhone, DateTime date,  decimal sum, decimal addedMoney,
            DateTime[] installmentDate, decimal[] installmentMoney)
        {
            Sale saleRemain = db.Sales.Include(s => s.Stock).FirstOrDefault(s => s.Number == sellNumb);
            Sale sale = new Sale
            {
                Stock = saleRemain.Stock,
                Number = sellNumb,
                Comment = "РАССРОЧКА",
                Date = date,
                Confirmed = true,
                Inspect = true
            };

            decimal remain = saleRemain.Remain;
            var saleDate = saleRemain.Date;

            db.Sales.Add(sale);
            db.SaveChanges();

            ViewBag.sellNumb = sellNumb;
            ViewBag.saleDate = saleDate;
            ViewBag.passport_number = Passport_number;
            ViewBag.FIO = FIO;
            ViewBag.issuedBy = issuedBy;
            ViewBag.issuedDate = issuedDate.ToLongDateString();
            ViewBag.sum = sum;
            ViewBag.addedMoney = addedMoney;
            ViewBag.remain = remain;
            ViewBag.installmentLastDate = installmentDate.Last();
            ViewBag.installmentDate = installmentDate;
            ViewBag.installmentMoney = installmentMoney;
            ViewBag.private_phone = privatePhone;
            ViewBag.home_phone = homePhone;
            ViewBag.adress = adress;

            return View();
        }

        [HttpGet]
        public ActionResult OrderInstallation(int? id)
        {
            IEnumerable<Installment> installments = db.Installments.Include(i => i.Sale).Include(m => m.Montazniks);
            IEnumerable<Installment> installmentsNotGreen = installments.Where(i => i.Color != "green").OrderBy(d => d.ForDate);
            IEnumerable<Installment> installmentsGreen = installments.Where(i => i.Color == "green");

            installments = installmentsNotGreen.Concat(installmentsGreen);

            return View(installments);
        }
        
        [HttpGet]
        public ActionResult OrderInstallationStart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult OrderInstallmentStart(string sellNumb)
        {
            return RedirectToAction("OrderInstallmentFinal", "Manager", new { sellNumb });
        }

        [HttpGet]
        public ActionResult OrderInstallmentFinal(string sellNumb)
        {
            Sale sale = db.Sales.FirstOrDefault(s => s.Number == sellNumb);
            ViewBag.Montazniks = db.Montazniks.ToList();

            return View(sale);
        }

        [HttpPost]
        public ActionResult OrderInstallmentFinal(string sellNumb, string adress, string phone, DateTime datefrom, DateTime datefor, bool light, string comment)
        {
            Sale sale = db.Sales.FirstOrDefault(s => s.Number == sellNumb);
            List<Montaznik> montazniksList = new List<Montaznik>();

            /*foreach(var i in montazniksIds)
            {
                Montaznik montaznik = db.Montazniks.FirstOrDefault(m => m.Id == i);
                montazniksList.Add(montaznik);
            }*/

            Installment installment = new Installment()
            {
                Adress = adress,
                Phone = phone,
                FromDate = datefrom,
                ForDate = datefor,
                Light = light,
                Comment = comment,
                Sale = sale,
                Montazniks = montazniksList,
                Color = "white"
            };
            db.Installments.Add(installment);
            db.SaveChanges();

            return RedirectToAction("OrderInstallation", "Manager");
        }

        [HttpGet]
        public ActionResult InstallationEdit(int? id)
        {
            Installment installment = db.Installments.Include(s => s.Sale).Include(s => s.Sale.Stock).Include(m => m.Montazniks).FirstOrDefault(i => i.Id == id);
            ViewBag.Montazniks = db.Montazniks.Where(m => m.Stock.Id == installment.Sale.Stock.Id).ToList();

            return View(installment);
        }
        [HttpPost]
        public ActionResult InstallationEdit(int id, string adress, string phone, DateTime datefrom, DateTime datefor, bool light, ICollection<int> montazniksIds, string comment)
        {
            Installment installment = db.Installments.FirstOrDefault(i => i.Id == id);

            installment.Montazniks.Clear();

            if (montazniksIds != null)
            {
                foreach (var i in db.Montazniks.Where(m => montazniksIds.Contains(m.Id)))
                {
                    installment.Montazniks.Add(i);
                }                
            }
            
            installment.Adress = adress;
            installment.Phone = phone;
            installment.FromDate = datefrom;
            installment.ForDate = datefor;
            installment.Light = light;
            installment.Comment = comment;
            db.Entry(installment).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("OrderInstallation", "Manager");
        }

        [HttpGet]
        public ActionResult ChangeColor(int? id, string color)
        {
            Installment installment = db.Installments.FirstOrDefault(i => i.Id == id);

            installment.Color = color;
            db.SaveChanges();

            return RedirectToAction("OrderInstallation", "Manager");
        }
        [HttpGet]
        public ActionResult InstallationSum(string sellNumb)
        {
            Sale sale = db.Sales.FirstOrDefault(s => s.Number == sellNumb);

            ViewBag.Remain = sale.Remain;
            ViewBag.Number = sellNumb;

            Sale saleDolg = db.Sales.Where(s => s.Number == sellNumb).FirstOrDefault(s => s.Comment == "РАССРОЧКА");
            ViewBag.saleDolg = "";
            if(saleDolg != null)
            {
                ViewBag.saleDolg = "Клиент с рассрочкой";
            }


            return View(sale);
        }

    }

    public class GreenHouseForList
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal Cost { get; set; }

        public GreenHouseForList(string _name, int _amount, int _cost)
        {
            Name = _name;
            Amount = _amount;
            Cost = _cost;
        }

        public GreenHouseForList() { }
    }

   

}