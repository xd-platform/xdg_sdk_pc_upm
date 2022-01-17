# XDGSDK-PC-6.0

## 1.在Packages/manifest.json中加入如下引用
```
"com.leancloud.realtime": "https://github.com/leancloud/csharp-sdk-upm.git#realtime-0.10.0",
"com.leancloud.storage": "https://github.com/leancloud/csharp-sdk-upm.git#storage-0.10.0",
"com.taptap.tds.bootstrap": "https://github.com/TapTap/TapBootstrap-Unity.git#3.5.0",
"com.taptap.tds.tapdb": "https://github.com/TapTap/TapDB-Unity.git#3.5.0",
"com.taptap.tds.common": "https://github.com/TapTap/TapCommon-Unity.git#3.5.2",
"com.taptap.tds.login": "https://github.com/taptap/TapLogin-Unity.git#3.5.2",
"com.unity.nuget.newtonsoft-json": "2.0.2",
"com.unity.textmeshpro": "3.0.6",
"com.xd.intl.pc": "https://github.com/xd-platform/xdg_sdk_pc_upm.git#{version}",
```

## 1.1 Tap信息配置
 可以通过xd后台通过接口配置 `tapSdkConfig` 信息，如果后台接口没配置，则会读取本地 `/Assets/Resources/XD-Info.json` 里的配置，如果接口和本地都没配置则报错。

## 2.接口使用

#### 设置语言
```
XDGSDK.SetLanguage(LanguageType);
```

#### 初始化
```
UIManager.ShowLoading();
XDGSDK.InitSDK("hn5RcJei2JxCYlS0",
     (success,msg) => {
      UIManager.DismissLoading();
      ResultText.text = "初始化结果：" + success;
});
```

#### 是否初始化
```
XDGSDK.IsInited();
```

#### 获取版本号
```
XDGSDK.GetSdkVersion();
```

#### 登录
```
  XDGSDK.LoginTyType(LoginType.TapTap, (success, userMd, msg) => {
            if (success){
                ResultText.text = "登录结果：" + success + "\n用户名：" + userMd.data.nickName;   
            } else{
                ResultText.text = "登录失败:" + msg;
            }
        });
```

#### 获取用户信息
```
  XDGSDK.GetUserInfo((success, md,msg) => {
            if (success){
                ResultText.text = "获取成功，用户昵称：" + md.data.nickName;     
            } else{
                ResultText.text = "获取失败" + msg;     
            }
        });
```

#### 检查是否有补单的
```
 XDGSDK.CheckPay((type, msg) => {
            ResultText.text = msg;
        });
```

#### 打开网页支付
```
XDGSDK.OpenWebPay(serverId, roleId);
```

#### 打开账号中心
```
XDGSDK.OpenUserCenter();
```

#### 打开客服中心
```
XDGSDK.OpenCustomerCenter("serverId", "roleId", "roleName");
```

#### 获取位置信息
```
XDGSDK.GetLocationInfo((success, md) => {
if (success){
ResultText.text = "地区信息：\ncountry: " + md.country + " \ncity: " + md.city + "\nregion:  " + md.country_code;
} else{
ResultText.text = "地区信息失败";
}
});
```

#### 退出登录
```
 XDGSDK.Logout();
```

#### Windows平台网页登录需要在本地添加如下注册表
```
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\open-taptap-Tap的ClientId]
@="应用名称"
"URL Protocol"="C:\\Users\\XD\\Desktop\\APP\\win_app\\app\\XDGSDK-PC-6.0.exe"  //改成自己的exe安装路径

[HKEY_CLASSES_ROOT\open-taptap-Tap的ClientId]
@="应用名称"

[HKEY_CLASSES_ROOT\open-taptap-Tap的ClientId]

[HKEY_CLASSES_ROOT\open-taptap-Tap的ClientId\Shell\Open]

[HKEY_CLASSES_ROOT\open-taptap-Tap的ClientId\Shell\Open\Command]
@="\"C:\\Users\\XD\\Desktop\\APP\\win_app\\app\\XDGSDK-PC-6.0.exe\" \"%1\""  //改成自己的exe安装路径
```
