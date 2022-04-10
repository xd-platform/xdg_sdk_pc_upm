using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;
using Random = System.Random;

namespace com.xd.intl.pc{
    [DisallowMultipleComponent]
    public class Net : MonoBehaviour{
        private static GameObject netObject;

        public static void GetRequest(string url,
            Dictionary<string, object> parameters,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            if (string.IsNullOrEmpty(url)){
                methodForError(-1, "empty url");
                return;
            }

            Net nt = Instance();
            nt.StartCoroutine(nt.Get(url, parameters, methodForResult, methodForError));
        }

        public static void GetRequest(string url,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            if (string.IsNullOrEmpty(url)){
                methodForError(-1, "empty url");
                return;
            }

            Net nt = Instance();
            nt.StartCoroutine(nt.Get(url, null, methodForResult, methodForError));
        }

        public static void PostRequest(string url,
            Dictionary<string, object> parameters,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            if (string.IsNullOrEmpty(url)){
                methodForError(-1, "empty url");
                return;
            }

            Net nt = Instance();
            nt.StartCoroutine(nt.Post(url, null, parameters, methodForResult, methodForError));
        }

        public static void PostRequest(string url, Dictionary<string, string> headers,
            Dictionary<string, object> parameters,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            if (string.IsNullOrEmpty(url)){
                methodForError(-1, "empty url");
                return;
            }

            Net nt = Instance();
            nt.StartCoroutine(nt.Post(url, headers, parameters, methodForResult, methodForError));
        }

        private static Net Instance(){
            if (netObject == null){
                netObject = new GameObject{
                    name = "SDKNet"
                };
                netObject.AddComponent<Net>();
                DontDestroyOnLoad(netObject);
            }

            return netObject.GetComponent<Net>();
        }

        private IEnumerator Post(string url, Dictionary<string, string> headers, Dictionary<string, object> parameters,
            Action<string> methodForResult, Action<int, string> methodForError){
            string finalUrl = url + "?" + DictToQueryString2(GetCommonParam(url));
            Dictionary<string, object> finalParameter = parameters ?? new Dictionary<string, object>();

            String jsonString = MiniJSON.Json.Serialize(finalParameter);
            Byte[] formData = Encoding.UTF8.GetBytes(jsonString);

            UnityWebRequest w = UnityWebRequest.Post(finalUrl, "");
            w.uploadHandler = new UploadHandlerRaw(formData);
            w.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            w.SetRequestHeader("Accept-Language", LanguageMg.GetLanguageKey());
            w.timeout = 6;

            var auth = GetMacToken(finalUrl, "POST");
            if (!string.IsNullOrEmpty(auth)){
                w.SetRequestHeader("Authorization", auth);
            }

            if (headers != null){
                Dictionary<string, string>.KeyCollection keys = headers.Keys;
                foreach (string headerKey in keys){
                    w.SetRequestHeader(headerKey, headers[headerKey]);
                }
            }

            yield return w.SendWebRequest();

            if (!string.IsNullOrEmpty(w.error)){
                XDGSDK.Log("数据失败：\n" + finalUrl + "\n\n" + w.downloadHandler.text);
                XDGSDK.Log("ERROR 信息：" + w.error);
                string data = w.downloadHandler.text;
                if (!string.IsNullOrEmpty(data)){
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    methodForError(md.code, md.msg);
                } else{
                    var lmd = LanguageMg.GetCurrentModel();
                    var str = (lmd == null ? "Network Error" : lmd.tds_network_error_safe_retry);
                    methodForError(-1, str);
                }
                w.Dispose();
                yield break;
            } else{
                string data = w.downloadHandler.text;
                if (data != null){
                    XDGSDK.Log("发起Post请求：" + finalUrl + "\n\nbody参数：" + jsonString + "\n\n响应结果：" + data);
                    methodForResult(data);
                    yield break;
                } else{
                    XDGSDK.Log("请求失败，response 为空。url: " + finalUrl);
                    var lmd = LanguageMg.GetCurrentModel();
                    var str = (lmd == null ? "Network Error" : lmd.tds_network_error_safe_retry);
                    methodForError(-1, str);
                }
            }
        }

