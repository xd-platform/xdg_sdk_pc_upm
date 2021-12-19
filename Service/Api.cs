using System;
using System.Collections.Generic;
using TapTap.Login;

namespace SDK.PC{
    public class Api{
        private readonly static string BASE_URL = "http://test-xdsdk-intnl-6.xd.com";

        //获取配置
        private readonly static string INIT_SDK = BASE_URL + "/api/init/v1/config";

        //IP信息
        private readonly static string IP_INFO = "https://ip.xindong.com/myloc2";

        // login
        private readonly static string XDG_USER_INFO = BASE_URL + @"/api/account/v1/info";

        // 与leanClound同步
        private readonly static string XDG_LOGIN_SYN = BASE_URL + @"/api/login/v1/syn";

        // 获取用户绑定信息
        private readonly static string XDG_CHECK_BIND_STATU = BASE_URL + @"/api/account/v1/bind/list";

        // 三方绑定接口
        private readonly static string XDG_BIND_INTERFACE = BASE_URL + @"/api/account/v1/bind";

        // 三方解绑接口
        private readonly static string XDG_UNBIND_INTERFACE = BASE_URL + @"/api/account/v1/unbind";
        
        private readonly static string TDSG_GLOBAL_SDK_DOMAIN = @"https://xdg-1c20f-intl.xd.com";


        public static void InitSDK(string clientId, string countryCode, string sdkLang,
            Action<bool, InitConfigModel.Data> callback){
            Dictionary<string, object> param = new Dictionary<string, object>{
                {"clientId", clientId},
                {"countryCode", countryCode},
                {"sdkLang", sdkLang}
            };

            Net.GetRequest(INIT_SDK, param, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                InitConfigModel.SaveToLocal(model);
                TapLogin.Init("jfqhF3x9mat70ez52i", false, false);
                callback(true, model.data);
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false, null);
            });
        }

        public static void GetIpInfo(Action<bool, IpInfoModel> callback){
            RequestIpInfo(true, callback);
        }

        private static void RequestIpInfo(bool repeat, Action<bool, IpInfoModel> callback){
            Net.GetRequest(IP_INFO, null, (data) => {
                var model = XDGSDK.GetModel<IpInfoModel>(data);
                IpInfoModel.SaveToLocal(model);
                callback(true, model);
            }, (code, msg) => {
                if (repeat){
                    RequestIpInfo(false, callback);
                } else{
                    var oldMd = IpInfoModel.GetLocalModel();
                    if (oldMd != null){
                        callback(true, oldMd);
                    } else{
                        XDGSDK.Log("获取 ip info 失败 code: " + code + " msg: " + msg);
                        callback(false, null);
                    }
                }
            });
        }

    }
}