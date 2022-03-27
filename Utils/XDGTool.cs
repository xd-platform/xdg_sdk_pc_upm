using UnityEngine;

namespace com.xd.intl.pc{
    public class XDGTool{
        public static bool hasNetwork(){
            if (Application.internetReachability == NetworkReachability.NotReachable){
               return false;
            }
            return true;
        }
    }
}