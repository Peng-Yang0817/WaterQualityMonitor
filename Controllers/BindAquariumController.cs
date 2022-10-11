using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;

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
            return View();
        }


        [HttpPost]
        public ActionResult Bind(string Email, string Password, string UnitNum,string WaterType)
        {
            //查看使用者是否存在
            Auth001 UserInfo = db.Auth001.FirstOrDefault(x => x.Email == Email && x.Password == Password);
            //查看魚缸是否已被綁定
            Aquarium AquInfo = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == UnitNum);
            if (UserInfo == null || AquInfo != null)
            {
                base.TempData["UnitNum"] = UnitNum;
                base.TempData["WaterType"] = WaterType;
                UserInfo = new Auth001();
                UserInfo.Email = Email;
                UserInfo.Password = Password;
                return View(UserInfo);
            }
            else
            {
                Aquarium AddData = new Aquarium();
                AddData.Auth001Id = int.Parse(Session["Auth001Id"].ToString());
                AddData.AquariumUnitNum = UnitNum;
                AddData.WaterType = WaterType;

                db.Aquarium.Add(AddData);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
        }
    }
}