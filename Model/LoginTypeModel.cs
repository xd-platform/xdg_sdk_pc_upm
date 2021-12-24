namespace SDK.PC{
    public class LoginTypeModel{
        public string typeName{ get; set; }
        public int typeValue{ get; set; }
        public LoginType type { get; set; }
        
       public LoginTypeModel(LoginType type){ 
            this.type = type;
            this.typeValue = (int)type;

            if (type == LoginType.TapTap){
                this.typeName = "TapTap";
            }else if (type == LoginType.Guest){
                this.typeName = "Guest";
            }
        }
    }
}