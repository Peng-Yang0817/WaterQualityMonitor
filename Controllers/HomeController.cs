using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;

namespace TestProject.Controllers
{
    public class HomeController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        public ActionResult Index()
        {
            if (Session["UserEmail"] == null)
            {
                ViewBag.UserInit = false;
                return View();
            }
            else
            {
                ViewBag.UserInit = true;
                string Email = Session["UserEmail"].ToString();
                Auth001 UserInfo = db.Auth001.FirstOrDefault(x => x.Email == Email);
                return View(UserInfo);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}