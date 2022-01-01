using System.Collections.Generic;

namespace com.xd.intl.pc{
    public class BindModel : BaseModel{
        public List<Data> data{ get; set; }
        
        public class Data{
            public int loginType{ get; set; }
            public string loginName{ get; set; }
            public string nickName{ get; set; }
            public string avatar{ get; set; }
            public string bindDate{ get; set; }
            public int status{ get; set; }
        }
    }
}