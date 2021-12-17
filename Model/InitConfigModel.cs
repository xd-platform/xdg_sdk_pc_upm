using System.Collections.Generic;
using Newtonsoft.Json;


namespace SDK.PC{
    public class InitConfigModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public string version{ get; set; }
            public int groupId{ get; set; }
            public Configs configs{ get; set; }
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
            public List<string> tapLoginPermissions{ get; set; }
            public List<string> bindEntries{ get; set; }
            public int appId{ get; set; }
            public long facebookClientId{ get; set; }
            public string region{ get; set; }
            public bool isKRPushServiceSwitchEnable{ get; set; }
        }

        public class BindEntriesConfig{
            public int canBind{ get; set; }
            public string entryName{ get; set; }
            public int canUnbind{ get; set; }
        }

        public static void SaveToLocal(InitConfigModel model){
            if (model != null){
                string json = XDGSDK.GetJson(model);
                DataStorage.SaveString(DataStorage.InitConfig, json);
            }
        }
        
        public static InitConfigModel GetLocalModel(){
            string json = DataStorage.LoadString(DataStorage.InitConfig);
            if (!string.IsNullOrEmpty(json)){
                return XDGSDK.GetModel<InitConfigModel>(json);
            } else{
                return null;
            }
        }

        public static void ClearLocalModel(){
            DataStorage.SaveString(DataStorage.InitConfig, "");
        }

    }
}