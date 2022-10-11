using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
using System.Data.Entity;

namespace TestProject.Controllers
{
    public class LogInAndRegisterController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        // GET: LogIn
        public ActionResult CreateOrLogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Auth001 auth001)
        {
            Auth001 DataTrack = db.Auth001.FirstOrDefault(x => x.Email == auth001.Email);
            if (DataTrack != null)
            {
                return View(auth001);
            }
            else
            {
                db.Auth001.Add(auth001);
                db.SaveChanges();
                return Redirect("CreateSuccess");
            }
        }

        public ActionResult CreateSuccess()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(string Email, string Password)
        {
            Auth001 DataTrack = db.Auth001.FirstOrDefault(x => x.Email == Email && x.Password == Password);
            if (DataTrack == null)
            {
                base.TempData["Email"] = Email;
                return RedirectToAction("CreateOrLogIn", "LogInAndRegister");
            }
            else
            {
                Session["UserEmail"] = Email;
                Session["Auth001Id"] = DataTrack.Id;
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOut()
        {
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Edit()
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            string Email = Session["UserEmail"].ToString();
            Auth001 UserInfo = db.Auth001.FirstOrDefault(x => x.Email == Email);
            return View(UserInfo);
        }

        [HttpPost]
        public ActionResult Edit(Auth001 auth001)
        {
            if (Session["UserEmail"] == null)
            {
                Session.RemoveAll();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string Email = Session["UserEmail"].ToString();
                Auth001 DataTrack = db.Auth001.FirstOrDefault(x => x.Email == Email);
                if (DataTrack != null)
                {
                    DataTrack.UserName = auth001.UserName;
                    DataTrack.Password = auth001.Password;
                    DataTrack.LineID = auth001.LineID;
                    db.Entry(DataTrack).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

        }

    }
}