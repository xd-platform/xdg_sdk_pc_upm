using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TapTap.Common;
using UnityEngine;
using Json = com.xd.intl.pc.MiniJSON.Json;

namespace com.xd.intl.pc{
    public class LanguageMg{
        private static LanguageType anguageType = LanguageType.CN;
        private static string totalJson = null;
        private static LanguageModel currentModel = null;

        public static void SetLanguageType(LanguageType type){
            anguageType = type;
            UpdateLanguageModel();
        }

        public static LanguageType GetCurrentType(){
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
            if (anguageType == LanguageType.CN){
                return @"zh_CN";
            } else if (anguageType == LanguageType.TW){
                return @"zh_TW";
            } else if (anguageType == LanguageType.US){
                return @"en_US";
            } else if (anguageType == LanguageType.TH){
                return @"th_TH";
            } else if (anguageType == LanguageType.ID){
                return @"in_ID";
            } else if (anguageType == LanguageType.KR){
                return @"ko_KR";
            } else if (anguageType == LanguageType.JP){
                return @"ja_JP";
            } else if (anguageType == LanguageType.DE){
                return @"de_DE";
            } else if (anguageType == LanguageType.FR){
                return @"fr_FR";
            } else if (anguageType == LanguageType.PT){
                return @"pt_PT";
            } else if (anguageType == LanguageType.ES){
                return @"es_ES";
            } else if (anguageType == LanguageType.TR){
                return @"tr_TR";
            } else if (anguageType == LanguageType.RU){
                return @"ru_RU";
            }
            return @"zh_CN";
        }
        
        public static string GetCustomerCenterLang(){
            if (anguageType == LanguageType.CN){
                return @"cn";
            } else if (anguageType == LanguageType.TW){
                return @"tw";
            } else if (anguageType == LanguageType.US){
                return @"us";
            } else if (anguageType == LanguageType.TH){
                return @"th";
            } else if (anguageType == LanguageType.ID){
                return @"id";
            } else if (anguageType == LanguageType.KR){
                return @"kr";
            } else if (anguageType == LanguageType.JP){
                return @"jp";
            } else if (anguageType == LanguageType.DE){
                return @"de";
            } else if (anguageType == LanguageType.FR){
                return @"fr";
            } else if (anguageType == LanguageType.PT){
                return @"pt";
            } else if (anguageType == LanguageType.ES){
                return @"es";
            } else if (anguageType == LanguageType.TR){
                return @"tr";
            } else if (anguageType == LanguageType.RU){
                return @"ru";
            }
            return @"cn";
        }

        private static string GetJsonPath(){
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