using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;

namespace TestProject.Controllers
{
    public class HistoryQueryController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        // GET: HistoryQuery
        public ActionResult Index()
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult GetUserData(string AquariumNum)
        {
            //第一個物件
            List<Aquarium> Datalist = db.Aquarium.Where(x => x.AquariumUnitNum == AquariumNum).ToList();
            return Json(Datalist, "text/json", JsonRequestBehavior.AllowGet);
        }
    }
}