namespace SDK.PC{
    public class SyncTokenModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public string sessionToken{ get; set; }
        }
    }
}