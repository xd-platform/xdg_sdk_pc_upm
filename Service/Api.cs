using System;
using System.Collections.Generic;
using TapTap.Login;
using UnityEngine;

namespace SDK.PC{
    public class Api{
       
        private readonly static string BASE_URL = "http://test-xdsdk-intnl-6.xd.com"; //测试
        // private readonly static string BASE_URL = " https://xdsdk-intnl-6.xd.com"; //正式

        //获取配置
        private readonly static string INIT_SDK = BASE_URL + "/api/init/v1/config";

        //IP信息
        private readonly static string IP_INFO = "https://ip.xindong.com/myloc2";

        // login
        private readonly static string XDG_USER_PROFILE = BASE_URL + @"/api/account/v1/info";
        
        //游客
        private readonly static string  XDG_COMMON_LOGIN  = BASE_URL +  @"/api/login/v1/union";

        // 与leanClound同步
        private readonly static string XDG_LOGIN_SYN = BASE_URL + @"/api/login/v1/syn";

        // 获取用户绑定信息
        private readonly static string XDG_CHECK_BIND_STATU = BASE_URL + @"/api/account/v1/bind/list";

        // 三方绑定接口
        private readonly static string XDG_BIND_INTERFACE = BASE_URL + @"/api/account/v1/bind";

        // 三方解绑接口
        private readonly static string XDG_UNBIND_INTERFACE = BASE_URL + @"/api/account/v1/unbind";

        private readonly static string TDSG_GLOBAL_SDK_DOMAIN = @"https://xdg-1c20f-intl.xd.com";


        public static void InitSDK(string sdkClientId,
            Action<bool> callback){
            DataStorage.SaveString(DataStorage.ClientId, sdkClientId);
            Net.GetRequest(INIT_SDK, null, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                InitConfigModel.SaveToLocal(model);
                TapLogin.Init(model.data.configs.tapSdkConfig.clientId, false, false);
                callback(true);
                XDGSDK.Tmp_IsInited = true;
                XDGSDK.Tmp_IsInitSDK_ing = false;
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false);
                XDGSDK.Tmp_IsInitSDK_ing = false;
            });
        }

        public static void LoginTyType(LoginType loginType, Action<bool, XDGUserModel> callback){
            Dictionary<string, object> param = new Dictionary<string, object>{
                {"type", (int)loginType},
                {"token", SystemInfo.deviceUniqueIdentifier}
            };
            Net.PostRequest(XDG_COMMON_LOGIN, param, (data) => {
                var model = XDGSDK.GetModel<TokenModel>(data);
                TokenModel.SaveToLocal(model);
                GetUserInfo((success, userMd) => {
                    if (success){
                        callback(true, userMd);
                    } else{
                        callback(false, null);
                    }
                });
            }, (code, msg) => {
                XDGSDK.Log("登录失败 code: " + code + " msg: " + msg);
                callback(false, null);
            });
        }

        public static void GetUserInfo(Action<bool, XDGUserModel> callback){
            Net.GetRequest(XDG_USER_PROFILE, null, (data) => {
                var model = XDGSDK.GetModel<XDGUserModel>(data);
                XDGUserModel.SaveToLocal(model);
                callback(true, model);
            }, (code, msg) => {
                XDGSDK.Log("获取用户信息失败 code: " + code + " msg: " + msg);
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