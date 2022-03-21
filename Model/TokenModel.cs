namespace com.xd.intl.pc{
    public class TokenModel : BaseModel{
        public XDGAccessToken data{ get; set; }
        
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