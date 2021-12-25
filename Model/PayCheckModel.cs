using System.Collections.Generic;

namespace SDK.PC{
    public class PayCheckModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public List<InfoModel> list{ get; set; }
        }

        public class InfoModel{
            public string tradeNo{ get; set; }
            public string outTradeNo{ get; set; }
            public string productId{ get; set; }
            public string currency{ get; set; }
            public string channelType{ get; set; }
            public double refundAmount{ get; set; }
            public long supplyStatus{ get; set; }
            public long platform{ get; set; }  //1:ios 2:android
            public string channelId{ get; set; }
        }
    }
}