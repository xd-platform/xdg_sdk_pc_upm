using System;
using UnityEngine;

namespace SDK.PC{
    public class XDGSDK{
        private readonly static string VERSION = "6.0.0"; //SDK 版本号

        public static void InitSDK(string appId, Action<bool> callback){
            Api.InitSDK(appId, callback);
        }

        public static string GetSdkVersion(){
            return VERSION;
        }

        public static void Log(string msg){
            Debug.Log("-------------SDK 打印-------------\n" + msg + "\n\n");
        }

        public static void LogError(string msg){
            Debug.LogError("-------------SDK 报错-------------\n" + msg + "\n\n");
        }
    }
}