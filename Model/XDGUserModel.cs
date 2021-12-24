using System.Collections.Generic;
using PlasticGui.Configuration.CloudEdition.Welcome;

namespace SDK.PC{
    public class XDGUserModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public long appId{ get; set; }
            public string userId{ get; set; }
            public string userCode{ get; set; }
            public string username{ get; set; }
            public string nickName{ get; set; }
            public string avatar{ get; set; }
            public long loginType{ get; set; }
            public string registTime{ get; set; }
            public string registIp{ get; set; }
            public long source{ get; set; }
            public List<string> loginList{ get; set; }
            public bool isGuest{ get; set; }

            public LoginType GetLoginType(){
                if (this.loginType == 0){
                    return LoginType.Guest;
                } else if (this.loginType == 1){
                    return LoginType.TapTap;
                }
                return LoginType.Guest;
            }
        }

        private static XDGUserModel currentMd = null;

        public static void SaveToLocal(XDGUserModel model){
            if (model != null){
                string json = XDGSDK.GetJson(model);
                DataStorage.SaveString(DataStorage.UserInfo, json);
                currentMd = model;
            }
        }

        public static XDGUserModel GetLocalModel(){
            if (currentMd == null){
                string json = DataStorage.LoadString(DataStorage.UserInfo);
                if (!string.IsNullOrEmpty(json)){
                    currentMd = XDGSDK.GetModel<XDGUserModel>(json);
                }
            }

            return currentMd;
        }

        public static void ClearUserData(){
            currentMd = null;
            DataStorage.SaveString(DataStorage.UserInfo, "");
            TokenModel.ClearToken();
        }

        public static bool IsPushServiceEnable(){
            var user = XDGUserModel.GetLocalModel();
            var result = false;
            if (user != null){
                var key = user.data.userId + "_push_key";
                var value = DataStorage.LoadString(key);
                if (!string.IsNullOrEmpty(value) && "1".Equals(value)){
                    result = true;
                }
            }

            return result;
        }

        public static void SetPushServiceEnable(bool enable){
            var user = XDGUserModel.GetLocalModel();
            if (user != null){
                var key = user.data.userId + "_push_key";
                if (enable){
                    DataStorage.SaveString(key, "1");
                } else{
                    DataStorage.SaveString(key, "0");
                }
            }
        }
    }
}