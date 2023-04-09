using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
using TestProject.Service;

namespace TestProject.Controllers
{
    public class MobileServiceController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();

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


        /// <summary>
        /// 取得用戶所有魚缸的當前資訊與水質現況
        /// </summary>
        [HttpPost]
        public ActionResult GetAquariumDatas(int Auth001Id)
        {
            // 創建取資料服務
            MobileDataProcess mobileDataProcess = new MobileDataProcess();

            // 透過手機服務類，取得資料!
            List<ViewAquaruimSituation_ForMobile> aquariumDataList = mobileDataProcess.GetAquariumDataList(Auth001Id);

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            string json = JsonConvert.SerializeObject(aquariumDataList);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }

        /// <summary>
        /// 取得用戶對應魚缸的圖表按鈕是否可按
        /// </summary>
        [HttpPost]
        public ActionResult GetAquariumDataStatus(int Auth001Id)
        {
            // 創建取資料服務
            MobileDataProcess mobileDataProcess = new MobileDataProcess();

            // 透過手機服務類，取得資料!
            List<ViewAquaruimSituation_ForMobile> aquariumDataList = mobileDataProcess.GetAquariumDataList(Auth001Id);

            // 準備狀態List
            // List<bool> statusList = new List<bool>();

            // 建立Dic
            Dictionary<string, bool> keyValues = new Dictionary<string, bool>();

            // 將對應魚缸
            foreach (var item in aquariumDataList)
            {
                // 將對應 AquaruimId 的數據全部抓出
                List<AquariumSituation> DataArray = db.AquariumSituation.Where(x => x.AquariumId == item.AquariumId).ToList();

                if (DataArray.Count >= 6)
                {
                    keyValues.Add(item.AquariumUnitNum, true);
                    continue;
                }

                keyValues.Add(item.AquariumUnitNum, false);
            }

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            string json = JsonConvert.SerializeObject(keyValues);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }

        /// <summary>
        /// 給予用戶圖表資訊
        /// </summary>
        [HttpPost]
        public ActionResult GetAquariumDatasForAquaruimId(string AquariumNum)
        {
            // 創建取資料服務
            MobileDataProcess mobileDataProcess = new MobileDataProcess();

            // 透過手機服務類，取得資料!
            List<AquariumSituationMotify> aquariumDataList = mobileDataProcess.PeriodQuality(AquariumNum);

            // 準備 json 字串
            string json;

            // 理論上來說不可能會等於0，但這裡先做個保險
            if (aquariumDataList.Count < 6)
            {
                // 使用 Newtonsoft.Json 將列表轉換為 JSON
                json = JsonConvert.SerializeObject(new List<AquariumSituationMotify>());

                // 將 JSON 作為 FileResult 返回
                return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
            }

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(aquariumDataList);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");

        }

    }
    public class ReturnMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Auth001Id { get; set; }
        public string UserName { get; set; }
    }

    // 自定義魚缸水質狀況類別
    public class AquariumSituationMotify
    {
        public int Id { get; set; }
        public int AquariumId { get; set; }
        public string temperature { get; set; }
        public string Turbidity { get; set; }
        public string PH { get; set; }
        public string TDS { get; set; }
        public string WaterLevel { get; set; }
        public string createTime { get; set; }
        public string WaterLevelNum { get; set; }
    }
}