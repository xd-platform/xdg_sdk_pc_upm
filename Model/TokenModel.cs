namespace SDK.PC{
    public class TokenModel : BaseModel{
        public Data data{ get; set; }

        public class Data{
            public string kid{ get; set; }
            public string tokenType{ get; set; }
            public string macKey{ get; set; }
            
            public long expireIn{ get; set; }  //游客才有
            
            public string macAlgorithm{ get; set; } //tapTap才有
        }

        private static TokenModel currentMd = null;

        public static void SaveToLocal(TokenModel model){
            if (model != null){
                string json = XDGSDK.GetJson(model);
                DataStorage.SaveString(DataStorage.TokenInfo, json);
                currentMd = model;
            }
        }

        public static TokenModel GetLocalModel(){
            if (currentMd == null){
                string json = DataStorage.LoadString(DataStorage.TokenInfo);
                if (!string.IsNullOrEmpty(json)){
                    currentMd = XDGSDK.GetModel<TokenModel>(json);
                }
            }
            return currentMd;
        }

        public static void ClearToken(){
            currentMd = null;
            DataStorage.SaveString(DataStorage.TokenInfo, "");
        }
    }
}