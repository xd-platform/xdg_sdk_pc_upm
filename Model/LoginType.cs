namespace com.xd.intl.pc{
    public enum LoginType : int{ 
        Default = -1,
        Guest  = 0,
        TapTap = 5,  //值要与后台一致, 看安卓的就知道了
    }
}
// ios的，要与后台一致！
// XDGLoginInfoTypeGuest = 0,
// XDGLoginInfoTypeWeChat = 1,
// XDGLoginInfoTypeApple = 2,
// XDGLoginInfoTypeGoogle = 3,
// XDGLoginInfoTypeFacebook = 4,
// XDGLoginInfoTypeTapTap = 5,
// XDGLoginInfoTypeLine = 6,
// XDGLoginInfoTypeTwitter = 7