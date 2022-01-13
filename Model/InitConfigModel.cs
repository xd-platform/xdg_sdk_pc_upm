using System;
using System.Collections.Generic;
using System.IO;
using TapTap.Common;
using UnityEngine;

namespace com.xd.intl.pc{
    public class InitConfigModel : BaseModel{
        public Data data{ get; set; }

        public class BindEntriesConfig{
            public int canBind{ get; set; }
            public string entryName{ get; set; }
            public int canUnbind{ get; set; }
        }

        public class TapSdkConfig : BaseModel{
            public string clientId{ get; set; }
            public string tapDBChannel{ get; set; }
            public string clientToken{ get; set; }
            public string serverUrl{ get; set; }
            public bool enableTapDB{ get; set; }

            public static TapSdkConfig ReadLocalTapConfig(){
                var txtAsset = Resources.Load("XD-Info") as TextAsset;
                TapSdkConfig md = null;
                
                if (txtAsset != null){
                    var json = txtAsset.text;
                    var dic = Json.Deserialize(json) as Dictionary<string, object>;
                    var cfg = SafeDictionary.GetValue<Dictionary<string, object>>(dic, "tapsdk"); 
                    md = XDGSDK.GetModel<TapSdkConfig>(XDGSDK.GetJson(cfg));   
                }

                if (md == null || string.IsNullOrEmpty(md.clientId)){
                    XDGSDK.LogError("/Plugins/Resources/XD-Info.json  解析失败");
                }

                return md;
            }
        }

        public class Configs{
            public string webPayUrl{ get; set; }
            public string serviceAgreementUrl{ get; set; }
            public string serviceAgreementTxt{ get; set; }
            public string googlePlayGamesAndroidClientId{ get; set; }
            public string serviceTermsUrl{ get; set; }
            public string serviceTermsTxt{ get; set; }
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
                currentMd.SavePrivacyTxt();
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
                var currentStr =
                    $"{md.data.version}-{md.data.configs.serviceAgreementUrl}-{md.data.configs.serviceTermsUrl}";
                if (string.IsNullOrEmpty(preStr)){
                    return true;
                } else if (currentStr.Equals(preStr)){
                    return false;
                }

                return true;
            }
        }

        public static void UpdatePrivacyState(){ //弹过之后若没变化就不再弹出
            var md = GetLocalModel();
            if (md != null){
                var str = $"{md.data.version}-{md.data.configs.serviceAgreementUrl}-{md.data.configs.serviceTermsUrl}";
                DataStorage.SaveString(DataStorage.PrivacyKey, str);
            }
        }

        private void SavePrivacyTxt(){
            Net.GetRequest(data.configs.serviceTermsTxt, (txt) => {
                if (!string.IsNullOrEmpty(txt)){
                    DataStorage.SaveString(data.configs.serviceTermsTxt, txt);
                }
            }, (code, msg) => { });

            Net.GetRequest(data.configs.serviceAgreementTxt, (txt) => {
                if (!string.IsNullOrEmpty(txt)){
                    DataStorage.SaveString(data.configs.serviceAgreementTxt, txt);
                }
            }, (code, msg) => { });
        }

        public void GetPrivacyTxt(string txtUrl, Action<string> callback){
            var txt = DataStorage.LoadString(txtUrl);
            if (!string.IsNullOrEmpty(txt)){
                callback(txt);
            } else{
                Net.GetRequest(txtUrl, (data) => {
                    callback(data);
                    DataStorage.SaveString(txtUrl, data);
                }, (code, msg) => { callback(""); });
            }
        }
    }
}