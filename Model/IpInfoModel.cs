namespace com.xd.intl.pc{
    public class IpInfoModel : BaseModel{
        public string city { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string src_ip { get; set; }
        public string timeZone { get; set; }

        private static IpInfoModel currentMd = null;

        public static void SaveToLocal(IpInfoModel model){
            if (model != null){
                //测试用的代码
                // var region = DataStorage.LoadString("test_region");
                // if (!string.IsNullOrEmpty(region)){
                //     model.country_code = region;
                // }
                
                string json = XDGSDK.GetJson(model);
                DataStorage.SaveString(DataStorage.IpInfo, json);
                currentMd = model;
            }
        }

        public static IpInfoModel GetLocalModel(){
            if (currentMd == null){
                string json = DataStorage.LoadString(DataStorage.IpInfo);
                if (!string.IsNullOrEmpty(json)){
                    currentMd = XDGSDK.GetModel<IpInfoModel>(json);
                }
            }
            return currentMd;
        }
    }
}