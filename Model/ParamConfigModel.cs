namespace SDK.PC{
    public class ParamConfigModel{
        public string clientId{ get; set; }
        public long screenWidth{ get; set; }
        public long screenHeight{ get; set; }
        

        private static ParamConfigModel currentMd = null;
        public static ParamConfigModel Instance(){
            if (currentMd == null){
                currentMd = new ParamConfigModel();
            }
            return currentMd;
        }
    }
}