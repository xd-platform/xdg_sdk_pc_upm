using System.Collections.Generic;
using Newtonsoft.Json;


namespace SDK.PC{
    public class InitConfigModel : BaseModel{
        public Data data{ get; set; }

        public class BindEntriesConfig{
            public int canBind{ get; set; }
            public string entryName{ get; set; }
            public int canUnbind{ get; set; }
        }

        public class TapSdkConfig{
            public string clientId{ get; set; }
            public string tapDBChannel{ get; set; }
            public string clientToken{ get; set; }
            public string serverUrl{ get; set; }
            public bool enableTapDB{ get; set; }
        }

        public class Configs{
            public string webPayUrl{ get; set; }
            public string serviceAgreementUrl{ get; set; }
            public string googlePlayGamesAndroidClientId{ get; set; }
            public string serviceTermsUrl{ get; set; }
            public string reportUrl{ get; set; }
            public List<BindEntriesConfig> bindEntriesConfig{ get; set; }
            public List<string> androidLoginEntries{ get; set; }
            public bool enableFirebaseTrack{ get; set; }
            public List<string> iosLoginEntries{ get; set; }
            public string californiaPrivacyUrl{ get; set; }
            public string gameName{ get; set; }
            public TapSdkConfig tapSdkConfig{ get; set; }
            public List<string> tapLoginPermissions{ get; set; }
            public List<string> bindEntries{ get; set; }
            public string appId{ get; set; }
            public string facebookClientId{ get; set; }
            public string region{ get; set; }
            public bool isKRPushServiceSwitchEnable{ get; set; }
        }

        public class Data{
            public string version{ get; set; }
            public string groupId{ get; set; }
            public Configs configs{ get; set; }
        }
        
        private static InitConfigModel currentMd = null;

        public static void SaveToLocal(InitConfigModel model){
            if (model != null){
                string json = XDGSDK.GetJson(model);
                DataStorage.SaveString(DataStorage.InitConfig, json);
                currentMd = model;
            }
        }

        public static InitConfigModel GetLocalModel(){
            if (currentMd == null){
                string json = DataStorage.LoadString(DataStorage.InitConfig);
                if (!string.IsNullOrEmpty(json)){
                    currentMd = XDGSDK.GetModel<InitConfigModel>(json);
                }
            }
            return currentMd;
        }

        public static bool CanShowPrivacyAlert(){
            var md = GetLocalModel();
            if (md == null){
                XDGSDK.Log("请先初始化");
                return false;
            } else{
                var preStr = DataStorage.LoadString(DataStorage.PrivacyKey);
                var currentStr = $"{md.data.version}-{md.data.configs.serviceAgreementUrl}-{md.data.configs.serviceTermsUrl}";
                if (string.IsNullOrEmpty(preStr)){
                    return true;
                }else if (currentStr.Equals(preStr)){
                    return false;
                }
                return true;
            }
        }

        public static void UpdatePrivacyState(){
            var md = GetLocalModel();
            if (md != null){
                var str = $"{md.data.version}-{md.data.configs.serviceAgreementUrl}-{md.data.configs.serviceTermsUrl}";
                DataStorage.SaveString(DataStorage.PrivacyKey, str);
            }
        }

    }
}