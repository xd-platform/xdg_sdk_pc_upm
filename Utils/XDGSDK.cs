using System;
using System.Collections.Generic;
using TapTap.Common;
using TapTap.Login;
using UnityEngine;
using Newtonsoft.Json;
using TapTap.Bootstrap;
using UnityEngine.Networking;

namespace SDK.PC{
    public class XDGSDK{
        private readonly static string VERSION = "6.0.0";
        public static bool Tmp_IsInited = false;
        public static bool Tmp_IsInitSDK_ing = false;

        public static void InitSDK(string sdkClientId, Action<bool> callback){
            if (Tmp_IsInitSDK_ing){
                XDGSDK.Log("正在初始化中...");
                return;
            }

            if (Tmp_IsInited){
                callback(true);
                XDGSDK.Log("已经初始化过了");
                return;
            }

            Tmp_IsInitSDK_ing = true;
            XDGUserModel.ClearUserData();
            Api.GetIpInfo((success, model) => {
                if (success){
                    Api.InitSDK(sdkClientId, callback);
                } else{
                    callback(false);
                    Tmp_IsInitSDK_ing = false;
                }
            });
        }

        public static void LoginTyType(LoginType loginType, Action<bool, XDGUserModel> callback){
            if (!IsInited()){
                XDGSDK.Log("请先初始化！");
                callback(false, null);
                return;
            }
            var md = XDGUserModel.GetLocalModel();
            if (md != null){
                XDGSDK.Log("已经登录了");
                callback(true, md);
                return;
            }
            Api.LoginTyType(loginType, callback);
        }

        public static void GetUserInfo(Action<bool, XDGUserModel> callback){
            if (!IsInited()){
                XDGSDK.Log("请先初始化！");
                callback(false, null);
                return;
            }

            if (TokenModel.GetLocalModel() == null){
                XDGSDK.Log("请先登录！");
                callback(false, null);
                return;
            }

            Api.GetUserInfo(callback);
        }

        public static bool IsPushServiceEnable(){
            return XDGUserModel.IsPushServiceEnable();
        }

        public static void SetPushServiceEnable(bool enable){
            XDGUserModel.SetPushServiceEnable(enable);
        }

        public static void Logout(){
            XDGUserModel.ClearUserData();
            TapLogin.Logout();
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

        public static bool IsInited(){
            return Tmp_IsInited;
        }

        public static void SetLanguage(LanguageType type){
            LanguageMg.SetLanguageType(type);
        }

        public static void Log(string msg){
            Debug.Log("-------------SDK 打印-------------\n" + msg + "\n\n");
        }

        public static void LogError(string msg){
            Debug.LogError("-------------SDK 报错-------------\n" + msg + "\n\n");
        }

        public static T GetModel<T>(string json) where T : BaseModel{
            if (string.IsNullOrEmpty(json)){
                return null;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetJson<T>(T model) where T : BaseModel{
            if (model == null){
                return null;
            }

            return JsonConvert.SerializeObject(model);
        }

        public static void CheckPay(){
            Api.checkPay((success, model) => {
                if (success && model.data.list != null && model.data.list.Count > 0){
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
                            {"hasIOS",hasIOS ?  1 : 0},
                            {"hasAndroid",hasAndroid ? 1 : 0},
                        };
                        UIManager.ShowUI<PayHintAlert>(param, null);   
                    }
                }
            });
        }
    }
}