        private IEnumerator Get(string url, Dictionary<string, object> parameters,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            string finalUrl = url + "?" + DictToQueryString(parameters) + "&" + DictToQueryString2(GetCommonParam(url));

            UnityWebRequest w = UnityWebRequest.Get(finalUrl);
            w.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            w.SetRequestHeader("Accept-Language", LanguageMg.GetLanguageKey());
            w.timeout = 6;

            var auth = GetMacToken(finalUrl, "GET");
            if (!string.IsNullOrEmpty(auth)){
                w.SetRequestHeader("Authorization", auth);
            }

            yield return w.SendWebRequest();

            if (!string.IsNullOrEmpty(w.error)){
                XDGSDK.Log("数据失败：\n" + finalUrl + "\n\n" + w.downloadHandler.text);
                XDGSDK.Log("ERROR 信息：" + w.error);
                string data = w.downloadHandler.text;
                if (!string.IsNullOrEmpty(data)){
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    methodForError(md.code, md.msg);
                } else{
                    var lmd = LanguageMg.GetCurrentModel();
                    var str = (lmd == null ? "Network Error" : lmd.tds_network_error_safe_retry);
                    methodForError(-1, str);
                }
                w.Dispose();
                yield break;
            } else{
                string data = w.downloadHandler.text;
                if (data != null){
                    XDGSDK.Log("发起Get请求：" + finalUrl + "\n\n响应结果：" + data);
                    methodForResult(data);
                    yield break;
                } else{
                    XDGSDK.Log("请求失败，response 为空。url: " + finalUrl);
                    var lmd = LanguageMg.GetCurrentModel();
                    var str = (lmd == null ? "Network Error" : lmd.tds_network_error_safe_retry);
                    methodForError(-1, str);
                }
            }
        }

        private static string DictToQueryString(IDictionary<string, object> dict){
            if (dict == null){
                return "";
            }

            List<string> list = new List<string>();
            foreach (var item in dict){
                list.Add(item.Key + "=" + item.Value);
            }

            return string.Join("&", list.ToArray());
        }

        private static string DictToQueryString2(IDictionary<string, string> dict){
            if (dict == null){
                return "";
            }

            List<string> list = new List<string>();
            foreach (var item in dict){
                list.Add(item.Key + "=" + item.Value);
            }

            return string.Join("&", list.ToArray());
        }

        private static string GetMacToken(string url, string method){
            TokenModel model = TokenModel.GetLocalModel();
            string authToken = null;
            if (model != null && model.data != null){
                var uri = new Uri(url);
                var timeStr = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
                var nonce = GetRandomStr(5);
                var md = method.ToUpper();

                var pathAndQuery = uri.PathAndQuery;
                var domain = uri.Host.ToLower();
                var port = uri.Port + "";

                var dataStr = $"{timeStr}\n{nonce}\n{md}\n{pathAndQuery}\n{domain}\n{port}\n";
                var mac = Base64WithSecret(model.data.macKey, dataStr);
                authToken = $"MAC id=\"{model.data.kid}\",ts=\"{timeStr}\",nonce=\"{nonce}\",mac=\"{mac}\"";
            }

            return authToken;
        }

        private static string Base64WithSecret(string secret, string data){
            Byte[] secretBytes = UTF8Encoding.UTF8.GetBytes(secret);
            HMACSHA1 hmac = new HMACSHA1(secretBytes);
            Byte[] dataBytes = UTF8Encoding.UTF8.GetBytes(data);
            Byte[] calcHash = hmac.ComputeHash(dataBytes);
            String calcHashString = Convert.ToBase64String(calcHash);
            return calcHashString;
        }

