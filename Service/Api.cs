using System;
using System.Collections.Generic;

namespace SDK.PC{
    public class Api{
    
        private readonly static string BASE_URL = "https://api.xd.com/v1";
        private readonly static string BASE_URL_V2 = "https://api.xd.com/v2";
        private readonly static string INIT_SDK = BASE_URL_V2 + "/sdk/appid_info";

        public static void InitSDK(string appId, Action<bool> callback){
            Dictionary<string, string> param = new Dictionary<string, string>{
                {"client_id", appId}
            };

            Net.GetRequest(INIT_SDK, param, (data) => {
                callback(true);
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false);
            });
        }
        
        
        
    }
}