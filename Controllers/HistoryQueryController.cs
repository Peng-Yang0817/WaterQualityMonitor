using Microsoft.AspNetCore.Cors;
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

        [EnableCors("Policy1")]
        public ActionResult GetUserData(string AquariumNum)
        {
            // 取得帳戶ID
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            // 先把所有對應魚缸編號的魚缸抓出
            List<Aquarium> Datalist = db.Aquarium.Where(x => x.AquariumUnitNum == AquariumNum).ToList();


            if (Datalist.Count == 0)
            {
                // 有可能用戶是輸入魚缸暱稱，因此將用戶的暱稱轉成AquariumNum 再找一次
                Aquarium CustomNameToAquariumNum = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id &&
                                                                          x.customAquaruimName == AquariumNum);
                if (CustomNameToAquariumNum != null)
                {
                    Datalist = db.Aquarium.Where(x => x.AquariumUnitNum == CustomNameToAquariumNum.AquariumUnitNum).ToList();
                }
                
            }

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