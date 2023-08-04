using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestProject.Models;
using TestProject.Controllers;

namespace TestProject.Service
{
    public class MobileDataProcess
    {
        private WaterQualityEntities1 db = new WaterQualityEntities1();
        public List<ViewAquaruimSituation_ForMobile> GetAquariumDataList(int Auth001Id)
        {
            // 準備好空集合，等等要裝資料用
            List<ViewAquaruimSituation_ForMobile> DataList = new List<ViewAquaruimSituation_ForMobile>();

            // 先得知該用戶擁有多少魚缸
            List<Aquarium> aquariums = db.Aquarium.Where(x => x.Auth001Id == Auth001Id && x.BindTag == "0").ToList();

            // 用魚缸編號將該傳回的資料取出
            //foreach (var item in aquariums)
            //{
            //    ViewAquaruimSituation_ForMobile viewAquaruimSituation_ForMobile = db.ViewAquaruimSituation_ForMobile.Where(x => x.Auth001Id == Auth001Id && x.AquariumId == item.Id)
            //                                                                                                        .OrderByDescending(x => x.createTime)
            //                                                                                                        .FirstOrDefault();
            //    if (viewAquaruimSituation_ForMobile == null)
            //    {
            //        viewAquaruimSituation_ForMobile = new ViewAquaruimSituation_ForMobile
            //        {
            //            Auth001Id = Auth001Id,
            //            AquariumUnitNum = item.AquariumUnitNum,
            //            WaterType = "",
            //            BindTag = "",
            //            AquariumId = item.Id,
            //            temperature = "尚未擁有資料",
            //            Turbidity = "尚未擁有資料",
            //            PH = "尚未擁有資料",
            //            TDS = "尚未擁有資料",
            //            WaterLevel = "",
            //            createTime = DateTime.Now,
            //            WaterLevelNum = ""
            //        };
            //    }

            //    DataList.Add(viewAquaruimSituation_ForMobile);
            //}

            for (int i = 0; i < aquariums.Count; i++)
            {
                var item = aquariums[i];
                ViewAquaruimSituation_ForMobile viewAquaruimSituation_ForMobile = db.ViewAquaruimSituation_ForMobile.Where(x => x.Auth001Id == Auth001Id && x.AquariumId == item.Id)
                                                                                                                    .OrderByDescending(x => x.createTime)
                                                                                                                    .FirstOrDefault();
                if (viewAquaruimSituation_ForMobile == null)
                {
                    viewAquaruimSituation_ForMobile = new ViewAquaruimSituation_ForMobile
                    {
                        Auth001Id = Auth001Id,
                        AquariumUnitNum = item.AquariumUnitNum,
                        customAquaruimName = "未定義",
                        WaterType = item.WaterType,
                        BindTag = "",
                        AquariumId = item.Id,
                        temperature = "尚未擁有資料",
                        Turbidity = "尚未擁有資料",
                        PH = "尚未擁有資料",
                        TDS = "尚未擁有資料",
                        WaterLevel = "",
                        createTime = DateTime.Now,
                        WaterLevelNum = ""
                    };
                }
                // 如果沒有設定魚缸名稱，則顯示未定義
                if (viewAquaruimSituation_ForMobile.customAquaruimName == null ||
                    viewAquaruimSituation_ForMobile.customAquaruimName == "")
                {
                    viewAquaruimSituation_ForMobile.customAquaruimName = "未定義";
                }

                DataList.Add(viewAquaruimSituation_ForMobile);
            }

            // Replace this with your actual code to fetch the aquarium data
            return DataList;
        }

        /// <summary>
        /// 給予該魚缸的圖表所需資訊
        /// </summary>
        /// <param name="AquariumNum">要查詢的魚缸編號! 必須先確保該魚缸有人綁定且正在使用!</param>
        /// <returns></returns>
        public List<AquariumSituationMotify> PeriodQuality(string AquariumNum, int queryitemCount)
        {
            // 取幾筆資料設定
            int RowRange = queryitemCount;

            // 透過魚缸編號得知該魚缸目前跟使用者對應的AquariumId
            Aquarium Aquarium = db.Aquarium.FirstOrDefault(x => x.AquariumUnitNum == AquariumNum && x.BindTag == "0");

            // 魚缸id
            int AquariumIdNum;

            // 找無魚缸編號
            if (Aquarium == null)
            {
                // TODO!!!!!!!
                // 要是回傳為空集合，那呼叫者要在做處裡
                return new List<AquariumSituationMotify>();
            }

            AquariumIdNum = Aquarium.Id;

            //將最接近目前的資料抓出一個區塊
            List<AquariumSituation> DataList = db.AquariumSituation.Where(x => x.AquariumId == AquariumIdNum)
                                                                    .OrderByDescending(x => x.createTime)
                                                                    .ToList()
                                                                    .Take(RowRange)
                                                                    .OrderBy(x => x.createTime)
                                                                    .ToList();

            // 不能小於 6 筆
            if (DataList.Count == 0)
            {
                // TODO!!!!!!!
                // 要是回傳為空集合，那呼叫者要在做處裡
                return new List<AquariumSituationMotify>();
            }

            List<AquariumSituationMotify> DataListMotify = new List<AquariumSituationMotify>();
            // 更改物件，改為自定義水質狀況類 AquariumSituationMotify
            foreach (var item in DataList)
            {
                DateTime myDate = (DateTime)item.createTime;

                string MyDateTime = myDate.ToString("yy/MM/dd-HH:mm");

                DataListMotify.Add(new AquariumSituationMotify
                {
                    Id = item.Id,
                    AquariumId = item.AquariumId,
                    temperature = item.temperature,
                    Turbidity = item.Turbidity,
                    PH = item.PH,
                    TDS = item.TDS,
                    WaterLevel = item.WaterLevel,
                    createTime = MyDateTime,
                    WaterLevelNum = item.WaterLevelNum
                });
            }

            return DataListMotify;
        }
    }
}