        private static Dictionary<string, string> GetCommonParam(string url){
            if (url.Contains("ip.xindong.com")){
                return null;
            }

            var clientId = DataStorage.LoadString(DataStorage.ClientId);
            var cfgMd = InitConfigModel.GetLocalModel();
            var ipMd = IpInfoModel.GetLocalModel();
            if (ipMd == null){
                ipMd = new IpInfoModel();
            }

            Dictionary<string, string> param = new Dictionary<string, string>{
                {"clientId", string.IsNullOrEmpty(clientId) ? "" : clientId},
                {"appId", cfgMd == null ? "" : cfgMd.data.configs.appId + ""},
                {"did", XDGTool.GetDid()},
                {"sdkLang", LanguageMg.GetLanguageKey()},
                {"lang", LanguageMg.GetLanguageKey()},
                {"loc", ipMd.country_code},
                {"time", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + ""},
                {"chn", "PC"},
                {"city", ipMd.city},
                {"timeZone", ipMd.timeZone},
                {"countryCode", ipMd.country_code},
                {"locationInfoType", "ip"},
                {"mem", SystemInfo.systemMemorySize / 1024 + "GB"},
                {"res", Screen.width + "_" + Screen.height},
                {"mod", SystemInfo.deviceModel},
                {"sdkVer", XDGSDK.GetSdkVersion()},
                {"pkgName", Application.identifier},
                {"brand", SystemInfo.graphicsDeviceVendor},
                {"os", SystemInfo.operatingSystem},
                {"pt", GetPlatform()},
                {"appVer", Application.version},
                {"appVerCode", Application.version},
                {"cpu", SystemInfo.processorType}
            };
            return param;
        }

        public static string GetCustomerCenterUrl(string serverId, string roleId, string roleName){
            var clientId = DataStorage.LoadString(DataStorage.ClientId);
            var userMd = XDGUserModel.GetLocalModel();
            var cfgMd = InitConfigModel.GetLocalModel();
            var tkModel = TokenModel.GetLocalModel();
            if (userMd == null){
                return null;
            }

            var uri = new Uri(cfgMd.data.configs.reportUrl);
            var url = $"{uri.Scheme}://{uri.Host}";
            Dictionary<string, string> param = new Dictionary<string, string>{
                {"client_id", string.IsNullOrEmpty(clientId) ? "" : clientId},
                {"access_token", tkModel.data.kid},
                {"user_id", userMd.data.userId},
                {"server_id", serverId},
                {"role_id", roleId},
                {"role_name", roleName},
                {"region", cfgMd.data.configs.region},
                {"sdk_ver", XDGSDK.GetSdkVersion()},
                {"sdk_lang", LanguageMg.GetCustomerCenterLang()},
                {"app_ver", Application.version},
                {"app_ver_code", Application.version},
                {"res", Screen.width + "_" + Screen.height},
                {"cpu", SystemInfo.processorType},
                {"mem", SystemInfo.systemMemorySize / 1024 + "GB"},
                {"pt", GetPlatform()},
                {"os", SystemInfo.operatingSystem},
                {"brand", SystemInfo.graphicsDeviceVendor},
                {"game_name", Application.productName},
            };
            return url + "?" + DictToQueryString2(param);
        }

        public static string GetPayUrl(string serverId, string roleId){
            var userMd = XDGUserModel.GetLocalModel();
            var cfgMd = InitConfigModel.GetLocalModel();
            if (userMd == null){
                return null;
            }

            var uri = new Uri(cfgMd.data.configs.webPayUrl);
            var url = $"{uri.Scheme}://{uri.Host}";
            var appId = cfgMd.data.configs.appId;

            var lang = LanguageMg.GetLanguageKey();
            Dictionary<string, string> param = new Dictionary<string, string>{
                {"serverId", serverId},
                {"roleId", roleId},
                {"region", cfgMd.data.configs.region},
                {"appId", appId},
                {"lang", lang},
            };
            return url + "?" + DictToQueryString2(param);
        }

        private static string GetPlatform(){
            string os = "Linux";
#if UNITY_STANDALONE_OSX
            os = "macOS";
#endif
#if UNITY_STANDALONE_WIN
            os = "Windows";
#endif
            return os;
        }

        private static string LetterStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static string GetRandomStr(int length){
            StringBuilder SB = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < length; i++){
                SB.Append(LetterStr.Substring(rd.Next(0, LetterStr.Length), 1));
            }

            return SB.ToString();
        }
    }
}