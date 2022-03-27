using System;
using System.Collections.Generic;
using TapTap.Login;
using UnityEngine;
using Newtonsoft.Json;
using TapTap.Bootstrap;

namespace com.xd.intl.pc{
    public class XDGSDK{
        private readonly static string VERSION = "6.1.1";
        public static bool Tmp_IsInited = false;
        public static bool Tmp_IsInitSDK_ing = false;

        public static void InitSDK(string sdkClientId, Action<bool, string> callback){
            if (Tmp_IsInitSDK_ing){
                return;
            }

            if (Tmp_IsInited){
                callback(true, "已经初始化");
                return;
            }

            Tmp_IsInitSDK_ing = true;
            Api.GetIpInfo((model) => {
                Api.InitSDK(sdkClientId, callback);
            },(error)=>{
                callback(false, error.error_msg);
                Tmp_IsInitSDK_ing = false;
            });
        }

        public static void LoginTyType(LoginType loginType, Action<XDGUser> callback, Action<XDGError> errorCallback){
            if (!IsInitialized()){
                errorCallback(XDGError.msg("Please init first"));
                return;
            }
            Api.LoginTyType(loginType, callback, (e) => {
                LogError($"LoginTyType 登录失败 code:{e.code} msg:{e.error_msg}");
                errorCallback(e);
            });
        }

        public static void GetUserInfo(Action<XDGUser> callback, Action<XDGError> errorCallback){
            if (!IsInitialized()){
                errorCallback(XDGError.msg("Please init first"));
                return;
            }

            if (TokenModel.GetLocalModel() == null){
                errorCallback(XDGError.msg("Please login first"));
                return;
            }

            Api.RequestUserInfo(true, (md) => {
                callback(md.data);
            }, errorCallback);
        }

        public static bool IsPushServiceEnable(){
            return XDGUserModel.IsPushServiceEnable();
        }

        public static void SetPushServiceEnable(bool enable){
            XDGUserModel.SetPushServiceEnable(enable);
        }

        public static async void Logout(){
            await TDSUser.Logout();
            TapLogin.Logout();
            XDGUserModel.ClearUserData(); 
        }

        public static string GetSdkVersion(){
            return VERSION;
        }

        public static void OpenCustomerCenter(string serverId, string roleId, string roleName){
            var url = Net.GetCustomerCenterUrl(serverId, roleId, roleName);
            if (string.IsNullOrEmpty(url)){
                XDGSDK.Log("请先登录游戏");
            } else{
                XDGSDK.Log("客服中心URL: " + url);
                Application.OpenURL(new WWW(url).url);
            }
        }

        public static void OpenUserCenter(){
            if (XDGUserModel.GetLocalModel() == null){
                XDGSDK.Log("请先登录");
                return;
            }
            UIManager.ShowUI<UserCenterAlert>(null, null);
        }

        public static bool IsInitialized(){
            return Tmp_IsInited;
        }

        public static void SetLanguage(LangType type){
            LanguageMg.SetLanguageType(type);
        }

        public static void CheckPay(Action<CheckPayType> callback, Action<XDGError> errorCallback){
            Api.checkPay((model) => {
                    if (model.data.list != null && model.data.list.Count > 0){
                        var hasIOS = false;
                        var hasAndroid = false;
                        foreach (var md in model.data.list){
                            if (md.platform == 1){
                                hasIOS = true;
                            }

                            if (md.platform == 2){
                                hasAndroid = true;
                            }
                        }

                        if (hasIOS || hasAndroid){
                            var param = new Dictionary<string, object>(){
                                {"hasIOS", hasIOS},
                                {"hasAndroid", hasAndroid},
                            };
                            UIManager.ShowUI<PayHintAlert>(param, null);
                        }

                        if (hasIOS && hasAndroid){
                            callback(CheckPayType.iOSAndAndroid);
                        } else if (hasIOS){
                            callback(CheckPayType.iOS);
                        } else if (hasAndroid){
                            callback(CheckPayType.Android);
                        } else{
                            callback(CheckPayType.None);
                        }
                    } else{
                        callback(CheckPayType.None);
                    }
            }, errorCallback);
        }

        public static void OpenWebPay(string serverId, string roleId){
            var url = Net.GetPayUrl(serverId, roleId);
            if (string.IsNullOrEmpty(url)){
                XDGSDK.Log("请先登录游戏");
            } else{
                XDGSDK.Log("支付 pay URL: " + url);
                Application.OpenURL(new WWW(url).url);
            }
        }

        public static void GetLocationInfo(Action<IpInfoModel> callback, Action<XDGError> errorCallback){ //根据ip地址解析的
            var model = IpInfoModel.GetLocalModel();
            if (model == null){
                Api.RequestIpInfo(true, callback, errorCallback);
            } else{
                callback(model);
            }
        }

        public static void Log(string msg){
            Debug.Log($"-------------PCGSDK Log-------------\n {msg} \n\n");
        }

        public static void LogError(string msg){
            var umd = XDGUserModel.GetLocalModel();
            var userId = (umd == null ? "" : umd.data.userId);
            Debug.LogError($"-------------PCGSDK LogError-------------\n userId:【{userId}】{msg} \n\n");
        }

        public static T GetModel<T>(string json) where T : BaseModel{
            if (string.IsNullOrEmpty(json)){
                return null;
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetJson<T>(T model){
            if (model == null){
                return null;
            }

            return JsonConvert.SerializeObject(model);
        }
    }
}