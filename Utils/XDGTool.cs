using UnityEngine;

namespace com.xd.intl.pc{
    public class XDGTool{
        public static bool HasNetwork(){
            if (Application.internetReachability == NetworkReachability.NotReachable){
                return false;
            }

            return true;
        }

        public static string GetDid(){
            //测试用 start-----
            // var clientId = DataStorage.LoadString(DataStorage.ClientId);
            // if ("hn5RcJei2JxCYlS0".Equals(clientId)){ //demo的id
            //     var str = "";
            //     if (Api.BASE_URL.Contains("test")){
            //         str = "dev";
            //     } else{
            //         str = "res";
            //     }
            //     return SystemInfo.deviceUniqueIdentifier + str;
            // }
            //测试用 end-----
            
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
}