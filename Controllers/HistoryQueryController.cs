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
            List<string[]> data = new List<string[]>();
            for (int i = 0; i < Datalist.Count; i++)
            {
                string bindtag = "";
                if (Datalist[i].BindTag.Equals("1"))
                {
                    bindtag = "解綁";
                }
                else
                {
                    bindtag = "使用中";
                }
                string[] item = new string[] { Datalist[i].createTime.ToString(),
                                                Datalist[i].modifyTime.ToString(),
                                                bindtag };
                data.Add(item);
            }
            return Json(data, "text/json", JsonRequestBehavior.AllowGet);
        }
    }
}