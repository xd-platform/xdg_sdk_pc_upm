namespace com.xd.intl.pc{
    public class LoginTypeModel{
        public string typeName{ get; set; }
        public int typeValue{ get; set; }
        public LoginType type { get; set; }
        
       public LoginTypeModel(LoginType type){ 
            this.type = type;
            this.typeValue = (int)type;
            this.typeName = GetName((int)type);
       }

       public static string GetName(int type){
           if (type == (int)LoginType.TapTap){
              return "TapTap";
           }else if (type == (int)LoginType.Guest){
              return "Guest";
           }
           return "";
       }

    }
}