using System;
using TapTap.Common;
using TapTap.Login;
using UnityEngine;
using Newtonsoft.Json;

namespace SDK.PC{
    public class XDGSDK{
        private readonly static string VERSION = "6.0.0";
        private readonly static string VERSION_CODE = "6000";
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
            var user = XDGUserModel.GetLocalModel();
            if (user != null){
                return user.data.isPushEnable;
            }
            return false;
        }
        
        public static void SetPushServiceEnable(bool enable){
            var user = XDGUserModel.GetLocalModel();
            if (user != null){
                user.data.isPushEnable = enable;
                XDGUserModel.SaveToLocal(user);
            }
        }

        public static void Logout(){
            XDGUserModel.ClearUserData();
        }
        
        public static string GetSdkVersion(){
            return VERSION;
        }

        public static string GetSdkVersionCode(){
            return VERSION_CODE;
        }

        public static bool IsInited(){
            return Tmp_IsInited;
        }

        public async static void Login(){
            try{
                // 会显示二维码，玩家可以通过移动设备上的 TapTap 客户端扫码登录
                var accessToken = await TapLogin.Login();
                Log($"TapTap 登录成功 accessToken: {accessToken}");
            } catch (Exception e){
                if (e is TapException tapError){
                    if (tapError.code == (int) TapErrorCode.ERROR_CODE_BIND_CANCEL){
                        Log("Tap 登录取消");
                    } else{
                        Log($"Tap 登录失败: code: {tapError.code},  message: {tapError.message}");
                    }
                }
            }

// 获取 TapTap Profile  可以获得当前用户的一些基本信息，例如名称、头像。
            // var profile = await TapLogin.FetchProfile();
            // Debug.Log($"TapTap 登录成功 profile: {profile.ToJson()}");
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
    }
}