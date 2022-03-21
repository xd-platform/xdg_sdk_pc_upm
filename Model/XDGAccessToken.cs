namespace com.xd.intl.pc{
    public class XDGAccessToken{
        public string kid{ get; set; }
        public string macKey{ get; set; }
        public string tokenType{ get; set; }
        public long expireIn{ get; set; }  //游客才有
            
        public string macAlgorithm{ get; set; } //tapTap才有
    }
}