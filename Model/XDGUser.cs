using System.Collections.Generic;

namespace com.xd.intl.pc{
    public class XDGUser{
        public string userId{ get; set; }
        public string username{ get; set; }  // name
        public string nickName{ get; set; }
        public string avatar{ get; set; }
        public long loginType{ get; set; }
        public List<string> loginList{ get; set; }  // boundAccounts 
        public XDGAccessToken token{
            get{
                var tmd = TokenModel.GetLocalModel();
                if (tmd != null){
                    return tmd.data;
                }
                return null;
            }
        }

        public LoginType GetLoginType(){
            if (this.loginType == 0){
                return LoginType.Guest;
            } else if (this.loginType == 5){
                return LoginType.TapTap;
            }

            return LoginType.Guest;
        }
    }
}