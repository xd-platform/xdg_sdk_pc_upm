# XDGSDK-PC-6.0

## 1.添加引用
```
   "com.leancloud.realtime": "https://github.com/leancloud/csharp-sdk-upm.git#realtime-0.10.5",
    "com.leancloud.storage": "https://github.com/leancloud/csharp-sdk-upm.git#storage-0.10.5",
    "com.taptap.tds.bootstrap": "https://github.com/TapTap/TapBootstrap-Unity.git#3.6.3",
    "com.taptap.tds.common": "https://github.com/TapTap/TapCommon-Unity.git#3.6.3",
    "com.taptap.tds.login": "https://github.com/taptap/TapLogin-Unity.git#3.6.3",
    "com.taptap.tds.tapdb": "https://github.com/TapTap/TapDB-Unity.git#3.6.3",
    "com.xd.intl.pc": "https://github.com/xd-platform/xdg_sdk_pc_upm.git#6.1.1",
    "com.unity.nuget.newtonsoft-json": "2.0.2",
    "com.unity.textmeshpro": "3.0.6",
```
## 1.1 Tap信息配置
  1.申请xd client id 用于初始化sdk。2.在Tap开发者后台创建应用，然后把 tapSdkConfig 信息给xd 后端配置，Tap信息从接口下发。
```
  //Tap配置信息
  "clientId": "jfqhF3x9mxxxxx", 
  "clientToken": "C91pZh9OJNt7oDPx3lku4H01Helxxxx",
  "enableTapDB": true,
  "tapDBChannel": "tapdb_xxxx",
  "serverUrl": "https://xxxx.tapapis.com"
```

## 2.接口使用

#### 设置语言
```
 XDGSDK.SetLanguage(LangType type);
 支持如下语言
 public enum LangType{
        ZH_CN = 0, 中文
        ZH_TW = 1, 繁体中文 
        EN = 2, 英文
        TH = 3, 泰文
        ID = 4, 印尼文
        KR = 5, 韩语
        JP = 6, 日语
        DE = 7, 德语
        FR = 8, 法语
        PT = 9, 葡萄牙语
        ES = 10, 西班牙语
        TR = 11, 土耳其语
        RU = 12, 俄罗斯语
    }
```

#### 初始化
```
 XDGSDK.InitSDK("xd client id",
            (isSuccess, msg) => {
                if (isSuccess){
                    ResultText.text = "初始化成功";
                    //开始调用登录
                } else{
                    ResultText.text = "初始化失败：" + msg;
                }
            });
```

#### 登录
```
1.初始化成功后才可以调用登录
XDGSDK.LoginByType(LoginType.TapTap, (user)=>{
            ResultText.text = $"Tap登录成功：{user.nickName} userId: {user.userId} kid: {user.token.kid}";
        },(error) => {
            ResultText.text = $"登录失败：code: {error.code}  msg: {error.error_msg}";
        });
2.支持登录类型        
   LoginType  { 
        Default, //自动登录，以上次登录成功的信息登录，第一次自动登录是失败的。
        Guest,  //游客登录
        TapTap, //Tap 登录  
    }       
3.先自动登录，如果自动登录失败了，再调用Tap或游客登录

4. 注意 在 【6.1.1】 版本开始，内建账号采用本地创建，如果登录成功后要获取TdsUser信息的话，需要先调用  【await TDSUser.GetCurrent().Result.Fetch();】拉取TdsUser信息.

```

#### 获取用户信息
```
  XDGSDK.GetUserInfo((user) => {
            ResultText.text = $"获取成功，用户昵称： {user.nickName} userId: {user.userId} kid: {user.token.kid}";
        }, (error) => {
            ResultText.text = "获取失败" + error.error_msg;
        });
```

#### 检查是否有补单的
```
 XDGSDK.CheckPay((type) => {
            ResultText.text = $"补款提示类型：{type}";
        }, (error) => {
            ResultText.text = $"补款查询失败：code:{error.code}, msg: {error.error_msg}";
        });
 有对应的弹框提示，去APP端补款       
 CheckPayType{
        iOS,      //只有iOS需要补款
        Android,  //只有Android需要补款
        iOSAndAndroid,      //iOS Android 都需要补款
        None,     //没有要补款
    }       
```

#### 打开账号中心
```
XDGSDK.OpenUserCenter();
```

#### 打开网页支付
```
XDGSDK.OpenWebPay(serverId, roleId);
```

#### 打开客服中心
```
XDGSDK.OpenCustomerCenter("serverId", "roleId", "roleName");
```

#### 是否初始化
```
XDGSDK.IsInitialized();
```

#### 获取版本号
```
XDGSDK.GetSdkVersion();
```

#### 获取位置信息
```
  XDGSDK.GetLocationInfo((md) => {
            ResultText.text = "地区信息成功：\ncountry: " + md.country + " \ncity: " + md.city + "\nregion:  " + md.country_code;
        }, (error) => {
            ResultText.text = $"地区信息失败: {error.error_msg}";
        });
```

#### 退出登录
```
 XDGSDK.Logout();
```

#### Windows平台网页登录需要在本地添加如下注册表 （下面是Demo的注册表，仅供参考，修改对应信息就可以了） 
```
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\open-taptap-{Tap的ClientId}]
@="应用名称"
"URL Protocol"="C:\\Users\\XD\\Desktop\\APP\\win_app\\app\\XDGSDK-PC-6.0.exe"  //改成自己的exe安装路径

[HKEY_CLASSES_ROOT\open-taptap-{Tap的ClientId}]
@="应用名称"

[HKEY_CLASSES_ROOT\open-taptap-{Tap的ClientId}]

[HKEY_CLASSES_ROOT\open-taptap-{Tap的ClientId}\Shell\Open]

[HKEY_CLASSES_ROOT\open-taptap-{Tap的ClientId}\Shell\Open\Command]
@="\"C:\\Users\\XD\\Desktop\\APP\\win_app\\app\\XDGSDK-PC-6.0.exe\" \"%1\""  //改成自己的exe安装路径
```

[CHANGE LOG](https://github.com/xd-platform/xdg_sdk_pc_upm/blob/pc_upm/CHANGELOG.md)