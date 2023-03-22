using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;

namespace TestProject.Controllers
{
    public class MobileServiceController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        /// <summary>
        /// 檢查用戶是否存在的路由
        /// </summary>
        [HttpPost]
        public ActionResult LoginUserChecking(string email, string password)
        {
            // 檢查用戶是否存在
            Auth001 userTarget = db.Auth001.FirstOrDefault(x =>
                                                                x.Email == email &&
                                                                x.Password == password);

            // 準備一個bool判斷
            bool state; // 在這裡進行你的操作，得到一個布林值
            if (userTarget == null)
            {
                state = false;
            }
            else
            {
                state = true;
            }


            return Json(new { State = state });
        }

        /// <summary>
        /// 檢查用戶是否存在的路由_POST
        /// </summary>
        [HttpPost]
        public ActionResult LoginUserCheckingPost(string email, string password)
        {
            // 檢查用戶是否存在
            Auth001 userTarget = db.Auth001.FirstOrDefault(x =>
                                                                x.Email == email &&
                                                                x.Password == password);

            // 準備好回傳狀態與內容類
            ReturnMsg returnMsg = new ReturnMsg();

            if (userTarget == null) // 為 NULL 帶表該用戶不存在
            {
                returnMsg.Status = false;
                returnMsg.Message = "查無此用戶!";
                returnMsg.Auth001Id = "NULL";
            }
            else // ELSE 代表該用戶存在
            {
                returnMsg.Status = true;
                returnMsg.Message = "通過~已成功登入!";
                returnMsg.Auth001Id = userTarget.Id.ToString();
                returnMsg.UserName = userTarget.UserName;
            }


            return Json(returnMsg);
        }

        /// <summary>
        /// 檢查用戶是否存在
        /// </summary>
        [HttpPost]
        public ActionResult RegisterUser(string username, string password,
                                         string email, string lineID)
        {
            // 看DB內有沒有用戶已經使用過這個信箱
            Auth001 DataTrack = db.Auth001.FirstOrDefault(x => x.Email == email);

            // 準備好回傳狀態與內容類
            ReturnMsg returnMsg = new ReturnMsg();

            // 若找到代表有人使用了
            if (DataTrack != null)
            {
                returnMsg.Status = false;
                returnMsg.Message = "該信箱已被使用，請更換再試。 ";
            }
            else // 沒找到代表沒有人使用! 這個信箱可以被使用。
            {
                try
                {
                    Auth001 auth001 = new Auth001();
                    auth001.UserName = username;
                    auth001.Password = password;
                    auth001.Email = email;
                    auth001.LineID = lineID;
                    db.Auth001.Add(auth001);
                    db.SaveChanges();

                    returnMsg.Status = true;
                    returnMsg.Message = "帳戶已成功建立!";
                }
                catch (Exception e)
                {
                    returnMsg.Status = false;
                    returnMsg.Message = "伺服器異常! 請稍後再試。";
                }


            }
            returnMsg.Auth001Id = "NULL";

            return Json(returnMsg);
        }

    }
    public class ReturnMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Auth001Id { get; set; }
        public string UserName { get; set; }
    }
}