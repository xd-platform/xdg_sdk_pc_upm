using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TapTap.Common;
using UnityEngine;
using Json = com.xd.intl.pc.MiniJSON.Json;

namespace com.xd.intl.pc{
    public class LanguageMg{
        private static LangType anguageType = LangType.ZH_CN;
        private static string totalJson = null;
        private static LanguageModel currentModel = null;

        public static void SetLanguageType(LangType type){
            anguageType = type;
            UpdateLanguageModel();
        }

        public static LangType GetCurrentType(){
            return anguageType;
        }

        public static LanguageModel GetCurrentModel(){
            if (currentModel == null){
                UpdateLanguageModel();
            }
            return currentModel;
        }

        private static void UpdateLanguageModel(){
            if (totalJson == null){
                var txtAsset = Resources.Load("Language") as TextAsset;
                if (txtAsset != null){
                    totalJson = txtAsset.text;
                }
            }

            var totalDic = Json.Deserialize(totalJson) as Dictionary<string, object>;
            var tmpObj = SafeDictionary.GetValue<Dictionary<string, object>>(totalDic, GetLanguageKey());
            string modelJson = JsonConvert.SerializeObject(tmpObj);
            currentModel = XDGSDK.GetModel<LanguageModel>(modelJson);
        }

        public static string GetLanguageKey(){
            if (anguageType == LangType.ZH_CN){
                return @"zh_CN";
            } else if (anguageType == LangType.ZH_TW){
                return @"zh_TW";
            } else if (anguageType == LangType.EN){
                return @"en_US";
            } else if (anguageType == LangType.TH){
                return @"th_TH";
            } else if (anguageType == LangType.ID){
                return @"in_ID";
            } else if (anguageType == LangType.KR){
                return @"ko_KR";
            } else if (anguageType == LangType.JP){
                return @"ja_JP";
            } else if (anguageType == LangType.DE){
                return @"de_DE";
            } else if (anguageType == LangType.FR){
                return @"fr_FR";
            } else if (anguageType == LangType.PT){
                return @"pt_PT";
            } else if (anguageType == LangType.ES){
                return @"es_ES";
            } else if (anguageType == LangType.TR){
                return @"tr_TR";
            } else if (anguageType == LangType.RU){
                return @"ru_RU";
            }
            return @"en_US";
        }
        
        public static string GetCustomerCenterLang(){
            if (anguageType == LangType.ZH_CN){
                return @"cn";
            } else if (anguageType == LangType.ZH_TW){
                return @"tw";
            } else if (anguageType == LangType.EN){
                return @"us";
            } else if (anguageType == LangType.TH){
                return @"th";
            } else if (anguageType == LangType.ID){
                return @"id";
            } else if (anguageType == LangType.KR){
                return @"kr";
            } else if (anguageType == LangType.JP){
                return @"jp";
            } else if (anguageType == LangType.DE){
                return @"de";
            } else if (anguageType == LangType.FR){
                return @"fr";
            } else if (anguageType == LangType.PT){
                return @"pt";
            } else if (anguageType == LangType.ES){
                return @"es";
            } else if (anguageType == LangType.TR){
                return @"tr";
            } else if (anguageType == LangType.RU){
                return @"ru";
            }
            return @"us";
        }

        private static string GetJsonPath(){//废弃，app的方式，pc不可以
            var parentFolder = Directory.GetParent(Application.dataPath)?.FullName;
            var jsonPath = FilterFile(parentFolder + "/Library/PackageCache/", "com.xd.intl.pc@");
            if (string.IsNullOrEmpty(jsonPath)){
                jsonPath = parentFolder + "/Assets/XDGSDK";
            }
            jsonPath = jsonPath + "/Assets/Language/Language.json";
            XDGSDK.Log("language json path: " + jsonPath);
            return jsonPath;
        }

        private static string FilterFile(string srcPath, string filterName){
            if (!Directory.Exists(srcPath)){
                return null;
            }

            foreach (var dir in Directory.GetDirectories(srcPath)){
                string fileName = Path.GetFileName(dir);
                if (fileName.StartsWith(filterName)){
                    XDGSDK.Log("筛选到指定文件夹:" + Path.Combine(srcPath, Path.GetFileName(dir)));
                    return Path.Combine(srcPath, Path.GetFileName(dir));
                }
            }

            return null;
        }
    }
}