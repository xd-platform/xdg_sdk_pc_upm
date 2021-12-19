using UnityEngine;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine.Networking;
using Random = System.Random;

namespace SDK.PC{
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
            Dictionary<string, object> finalParameter = parameters ?? new Dictionary<string, object>();

#if UNITY_STANDALONE_OSX
            finalParameter.Add("system", "OSX");
#endif

#if UNITY_STANDALONE_WIN
            finalParameter.Add("system", "Windows");
#endif
            String jsonString = MiniJSON.Json.Serialize(finalParameter);
            UnityWebRequest w = UnityWebRequest.Post(url, jsonString);
            w.SetRequestHeader("Content-Type", "application/json");
            w.timeout = 15;

            var auth = GetMacToken(url, "POST");
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

            if (!string.IsNullOrEmpty(w.error) || w.isHttpError || w.isNetworkError){
                XDGSDK.LogError(w.error);
                string data = w.downloadHandler.text;
                if (data != null){
                    methodForError(GetResponseCode(w), data);
                } else{
                    methodForError(GetResponseCode(w), w.error);
                }

                w.Dispose();
                yield break;
            } else{
                string data = w.downloadHandler.text;
                if (data != null){
                    string heads = DictToQueryString2(w.GetResponseHeaders());
                    XDGSDK.Log("发起Post请求：" + url + "\n\n请求头：" + heads + "\n\n参数：" + jsonString + "\n\n响应结果：" + data);
                    methodForResult(data);
                    yield break;
                } else{
                    XDGSDK.Log("请求失败，response 为空。url: " + url);
                    methodForError(GetResponseCode(w), "Empyt response from server : " + url);
                }
            }
        }

        private IEnumerator Get(string url, Dictionary<string, object> parameters,
            Action<string> methodForResult,
            Action<int, string> methodForError){
            string finalUrl = "";
            string system = "";

            Dictionary<string, object> finalParameter = parameters ?? new Dictionary<string, object>();

#if UNITY_STANDALONE_OSX
            system = "OSX";
#endif

#if UNITY_STANDALONE_WIN
            system = "Windows";
#endif
            finalParameter.Add("system", system);
            NameValueCollection collection = new NameValueCollection();
            string baseUrl;

            ParseUrl(url, out baseUrl, out collection);

            foreach (string key in collection.AllKeys){
                foreach (string value in collection.GetValues(key)){
                    finalParameter.Add(key, value);
                }
            }

            finalUrl = baseUrl + "?" + DictToQueryString(finalParameter);

            UnityWebRequest w = UnityWebRequest.Get(finalUrl);
            w.SetRequestHeader("Content-Type", "application/json");
            w.timeout = 15;

            var auth = GetMacToken(url, "GET");
            if (!string.IsNullOrEmpty(auth)){
                w.SetRequestHeader("Authorization", auth);
            }

            yield return w.SendWebRequest();

            if (!string.IsNullOrEmpty(w.error) || w.isHttpError || w.isNetworkError){
                XDGSDK.LogError(w.error);
                methodForError(GetResponseCode(w), w.error);
                w.Dispose();
                yield break;
            } else{
                string data = w.downloadHandler.text;
                if (data != null){
                    string heads = DictToQueryString2(w.GetResponseHeaders());
                    XDGSDK.Log("发起Get请求：" + finalUrl + "\n\n请求头：" + heads + "\n\n响应结果：" + data);
                    methodForResult(data);
                    yield break;
                } else{
                    XDGSDK.Log("请求失败，response 为空。url: " + url);
                    methodForError(GetResponseCode(w), "Empty response from server : " + url);
                }
            }
        }

        private static int GetResponseCode(UnityWebRequest request){
            int ret = 0;
            if (request.GetResponseHeaders() == null){
                XDGSDK.LogError("no response headers.");
            } else{
                if (!request.GetResponseHeaders().ContainsKey("STATUS")){
                    XDGSDK.LogError("response headers has no STATUS.");
                } else{
                    ret = ParseResponseCode(request.GetResponseHeaders()["STATUS"]);
                }
            }

            return ret;
        }

        private static int ParseResponseCode(string statusLine){
            int ret = 0;

            string[] components = statusLine.Split(' ');
            if (components.Length < 3){
                XDGSDK.LogError("invalid response status: " + statusLine);
            } else{
                if (!int.TryParse(components[1], out ret)){
                    XDGSDK.LogError("invalid response code: " + components[1]);
                }
            }

            return ret;
        }

        private static string DictToQueryString(IDictionary<string, object> dict){
            List<string> list = new List<string>();
            foreach (var item in dict){
                list.Add(item.Key + "=" + item.Value);
            }

            return string.Join("&", list.ToArray());
        }

        private static string DictToQueryString2(IDictionary<string, string> dict){
            List<string> list = new List<string>();
            foreach (var item in dict){
                list.Add(item.Key + "=" + item.Value);
            }

            return string.Join("&", list.ToArray());
        }

        private static void ParseUrl(string url, out string baseUrl, out NameValueCollection nvc){
            if (url == null)
                throw new ArgumentNullException("url");
            nvc = new NameValueCollection();
            baseUrl = "";
            if (url == "")
                return;
            int questionMarkIndex = url.IndexOf('?');
            if (questionMarkIndex == -1){
                baseUrl = url;
                return;
            }

            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
                return;
            string ps = url.Substring(questionMarkIndex + 1);
            Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.None);
            MatchCollection mc = re.Matches(ps);
            foreach (Match m in mc){
                nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
            }
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

        private static Dictionary<string, string> GetCommonParam(){
            var paramMd = ParamConfigModel.Instance();
            var cfgMd = InitConfigModel.GetLocalModel();
            var ipMd = IpInfoModel.GetLocalModel();
            if (ipMd == null){
                ipMd = new IpInfoModel();
            }

            Dictionary<string, string> param = new Dictionary<string, string>{
                {"clientId", paramMd.clientId},
                {"appId", cfgMd == null ? "" : cfgMd.data.configs.appId + ""},
                {"did", SystemInfo.deviceUniqueIdentifier},
                {"sdkLang", LanguageMg.GetLanguageKey()},
                {"lang", Application.systemLanguage.ToString()},
                {"loc", ipMd.country_code},
                {"time", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + ""},
                {"chn", "PC"},
                {"city", ipMd.city},
                {"timeZone", ipMd.timeZone},
                {"countryCode", ipMd.country_code},
                {"locationInfoType", "ip"},
                {"mem", SystemInfo.systemMemorySize / 1024 + "GB"},
                {"res", paramMd.screenWidth + "_" + paramMd.screenHeight},
                {"mod", SystemInfo.deviceModel},
                {"sdkVer", XDGSDK.GetSdkVersion()},
                {"pkgName", PlayerSettings.applicationIdentifier},
                {"brand", SystemInfo.graphicsDeviceVendor},
                {"os", SystemInfo.operatingSystem},
                {"pt", GetPlatform()},
                {"appVer", PlayerSettings.bundleVersion},
                {"appVerCode", XDGSDK.GetSdkVersionCode()},
                {"cpu", SystemInfo.processorType}
            };
            return param;
        }

        private static string GetPlatform(){
            string os = "";
            #if UNITY_STANDALONE_OSX
               os = "OSX";
            #endif
            #if UNITY_STANDALONE_WIN
               os = "WIN";
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