using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestProject.Models;

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
            foreach (var item in aquariums)
            {
                ViewAquaruimSituation_ForMobile viewAquaruimSituation_ForMobile = db.ViewAquaruimSituation_ForMobile.Where(x => x.Auth001Id == Auth001Id && x.AquariumId == item.Id)
                                                                                                                    .OrderByDescending(x => x.createTime)
                                                                                                                    .FirstOrDefault();
                if (viewAquaruimSituation_ForMobile == null) {
                    viewAquaruimSituation_ForMobile = new ViewAquaruimSituation_ForMobile {
                        Auth001Id = Auth001Id,
                        AquariumUnitNum = item.AquariumUnitNum,
                        WaterType = "",
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

                DataList.Add(viewAquaruimSituation_ForMobile);
            }

            // Replace this with your actual code to fetch the aquarium data
            return DataList;
        }
    }
}