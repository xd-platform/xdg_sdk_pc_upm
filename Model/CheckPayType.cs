namespace com.xd.intl.pc{
    public enum CheckPayType{
        iOS,      //只有iOS需要补款
        Android,  //只有Android需要补款
        All,      //iOS Android 都需要补款
        None,     //没有要补款
        Error     //检查出错了
    }
}