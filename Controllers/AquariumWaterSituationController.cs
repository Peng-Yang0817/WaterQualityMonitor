using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
using System.Data.Entity;

namespace TestProject.Controllers
{
    public class AquariumWaterSituationController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        // GET: AquariumSituation
        public ActionResult Index()
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            // 該使用者所擁有的所有魚缸
            List<Aquarium> TitleDataList = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag =="0").ToList();
            List<AquariumSituation> DataList = new List<AquariumSituation>();
            for (int i = 0; i < TitleDataList.Count; i++)
            {
                //取得魚缸對應的Id
                int UnitUum = TitleDataList[i].Id;

                //將該魚缸擁有的數據全部抓出
                List<AquariumSituation> DataArray = db.AquariumSituation.Where(x => x.AquariumId == UnitUum).ToList();
                AquariumSituation DataGarge = new AquariumSituation();


                if (DataArray.Count == 0)
                {
                    DataGarge.temperature = "沒有紀錄";
                    DataGarge.Turbidity = "沒有紀錄";
                    DataGarge.PH = "沒有紀錄";
                    DataGarge.TDS = "沒有紀錄";
                    DataGarge.WaterLevel = "沒有紀錄";
                    DataList.Add(DataGarge);
                    continue;
                }

                DataGarge = DataArray[DataArray.Count - 1];
                DataList.Add(DataGarge);
            }
            ViewBag.DataList = DataList;
            ViewBag.TitleDataList = TitleDataList;
            return View();
        }


        public ActionResult PeriodQuality(string AquariumNum)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            //取幾筆資料設定
            int RowRange = 6;

            //透過魚缸編號得知該魚缸目前跟使用者對應的AquariumId
            int AquariumIdNum = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum && x.BindTag == "0").Id;

            //將最接近目前的資料抓出一個區塊
            List<AquariumSituation> DataList = db.AquariumSituation.Where(x=>x.AquariumId== AquariumIdNum).OrderByDescending(x => x.createTime).ToList().Take(RowRange).ToList();

            ViewBag.AquariumNum = AquariumNum;
            ViewBag.DataList = DataList;
            return View();
        }
    }
}