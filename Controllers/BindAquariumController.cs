using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
//必須多添加這using 才能使用Entity Framework的Save功能
using System.Data.Entity;

namespace TestProject.Controllers
{
    public class BindAquariumController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        // GET: BindAquarium
        public ActionResult Bind()
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

            if (base.TempData["ErMsg"] != null)
            {
                Auth001 UserInfo = new Auth001();
                UserInfo.Email = Email;

                return View(UserInfo);
            }
            return View();
        }


        [HttpPost]
        public ActionResult Bind(string Email, string Password, string UnitNum, string WaterType)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }

            //查看使用者是否存在
            Auth001 UserInfo = db.Auth001.FirstOrDefault(x => x.Email == Email && x.Password == Password);

            //查看魚缸是否已被綁定
            //查看該魚缸是否BindTag為0，等於0代表有人正在使用!!
            //要是有資料! 代表就有人正在使用! 所以會擋回前端~
            Aquarium AquInfo = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == UnitNum && x.BindTag == "0");
            if (UserInfo == null || AquInfo != null)
            {
                base.TempData["UnitNum"] = UnitNum;
                base.TempData["WaterType"] = WaterType;

                if (UserInfo == null)
                {
                    base.TempData["ErMsg"] = "帳密錯誤，請再做確認!";
                }
                else
                {
                    base.TempData["ErMsg"] = "錯誤! 填寫魚缸已被綁定";
                }
                UserInfo = new Auth001();
                UserInfo.Email = Email;
                UserInfo.Password = Password;

                int Auth001Id = int.Parse(Session["Auth001Id"].ToString());
                // 該使用者所擁有的所有魚缸
                List<Aquarium> TitleDataList = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag == "0").ToList();

                ViewBag.AquariumDataList = TitleDataList;

                return View(UserInfo);
            }
            else
            {
                Aquarium AddData = new Aquarium();
                AddData.Auth001Id = int.Parse(Session["Auth001Id"].ToString());
                AddData.AquariumUnitNum = UnitNum;
                AddData.WaterType = WaterType;

                String NowTime = DateTime.Now.ToString();
                AddData.BindTag = "0";
                AddData.createTime = DateTime.Now;
                AddData.modifyTime = DateTime.Now;

                db.Aquarium.Add(AddData);
                db.SaveChanges();
                //return RedirectToAction("Index", "Home");
                UserInfo = new Auth001();
                base.TempData["ErMsg"] = "成功! 已成功將魚缸綁定至該帳戶!";

                // 該使用者所擁有的所有魚缸
                int Auth001Id = int.Parse(Session["Auth001Id"].ToString());
                List<Aquarium> TitleDataList = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag == "0").ToList();

                ViewBag.AquariumDataList = TitleDataList;
                return View(UserInfo);
            }
        }

        public ActionResult DelectConfirmation(string DelectConfirmation)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            //判斷該用戶有無該魚缸的權力
            Aquarium JudgeUser = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id && x.AquariumUnitNum == DelectConfirmation && x.BindTag == "0");
            if (JudgeUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.UnitNum = DelectConfirmation;
            return View();
        }

        public ActionResult ConfirmationOK(string DelectConfirmation)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            int Auth001Id = int.Parse(Session["Auth001Id"].ToString());

            //判斷該用戶有無該魚缸的權力
            //查看該魚缸有無
            Aquarium JudgeUser = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id && x.AquariumUnitNum == DelectConfirmation && x.BindTag == "0");

            if (JudgeUser == null)
            {
                // 該使用者所擁有的所有魚缸
                base.TempData["ErMsg"] = "錯誤! 該帳戶未擁有此權限。";
                return RedirectToAction("Bind", "BindAquarium", new { UerInfo = new Auth001() });
            }
            else
            {
                JudgeUser.BindTag = "1";
                JudgeUser.modifyTime = DateTime.Now; ;
                db.Entry(JudgeUser).State = EntityState.Modified;
                db.SaveChanges();

                base.TempData["ErMsg"] = "成功! 已成功解除綁定。";
                return RedirectToAction("Bind", "BindAquarium", new { UerInfo = new Auth001() });
                //return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// 魚缸名稱字定義__GET
        /// </summary>
        public ActionResult CustomAquariumNameSet(string AquariumNum, string CustomName)
        {
            ViewBag.AquariumNum = AquariumNum;
            ViewBag.CustomName = CustomName;
            return View();
        }
        /// <summary>
        /// 魚缸名稱字定義__POST
        /// </summary>
        [HttpPost]
        public ActionResult CustomAquariumNameSetPost(string AquariumNum, string CustomName)
        {
            // 取得更改目標
            Aquarium JudgeUser = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum &&
                                                                 x.BindTag == "0");

            // 沒抓到目標，請求方式異常
            if (JudgeUser == null)
            {
                return View("Error");
            }

            JudgeUser.customAquaruimName = CustomName;
            db.Entry(JudgeUser).State = EntityState.Modified;
            db.SaveChanges();

            base.TempData["ErMsg"] = "成功! 已成功定義名稱。";
            return RedirectToAction("Bind", "BindAquarium", new { UerInfo = new Auth001() });
        }

    }
}