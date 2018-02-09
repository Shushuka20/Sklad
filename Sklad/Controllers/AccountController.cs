using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Sklad.Models;

namespace Sklad.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                using (SkladContext db = new SkladContext())
                {
                    user = db.Users.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);
                }
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Login, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Пользователя с таким логином и паролем нет");
                }
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                using (SkladContext db = new SkladContext())
                {
                    user = db.Users.FirstOrDefault(u => u.Login == model.Login);
                }
                if(user == null)
                {
                    using (SkladContext db = new SkladContext())
                    {
                        db.Users.Add(new User { Login = model.Login, Password = model.Password, Role = "manager" });
                        db.SaveChanges();

                        user = db.Users.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);
                    }
                    if(user != null)
                    {
                        //FormsAuthentication.SetAuthCookie(model.Login, true);
                        return RedirectToAction("Users", "Admin");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь с таким логином или почтой уже существует");
                }
            }

            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}