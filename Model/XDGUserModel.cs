using System.Collections.Generic;

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
                } else{
                    XDGSDK.Log("User Model 为空");
                }
            }

            return currentMd;
        }

        public static void Clear(){
            currentMd = null;
            DataStorage.SaveString(DataStorage.UserInfo, "");
            InitConfigModel.ClearLocalModel();
            TokenModel.ClearLocalModel();
        }

        public static bool IsLogin(){
            return currentMd == null ? false : true;
        }
    }
}