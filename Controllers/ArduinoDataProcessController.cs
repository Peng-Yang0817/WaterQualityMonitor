using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestProject.Models;
using TestProject.Models.MailTest;

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
            if (StringSplitDataSize != 6) {
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
                    bool Confirm = sendGmail.Send_Gmail(auth001Mail,"伺服器接收參數異常，請檢察感測器.");
                }
            }
            else {
                // 接收資料為7筆，代表資料正確

                // 得知魚缸編號
                string AquariunNum = StringSplitData[0];

                // 透過魚缸編號 先知道該魚缸有正在使用的對應Id為多少，以及該用戶資訊取得
                Aquarium AquariumData = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariunNum && x.BindTag == "0");

                if (AquariumData == null)
                {
                    // 找無資料代表該魚缸目前沒有綁定用戶
                }
                else {
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
                    string WaterLevel = WaterLevelNums[LevelNumInt-1];


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

                    
                }
            }
        }
    }
}