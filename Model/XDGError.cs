using System.Collections.Generic;
using TapTap.Common;

namespace com.xd.intl.pc{
    public class XDGError{
        public int code;
        public string error_msg;

        public XDGError(Dictionary<string, object> dic){
            if (dic != null){
                code = SafeDictionary.GetValue<int>(dic, "code");
                error_msg = SafeDictionary.GetValue<string>(dic, "error_msg");
            }
        }

        public XDGError(int code, string errorMsg){
            this.code = code;
            this.error_msg = errorMsg;
        }

        public static XDGError msg(string msg){
            return new XDGError(-1, msg);
        }
    }
}