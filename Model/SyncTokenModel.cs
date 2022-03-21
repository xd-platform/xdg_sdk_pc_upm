using System;

namespace com.xd.intl.pc{
    public class SyncTokenModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public string sessionToken{ get; set; }
        }

        public static void SaveToLocal(string token){
            DataStorage.SaveString(DataStorage.SessionTokenKey, token);
        }

        public static string GetLocalToken(){
           return DataStorage.LoadString(DataStorage.SessionTokenKey);
        }
        
        public static void ClearToken(){
            DataStorage.SaveString(DataStorage.SessionTokenKey, "");
        }

    }
}