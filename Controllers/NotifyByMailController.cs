using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using TestProject.Models.MailTest;
using TestProject.Models;

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
                DataTarget.WaterLevelLowerBound = "Low Level";
                DataTarget.NotifyTag = "0";

            }
            //若資料庫有該該區間設定，就直接送到前端
            ViewBag.DataTarget = DataTarget;
            ViewBag.AquarinmNum = AquariumNum;
            return View();
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

        [HttpPost]
        public ActionResult SendMail(string MessageToWeb, string MessageSubjectWeb, string MessageBodyWeb)
        {
            sendGmail sendMail = new sendGmail();

            if (sendMail.Send_PasswordForgot())
            {
                //成功
                return Redirect("Success");
            }
            else
            {
                //失敗
                return Redirect("Error");
            }
        }
    }
}