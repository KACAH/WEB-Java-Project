using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web.Security;
using Web_Java_Project.Models;

namespace Web_Java_Project.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.LoggedIn = User.Identity.IsAuthenticated;
            if (ViewBag.LoggedIn)
                ViewBag.UserName = User.Identity.Name;

            return View();
        }

        [HttpPost]
        public ActionResult Index(LogOnModel model)
        {
            bool authSuc = AccountController.ValidateLogOn(model);
            if (!authSuc)
            {
                ModelState.AddModelError("", WJP_Resources.Lang.IncorrectLoginOrPass);
            }

            ViewBag.LoggedIn = authSuc;
            ViewBag.UserName = model.UserName;
            return View(model);
        }

        public ActionResult Register()
        {
            return RedirectToAction("Register", "Account");
        }

        public ActionResult Error(string message)
        {
            printLog("ERROR! " + message);
            ViewBag.Message = message;
            return View();
        }
    }
}
