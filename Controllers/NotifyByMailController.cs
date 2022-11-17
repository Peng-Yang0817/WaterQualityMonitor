using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using TestProject.Models.MailTest;
using TestProject.Models;
//必須多添加這using 才能使用Entity Framework的Save功能
using System.Data.Entity;

namespace TestProject.Controllers
{
    public class NotifyByMailController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        public ActionResult SetAreaRange()
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            // 該使用者所擁有的所有魚缸
            List<Aquarium> TitleDataList = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag == "0").ToList();
            int TitleDataListSize = TitleDataList.Count;
            string[] TitleDataListNotifyOnOrOff = new string[TitleDataListSize];

            //將有開啟通知的魚缸全抓出來
            for (int i = 0; i < TitleDataListSize; i++) {
                string AquariumNum = TitleDataList[i].AquariumUnitNum;
                NotifySetRange NotifyOnListData = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum);
                if (NotifyOnListData != null)
                {
                    //代表這筆有開啟通知
                    if (NotifyOnListData.NotifyTag == "1")
                    {
                        TitleDataListNotifyOnOrOff[i] = "開啟";
                    }
                    if (NotifyOnListData.NotifyTag == "0")
                    {
                        TitleDataListNotifyOnOrOff[i] = "關閉";
                    }
                    
                }
                else 
                {
                    //代表這筆未開啟通知
                    TitleDataListNotifyOnOrOff[i] = "未定";
                }
            }
            ViewBag.TitleDataListNotifyOnOrOff = TitleDataListNotifyOnOrOff;
            ViewBag.AquariumDataList = TitleDataList;
            return View();
        }

        public ActionResult SetAreaRangeCheck(string AquariumNum)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            //核對開鹿由參數是否是該使用者的
            Aquarium DataCheck = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id && x.AquariumUnitNum == AquariumNum && x.BindTag == "0");
            //若不是該使用者就反回首頁
            if (DataCheck == null)
            {
                return RedirectToAction("Index", "Home");
            }
            NotifySetRange DataTarget = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum);
            if (DataTarget == null)
            {
                //若資料庫沒有該區間設定，就先初始化該魚缸的區間設定
                DataTarget = new NotifySetRange();
                DataTarget.AquariumUnitNum = AquariumNum;
                DataTarget.temperatureUpperBound = "100";
                DataTarget.temperatureLowerBound = "0";
                DataTarget.pHUpperBound = "14";
                DataTarget.phLowerBound = "0";
                DataTarget.TDSUpperBound = "10000";
                DataTarget.TDSLowerBound = "0";
                DataTarget.TurbidityUpperBound = "3000";
                DataTarget.WaterLevelLowerBound = "1";
                DataTarget.NotifyTag = "0";

            }
            //若資料庫有該該區間設定，就直接送到前端
            ViewBag.DataTarget = DataTarget;
            ViewBag.AquarinmNum = AquariumNum;
            return View();
        }
        [HttpPost]
        public ActionResult GetUserRangeData(NotifySetRange DataTarget)
        {
            // 時間到時請用戶重新登入
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }

            // 得知目前設定的魚缸編號
            string AquariumTargerNum = DataTarget.AquariumUnitNum;
            // 取得用戶設定的魚缸狀態
            string NotifyTag = DataTarget.NotifyTag;

            double temperatureUpperBound,
                temperatureLowerBound,
                pHUpperBound,
                phLowerBound,
                TDSUpperBound,
                TDSLowerBound,
                TurbidityUpperBound,
                WaterLevelLowerBound;

            // 確定傳過來的都為福點數後
            if (double.TryParse(DataTarget.temperatureUpperBound, out temperatureUpperBound) &&
                double.TryParse(DataTarget.temperatureLowerBound, out temperatureLowerBound) &&
                double.TryParse(DataTarget.pHUpperBound, out pHUpperBound) &&
                double.TryParse(DataTarget.phLowerBound, out phLowerBound) &&
                double.TryParse(DataTarget.TDSUpperBound, out TDSUpperBound) &&
                double.TryParse(DataTarget.TDSLowerBound, out TDSLowerBound) &&
                double.TryParse(DataTarget.TurbidityUpperBound, out TurbidityUpperBound) &&
                double.TryParse(DataTarget.WaterLevelLowerBound, out WaterLevelLowerBound))
            {
                //成功
                NotifySetRange UpdateTarget = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == AquariumTargerNum);

                //看資料庫是否有該魚缸編號的紀錄
                if (UpdateTarget == null)
                {
                    // 目前Table沒有該魚缸資料
                    UpdateTarget = new NotifySetRange();
                    UpdateTarget.AquariumUnitNum = AquariumTargerNum;
                    UpdateTarget.temperatureUpperBound = temperatureUpperBound.ToString();
                    UpdateTarget.temperatureLowerBound = temperatureLowerBound.ToString();
                    UpdateTarget.pHUpperBound = pHUpperBound.ToString();
                    UpdateTarget.phLowerBound = phLowerBound.ToString();
                    UpdateTarget.TDSUpperBound = TDSUpperBound.ToString();
                    UpdateTarget.TDSLowerBound = TDSLowerBound.ToString();
                    UpdateTarget.TurbidityUpperBound = TurbidityUpperBound.ToString();
                    UpdateTarget.WaterLevelLowerBound = WaterLevelLowerBound.ToString();
                    UpdateTarget.NotifyTag = NotifyTag;

                    db.NotifySetRange.Add(UpdateTarget);
                    db.SaveChanges();
                }
                else
                {
                    // 目前Table有該魚缸資料
                    UpdateTarget.temperatureUpperBound = temperatureUpperBound.ToString();
                    UpdateTarget.temperatureLowerBound = temperatureLowerBound.ToString();
                    UpdateTarget.pHUpperBound = pHUpperBound.ToString();
                    UpdateTarget.phLowerBound = phLowerBound.ToString();
                    UpdateTarget.TDSUpperBound = TDSUpperBound.ToString();
                    UpdateTarget.TDSLowerBound = TDSLowerBound.ToString();
                    UpdateTarget.TurbidityUpperBound = TurbidityUpperBound.ToString();
                    UpdateTarget.WaterLevelLowerBound = WaterLevelLowerBound.ToString();
                    UpdateTarget.NotifyTag = NotifyTag;

                    db.Entry(UpdateTarget).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("SetAreaRange");

            }
            else
            {
                //失敗
                return Redirect("Error");
            }

        }
        // GET: NotifyByMail
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult CodeError()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult SendMail(string MessageToWeb, string MessageSubjectWeb, string MessageBodyWeb)
        //{
        //    sendGmail sendMail = new sendGmail();

        //    if (sendMail.Send_Gmail())
        //    {
        //        //成功
        //        return Redirect("Success");
        //    }
        //    else
        //    {
        //        //失敗
        //        return Redirect("Error");
        //    }
        //}
    }
}