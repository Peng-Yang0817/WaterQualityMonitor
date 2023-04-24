﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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


        /// <summary>
        /// 把魚缸的綁定紀錄送到行動端
        /// </summary>
        /// <param name="AquariumNum">魚缸編號</param>
        [HttpPost]
        public ActionResult GetAquaruimNumBindHistory(string AquariumNum)
        {
            // 將該魚缸編號所有的使用紀錄都掉出來
            List<Aquarium> Datalist = db.Aquarium.Where(x => x.AquariumUnitNum == AquariumNum).ToList();

            // 創建歷史紀錄的集合，等等裝MOBILE所需要的資料
            List<AquaruimNumBindHistory> datas = new List<AquaruimNumBindHistory>();
            AquaruimNumBindHistory data = new AquaruimNumBindHistory();

            DateTime dateTime_createTime = new DateTime();
            DateTime dateTime_modifyTime = new DateTime();

            for (int i = 0; i < Datalist.Count; i++)
            {
                data = new AquaruimNumBindHistory();

                if (Datalist[i].BindTag.Equals("1"))
                {
                    data.bindtag = "解綁";
                }
                else
                {
                    data.bindtag = "使用中";
                }

                dateTime_createTime = (DateTime)Datalist[i].createTime;
                data.createTime = dateTime_createTime.ToString("yy/MM/dd-HH:mm");

                dateTime_modifyTime = (DateTime)Datalist[i].modifyTime;
                data.modifyTime = dateTime_modifyTime.ToString("yy/MM/dd-HH:mm");

                datas.Add(data);
            }
            // 準備 json 字串
            string json;

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(datas);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }

        /// <summary>
        /// 綁定魚缸服務__查看用戶資訊是否正確
        /// </summary>
        [HttpPost]
        public ActionResult BindAquariumService(string Email, string Password,
                                                string AquariumNum, string WaterType)
        {
            // 準備 json 字串
            string json;
            // 準備 回傳給用戶的資訊物件
            ReturnMsg returnMsg = new ReturnMsg();

            //查看使用者是否存在
            Auth001 UserInfo = db.Auth001.FirstOrDefault(x => x.Email == Email && x.Password == Password);

            // 查無該用戶，因此直接返回結果。
            if (UserInfo == null)
            {
                returnMsg = new ReturnMsg();
                returnMsg.Status = false;
                returnMsg.Message = "查無此用戶! 請再做確認。";
                returnMsg.Auth001Id = "-1";
                returnMsg.UserName = "Null";

                // 使用 Newtonsoft.Json 將列表轉換為 JSON
                json = JsonConvert.SerializeObject(returnMsg);

                // 將 JSON 作為 FileResult 返回
                return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
            }

            //查看魚缸是否已被綁定
            //查看該魚缸是否BindTag為0，等於0代表有人正在使用!!
            Aquarium AquInfo = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum && x.BindTag == "0");
            // 要是有資料! 代表就有人正在使用! 所以
            if (AquInfo != null)
            {
                returnMsg = new ReturnMsg();
                returnMsg.Status = false;
                returnMsg.Message = "該魚缸已有人正在使用，請確認魚缸代碼是否有被使用過。";
                returnMsg.Auth001Id = "-1";
                returnMsg.UserName = "Null";

                // 使用 Newtonsoft.Json 將列表轉換為 JSON
                json = JsonConvert.SerializeObject(returnMsg);

                // 將 JSON 作為 FileResult 返回
                return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
            }

            // 開始準備存取魚缸綁定者資訊
            Aquarium AddData = new Aquarium();
            AddData.Auth001Id = UserInfo.Id;
            AddData.AquariumUnitNum = AquariumNum;
            AddData.WaterType = WaterType;
            String NowTime = DateTime.Now.ToString();
            AddData.BindTag = "0";
            AddData.createTime = DateTime.Now;
            AddData.modifyTime = DateTime.Now;

            db.Aquarium.Add(AddData);
            db.SaveChanges();

            // 打包回傳字串
            returnMsg = new ReturnMsg();
            returnMsg.Status = true;
            returnMsg.Message = "綁定成功!";
            returnMsg.Auth001Id = UserInfo.Id.ToString();
            returnMsg.UserName = UserInfo.UserName.ToString();

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(returnMsg);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }


        /// <summary>
        /// 綁定魚缸服務__查看用戶資訊是否正確
        /// </summary>
        /// <param name="Auth001Id">用戶Id</param>
        /// /// <param name="AquariumNum">魚缸編號</param>
        [HttpPost]
        public ActionResult unBindService(string Auth001Id, string AquariumNum)
        {
            // 準備 json 字串
            string json;
            // 準備 回傳給用戶的資訊物件
            ReturnMsg returnMsg = new ReturnMsg();

            int Auth001Id_int = int.Parse(Auth001Id);

            // 判斷該用戶有無該魚缸的權力
            // 用戶Id 、 魚缸編號 、 BindTag 是使用中
            Aquarium JudgeUser = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id_int &&
                                                                 x.AquariumUnitNum == AquariumNum &&
                                                                 x.BindTag == "0");
            // NULL 代表用戶沒有該魚缸的擁有權
            if (JudgeUser == null)
            {
                returnMsg = new ReturnMsg();
                returnMsg.Status = false;
                returnMsg.Message = "該用戶並無該魚缸權限，請再做確認。";
                returnMsg.Auth001Id = "-1";
                returnMsg.UserName = "Null";

                // 使用 Newtonsoft.Json 將列表轉換為 JSON
                json = JsonConvert.SerializeObject(returnMsg);

                // 將 JSON 作為 FileResult 返回
                return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
            }

            // 將該魚缸的BindTag設定為1
            JudgeUser.BindTag = "1";
            JudgeUser.modifyTime = DateTime.Now; ;
            db.Entry(JudgeUser).State = EntityState.Modified;
            db.SaveChanges();

            // 取得用戶資訊
            Auth001 auth001 = db.Auth001.FirstOrDefault(x => x.Id == Auth001Id_int);

            // 打包成功的回傳字串
            returnMsg = new ReturnMsg();
            returnMsg.Status = true;
            returnMsg.Message = "已成功將魚缸 " + AquariumNum + " 解除綁定。";
            returnMsg.Auth001Id = Auth001Id;
            returnMsg.UserName = auth001.UserName;

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(returnMsg);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }

        /// <summary>
        /// 魚缸區間範圍設置狀態__全魚缸
        /// </summary>
        /// <param name="Auth001Id">用戶Id</param>
        [HttpPost]
        public ActionResult AquariumRangeSetNotifyState(int Auth001Id)
        {
            // 準備 json 字串
            string json;
            // 準備 回傳給用戶的資訊物件
            ReturnMsg returnMsg = new ReturnMsg();

            // 該使用者所擁有的所有魚缸
            List<Aquarium> titleDataList = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag == "0").ToList();
            List<AquaruimNotifyState> notifyStateList = new List<AquaruimNotifyState>();
            AquaruimNotifyState notifyState = new AquaruimNotifyState();
            string string_notifyState = "";

            foreach (var item in titleDataList)
            {
                notifyState = new AquaruimNotifyState();

                // 查看有無設置區間過
                NotifySetRange NotifyOnListData = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == item.AquariumUnitNum);
                if (NotifyOnListData != null)
                {
                    //代表這筆有開啟通知
                    if (NotifyOnListData.NotifyTag == "1")
                    {
                        string_notifyState = "開啟";
                    }
                    if (NotifyOnListData.NotifyTag == "0")
                    {
                        string_notifyState = "關閉";
                    }

                }
                else
                {
                    //代表這筆未開啟通知
                    string_notifyState = "未定";
                }

                notifyState.aquariumNum = item.AquariumUnitNum;
                notifyState.notifyTage = string_notifyState;
                notifyStateList.Add(notifyState);
            }

            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(notifyStateList);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }


        /// <summary>
        /// 魚缸水質區間範圍設置__單一魚缸
        /// </summary>
        /// <param name="Auth001Id">用戶Id</param>
        [HttpPost]
        public ActionResult AquariumRangeSetGetDeatilToEdit(int Auth001Id, string AquariumNum)
        {
            // 準備 json 字串
            string json;

            //核對開參數是否是是該使用者所擁有的
            Aquarium DataCheck = db.Aquarium.FirstOrDefault(x => x.Auth001Id == Auth001Id &&
                                                                 x.AquariumUnitNum == AquariumNum &&
                                                                 x.BindTag == "0");

            NotifySetRange DataTarget = new NotifySetRange();

            //若資料庫找不到，代表該用戶並無此魚缸使用權 (理論上來說不太會發生)
            if (DataCheck == null)
            {
                DataTarget = new NotifySetRange();

                DataTarget.AquariumUnitNum = AquariumNum;
                DataTarget.temperatureUpperBound = "";
                DataTarget.temperatureLowerBound = "";
                DataTarget.pHUpperBound = "";
                DataTarget.phLowerBound = "";
                DataTarget.TDSUpperBound = "";
                DataTarget.TDSLowerBound = "";
                DataTarget.TurbidityUpperBound = "";
                DataTarget.WaterLevelLowerBound = "";
                DataTarget.NotifyTag = "";

                // 使用 Newtonsoft.Json 將列表轉換為 JSON
                json = JsonConvert.SerializeObject(DataTarget);

                // 將 JSON 作為 FileResult 返回
                return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
            }

            // 返回用戶之前所編輯的資料
            DataTarget = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum);
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


            // 使用 Newtonsoft.Json 將列表轉換為 JSON
            json = JsonConvert.SerializeObject(DataTarget);

            // 將 JSON 作為 FileResult 返回
            return File(Encoding.UTF8.GetBytes(json), "application/json", "AquariumData.json");
        }
    }


    // 自定義:
    //  狀態、回復訊息、用戶Auth001Id、使用者名稱
    // 的類別
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

    // 自定義魚缸綁定歷史紀錄類別
    public class AquaruimNumBindHistory
    {
        public string createTime { get; set; }
        public string modifyTime { get; set; }
        public string bindtag { get; set; }
    }

    // 自定義用戶魚缸  通知範圍設置狀態  類別
    public class AquaruimNotifyState
    {
        public string aquariumNum { get; set; }
        public string notifyTage { get; set; }
    }
}