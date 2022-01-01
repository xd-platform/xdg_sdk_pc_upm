namespace com.xd.intl.pc{
    public class SyncTokenModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public string sessionToken{ get; set; }
        }
    }
}