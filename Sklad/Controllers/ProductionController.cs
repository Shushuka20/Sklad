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
    public class ProductionController : Controller
    {
        SkladContext db = new SkladContext();
        
        public ActionResult Index()
        {
            IEnumerable<Claim> claims = db.Claims
                .Include(c => c.Stock)
                .Where(c => c.Confirm == true)
                .OrderByDescending(s => s.Date);

            return View(claims);
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
            c1.CommentProd = model.CommentProd;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Ticket(int? id)
        {
            Claim claim = db.Claims
                .Include(s => s.GreenhouseForSales)
                .Include(s => s.Stock)
                .FirstOrDefault(s => s.Id == id);

            List<PackForGH> packs = new List<PackForGH>();

            foreach (var g in claim.GreenhouseForSales)
            {
                Greenhouse prod = db.Greenhouses
                    .Include(x => x.PacksForGH)
                    .Include(x => x.Stock)
                    .FirstOrDefault(x => x.Name == g.Name && x.Stock.Id == claim.Stock.Id);

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
                            packs.Add(new PackForGH()
                            {
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

            ViewBag.Packs = packs;

            return View(claim);
        }

        [HttpGet]
        public ActionResult ShipmentStart(int? id)
        {
            Stock stock = db.Stocks.FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;

            IEnumerable<Dealer> dealers = db.Dealers;
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
            ViewBag.Ghs8 = ghs8;

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
        public ActionResult ShipmentStart(int? id, string[] ghName, int[] ghAmount)
        {
            Stock stock = db.Stocks.Include(s => s.Greenhouses).FirstOrDefault(s => s.Id == id);
            ViewBag.ColorS = stock.BackgroundColor;
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


            sale = new Sale()
            {
                Date = DateTime.UtcNow.AddHours(6),
                Stock = stock,
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

            return RedirectToAction("Shipment", "Production", new { id = id, idS = sale.Id });
        }

        [HttpGet]
        public ActionResult Shipments(int? id)
        {
            IEnumerable<Sale> sales = db.Sales
                .Include(s => s.Stock)
                .Include(s => s.Dealer)
                .Where(s => s.Stock.Id == id && (s.SumWithoutD != 0 || s.Inspect == true) && s.Confirmed == true)
                .OrderByDescending(s => s.Id);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;
            ViewBag.Id = id;

            return View(sales);
        }

        [HttpGet]
        public ActionResult Shipment(int? id, int? idS)
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

            ViewBag.Sum = sum;
            ViewBag.Stock = stock;
            ViewBag.GreenHouseList = lala;
            return View(sale);
        }

        [HttpPost]
        public ActionResult Shipment(int? id, string buyer, Sale model)
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

            db.SaveChanges();

            return RedirectToAction("ShipmentConfirm", "Production", new { id = sale.Id });
        }
        [HttpGet]
        public ActionResult ShipmentConfirm(int? id)
        {
            Sale sale = db.Sales.Include(s => s.GreenhouseForSales).FirstOrDefault(s => s.Id == id);

            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                ViewBag.ColorS = user.Stock.BackgroundColor;
            }

            return View(sale);
        }

        [HttpPost]
        public ActionResult ShipmentConfirmM(int? id)
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

            return RedirectToAction("Index", "Production", new { id = sale.Id });
        }

        [HttpGet]
        public ActionResult ShipmentsFiltr(int? id, DateTime date1, DateTime date2)
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

        [HttpGet]
        public ActionResult Materials(int? id)
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
        public ActionResult HistoryMaterials(int? id)
        {
            IEnumerable<HistoryPack> hp1 = db.HistoryPacks
                .Include(h => h.Stock)
                .Include(h => h.Sale)
                .Where(h => h.Stock.Id == id && h.Sale == null);
            ViewBag.ColorS = db.Stocks.FirstOrDefault(s => s.Id == id).BackgroundColor;

            return View(hp1);
        }

        [HttpGet]
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

            return RedirectToAction("Materials", "Production", new { id = 1009 });
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

            return RedirectToAction("Materials", "Production", new { id = 1009 });
        }
    }
}