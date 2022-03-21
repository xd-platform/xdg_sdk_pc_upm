using System.Collections.Generic;

namespace com.xd.intl.pc{
    public class XDGUserModel : BaseModel{
        public XDGUser data{ get; set; }

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
            SyncTokenModel.ClearToken();
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