using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
using TestProject.Models.MailTest;

using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TestProject.Controllers
{
    public class ArduinoDataProcessController : Controller
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        public ActionResult LmitateArduino()
        {
            return View();
        }


        [HttpPost]
        public void DataProcess(string StringData)
        {

            string[] WaterLevelNums = new string[3] {
                "Low Level",
                "Middle Level",
                "Heigh Level"
            };

            string[] StringSplitData = StringData.Split('/');
            int StringSplitDataSize = StringSplitData.Length;
            if (StringSplitDataSize != 6)
            {
                // 接收資料少於7筆，可能感測器出問題，寄送mail通知

                // 得知魚缸編號
                string AquariunNum = StringSplitData[0];

                // 透過魚缸編號 先知道該魚缸有正在使用的對應Id為多少，以及該用戶資訊取得
                Aquarium AquariumData = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariunNum && x.BindTag == "0");

                // 找到資料代表該魚缸目前有綁定用戶
                if (AquariumData != null)
                {
                    int Auth001Num = AquariumData.Auth001Id;
                    Auth001 auth001 = db.Auth001.FirstOrDefault(x => x.Id == Auth001Num);
                    string auth001Mail = auth001.Email;

                    sendGmail sendGmail = new sendGmail();
                    bool Confirm = sendGmail.Send_Gmail(AquariunNum, auth001Mail, "伺服器接收參數異常，請檢察感測器.");

                    if (auth001.LineToken!=null) {
                        var wbSendUse = new WebClient();
                        var dataSendUse = new NameValueCollection();

                        string UserToken = auth001.LineToken;
                        string urlSendUse = "https://notify-api.line.me/api/notify";
                        string Bearer = "Bearer " + UserToken;

                        wbSendUse.Headers.Add("Authorization", Bearer);

                        dataSendUse["message"] = "\n魚缸編號 : "+ AquariunNum+"\n伺服器接收參數異常，請檢察感測器.";
                        var responseSendUse = wbSendUse.UploadValues(urlSendUse, "POST", dataSendUse);
                    }
                }
            }
            else
            {
                // 接收資料為7筆，代表資料正確

                // 得知魚缸編號
                string AquariunNum = StringSplitData[0];

                // 透過魚缸編號 先知道該魚缸有正在使用的對應Id為多少，以及該用戶資訊取得
                Aquarium AquariumData = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariunNum && x.BindTag == "0");

                if (AquariumData == null)
                {
                    // 找無資料代表該魚缸目前沒有綁定用戶
                }
                else
                {
                    // 找到資料代表該魚缸目前有綁定用戶

                    //  得知該魚缸的用戶是誰
                    int AquariumAuth = AquariumData.Auth001Id;

                    //  得知該魚缸的Id
                    int AquariumNumId = AquariumData.Id;

                    // 取德溫度
                    string temperature = StringSplitData[1];

                    // 取德濁度
                    string Turbidity = StringSplitData[2];

                    // 取德PH
                    string PH = StringSplitData[3];

                    // 取德TDS
                    string TDS = StringSplitData[4];

                    // 取德WaterLevelNum，是個數字
                    string WaterLevelNum = StringSplitData[5];

                    // 取德WaterLevelNum對應的水位敘述
                    int LevelNumInt = Int16.Parse(WaterLevelNum);
                    string WaterLevel = WaterLevelNums[LevelNumInt - 1];


                    // 透過使用者魚缸ID 上傳資料
                    AquariumSituation aquariumSituation = new AquariumSituation();
                    aquariumSituation.AquariumId = AquariumNumId;
                    aquariumSituation.temperature = temperature;
                    aquariumSituation.Turbidity = Turbidity;
                    aquariumSituation.PH = PH;
                    aquariumSituation.TDS = TDS;
                    aquariumSituation.WaterLevel = WaterLevel;
                    aquariumSituation.createTime = DateTime.Now;
                    aquariumSituation.WaterLevelNum = WaterLevelNum;

                    db.AquariumSituation.Add(aquariumSituation);
                    db.SaveChanges();


                    // 先把string 值轉成 double
                    double temperatureDouble = Convert.ToDouble(temperature);
                    double TurbidityDouble = Convert.ToDouble(Turbidity);
                    double PHDouble = Convert.ToDouble(PH);
                    double TDSDouble = Convert.ToDouble(TDS);
                    double WaterLevelNumDouble = Convert.ToDouble(WaterLevelNum);

                    // 看該魚缸是否有設定範圍，並開啟通知
                    NotifySetRange notifySetRange = db.NotifySetRange.FirstOrDefault(x => x.AquariumUnitNum == AquariunNum && x.NotifyTag == "1");
                    if (notifySetRange != null)
                    {
                        // 溫度判斷 (低、高)?
                        string temperatureNotfyLow = temperatureDouble < Convert.ToDouble(notifySetRange.temperatureLowerBound) ? "溫度過低" : "正常";
                        string temperatureNotfyHeigh = temperatureDouble > Convert.ToDouble(notifySetRange.temperatureUpperBound) ? "溫度過高" : "正常";

                        // 濁度判斷 (過高)?
                        string TurbidityNotfyHeigh = TurbidityDouble > Convert.ToDouble(notifySetRange.TurbidityUpperBound) ? "濁度過高" : "正常";

                        // PH判斷 (低、高)?
                        string PHNotfyLow = PHDouble < Convert.ToDouble(notifySetRange.phLowerBound) ? "水質過酸" : "正常";
                        string PHNotfyHeigh = PHDouble > Convert.ToDouble(notifySetRange.pHUpperBound) ? "水質過鹼" : "正常";

                        // 硬度判斷 (低、高)?
                        string TDSNotfyLow = TDSDouble < Convert.ToDouble(notifySetRange.TDSLowerBound) ? "硬度過低" : "正常";
                        string TDSNotfyHeigh = TDSDouble > Convert.ToDouble(notifySetRange.TDSUpperBound) ? "硬度過高" : "正常";

                        // 水位判斷 (過低)?
                        string WaterLevelyNotfyLow = WaterLevelNumDouble <= Convert.ToDouble(notifySetRange.WaterLevelLowerBound) ? "水位過低" : "正常";

                        List<string> dataGarge = new List<string>();
                        // 溫度 判斷
                        if (temperatureNotfyLow != "正常")
                        {
                            dataGarge.Add(temperatureNotfyLow);
                        }
                        if (temperatureNotfyHeigh != "正常")
                        {
                            dataGarge.Add(temperatureNotfyHeigh);
                        }
                        // 濁度 判斷
                        if (TurbidityNotfyHeigh != "正常")
                        {
                            dataGarge.Add(TurbidityNotfyHeigh);
                        }
                        // PH 判斷
                        if (PHNotfyLow != "正常")
                        {
                            dataGarge.Add(PHNotfyLow);
                        }
                        if (PHNotfyHeigh != "正常")
                        {
                            dataGarge.Add(PHNotfyHeigh);
                        }
                        // TDS 判斷
                        if (TDSNotfyLow != "正常")
                        {
                            dataGarge.Add(TDSNotfyLow);
                        }
                        if (TDSNotfyHeigh != "正常")
                        {
                            dataGarge.Add(TDSNotfyHeigh);
                        }
                        // 濁度 判斷
                        if (WaterLevelyNotfyLow != "正常")
                        {
                            dataGarge.Add(WaterLevelyNotfyLow);
                        }
                        if (dataGarge.Count > 0) {
                            Auth001 auth001 = db.Auth001.FirstOrDefault(x => x.Id == AquariumAuth);
                            string auth001Mail = auth001.Email;

                            sendGmail sendGmail = new sendGmail();
                            bool Confirm = sendGmail.Send_Gmail(AquariunNum, auth001Mail, dataGarge);

                            if (auth001.LineToken != null)
                            {
                                var wbSendUse = new WebClient();
                                var dataSendUse = new NameValueCollection();

                                string UserToken = auth001.LineToken;
                                string urlSendUse = "https://notify-api.line.me/api/notify";
                                string Bearer = "Bearer " + UserToken;

                                var sb = new System.Text.StringBuilder();
                                sb.AppendLine("\n魚缸編號 : " + AquariunNum+"\n");
                                for (int i = 0; i < dataGarge.Count; i++)
                                {
                                    sb.AppendLine(dataGarge[i]);
                                }

                                wbSendUse.Headers.Add("Authorization", Bearer);

                                dataSendUse["message"] = sb.ToString();
                                var responseSendUse = wbSendUse.UploadValues(urlSendUse, "POST", dataSendUse);
                            }
                        }
                        
                    }

                }
            }
        }
    
    
    }
}