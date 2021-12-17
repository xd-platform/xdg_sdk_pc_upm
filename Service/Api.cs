using System;
using System.Collections.Generic;
using TapTap.Login;

namespace SDK.PC{
    public class Api{
        private readonly static int SUCESS_CODE = 200;
        private readonly static string BASE_URL = "http://test-xdsdk-intnl-6.xd.com";
        private readonly static string INIT_SDK = BASE_URL + "/api/init/v1/config";

        public static void InitSDK(string clientId, string countryCode, string sdkLang,
            Action<bool, InitConfigModel.Data> callback){
            Dictionary<string, object> param = new Dictionary<string, object>{
                {"clientId", clientId},
                {"countryCode", countryCode},
                {"sdkLang", sdkLang}
            };

            Net.GetRequest(INIT_SDK, param, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                if (model.code == SUCESS_CODE){
                    InitConfigModel.SaveToLocal(model);
                    TapLogin.Init("jfqhF3x9mat70ez52i", false, false);
                    callback(true, model.data);
                } else{
                    callback(false, null);
                    XDGSDK.Log("未知错误 " + model.msg);
                }
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false, null);
            });
        }
    }
}