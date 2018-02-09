using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Sklad.Models;

namespace Sklad.Controllers
{
    public class HomeController : Controller
    {
        SkladContext db = new SkladContext();

        public ActionResult Index()
        {
            User user = null;
            user = db.Users.Include(u=>u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if(user != null)
            {
                if (user.Role == "admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == "manager")
                {
                    int id = user.Stock.Id;
                    return RedirectToAction("Info", "Manager", new { id = id });
                }
                else if (user.Role == "production")
                {
                    return RedirectToAction("Index", "Production");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize]
        public ActionResult RealizationStart()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                if (user.Role == "manager") { 
                    return RedirectToAction("RealizationStart", "Manager", new { id = user.Stock.Id });
                }
                else if (user.Role == "production")
                {
                    return RedirectToAction("ShipmentStart", "Production", new { id = 1009 });
                }
            }
            
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Sales()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                return RedirectToAction("Sales", "Manager", new { id = user.Stock.Id });
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Packs()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                if(user.Role == "manager")
                { 
                    return RedirectToAction("Packs", "Manager", new { id = user.Stock.Id });
                }
                else if(user.Role == "production")
                {
                    return RedirectToAction("Materials", "Production", new { id = 1009 });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Info()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {   
                if(user.Role == "manager") { 
                    return RedirectToAction("Info", "Manager", new { id = user.Stock.Id });
                }
                else if(user.Role == "production") {
                    return RedirectToAction("Info", "Production", new { id = 1009 });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Realizations()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                if(user.Role == "manager") { 
                    return RedirectToAction("Realizations", "Manager", new { id = user.Stock.Id });
                }
                else if  (user.Role == "production")
                {
                    return RedirectToAction("Shipments", "Production", new { id = 1009 });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult HistoryPacks()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {   
                if(user.Role == "manager") { 
                    return RedirectToAction("HistoryPacks", "Manager", new { id = user.Stock.Id });
                }
                else if(user.Role == "production") {
                    return RedirectToAction("HistoryMaterials", "Production", new { id = 1009 });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Debtors()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                return RedirectToAction("Debtors", "Manager", new { id = user.Stock.Id });
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Montazniks()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                return RedirectToAction("Montazniks", "Manager", new { id = user.Stock.Id });
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult MontaznikList()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                return RedirectToAction("MontaznikList", "Manager", new { id = user.Stock.Id });
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Claims()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
                return RedirectToAction("Claims", "Manager", new { id = user.Stock.Id });
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult DetailInfo()
        {
            User user = null;
            user = db.Users.Include(u => u.Stock).FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user != null)
                return RedirectToAction("DetailInfo", "Manager", new { id = user.Stock.Id });
            return RedirectToAction("Index", "Home");
        }


    }
}