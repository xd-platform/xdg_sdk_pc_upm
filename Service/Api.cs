using System;
using System.Collections.Generic;
using TapTap.Bootstrap;
using TapTap.Common;
using TapTap.Login;
using UnityEditor;
using UnityEngine;

namespace SDK.PC{
    public class Api{
        private readonly static string BASE_URL = "http://test-xdsdk-intnl-6.xd.com"; //测试
        // private readonly static string BASE_URL = " https://xdsdk-intnl-6.xd.com"; //正式

        //获取配置
        private readonly static string INIT_SDK = BASE_URL + "/api/init/v1/config";

        //IP信息
        private readonly static string IP_INFO = "https://ip.xindong.com/myloc2";

        // login
        private readonly static string XDG_USER_PROFILE = BASE_URL + "/api/account/v1/info";

        //游客
        private readonly static string XDG_COMMON_LOGIN = BASE_URL + "/api/login/v1/union";

        // 与leanClound同步
        private readonly static string XDG_LOGIN_SYN = BASE_URL + "/api/login/v1/syn";

        // 获取用户绑定信息
        private readonly static string XDG_BIND_LIST = BASE_URL + "/api/account/v1/bind/list";

        // 绑定接口
        private readonly static string XDG_BIND_INTERFACE = BASE_URL + "/api/account/v1/bind";

        // 解绑接口
        private readonly static string XDG_UNBIND_INTERFACE = BASE_URL + "/api/account/v1/unbind";
        
        // 查询补款订单信息
        private readonly static string XDG_PAYBACK_LIST   = BASE_URL + "/order/v1/user/repayOrders";  

        private readonly static string TDSG_GLOBAL_SDK_DOMAIN = "https://xdg-1c20f-intl.xd.com";

        private readonly static int SUCCESS = 200; 
        
        public static void InitSDK(string sdkClientId,
            Action<bool, string> callback){
            DataStorage.SaveString(DataStorage.ClientId, sdkClientId);
            Net.GetRequest(INIT_SDK, null, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                if (model.code == SUCCESS){
                    InitConfigModel.SaveToLocal(model);
                    var tapCfg = model.data.configs.tapSdkConfig;
                    TapLogin.Init(tapCfg.clientId, false, false);
                    var config = new TapConfig.Builder()
                        .ClientID(tapCfg.clientId) // 必须，开发者中心对应 Client ID
                        .ClientToken(tapCfg.clientToken) // 必须，开发者中心对应 Client Token
                        .ServerURL(tapCfg.serverUrl) // 开发者中心 > 你的游戏 > 游戏服务 > 云服务 > 数据存储 > 服务设置 > 自定义域名 绑定域名
                        .RegionType(RegionType.IO) // 非必须，默认 CN 表示国内
                        .TapDBConfig(tapCfg.enableTapDB, tapCfg.tapDBChannel, Application.version)
                        .ConfigBuilder();
                    TapBootstrap.Init(config);

                    callback(true,"");
                    XDGSDK.Tmp_IsInited = true;
                    XDGSDK.Tmp_IsInitSDK_ing = false;
                } else{
                    callback(false,model.msg);
                    XDGSDK.Tmp_IsInitSDK_ing = false;
                }
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false, msg);
                XDGSDK.Tmp_IsInitSDK_ing = false;
            });
        }

        public static void LoginTyType(LoginType loginType, Action<bool, XDGUserModel, string> callback){
            GetLoginParam(loginType, (pSuccess,param) => {
                if (param != null){
                    Net.PostRequest(XDG_COMMON_LOGIN, param, (data) => {
                        var model = XDGSDK.GetModel<TokenModel>(data);
                        if (model.code == SUCCESS){
                            TokenModel.SaveToLocal(model);
                            GetUserInfo((userSuccess, userMd, uMsg) => {
                                if (userSuccess){
                                    SyncTdsUser((tdsSuccess, tdsMsg) => {
                                        if (tdsSuccess){
                                            CheckPrivacyAlert(isPass => {
                                                if (isPass){
                                                    callback(true, userMd, "");
                                                }
                                            });
                                        } else{
                                            callback(false, null, tdsMsg);
                                        }
                                    });
                                } else{
                                    callback(false, null, uMsg);
                                }
                            });
                        } else{
                            callback(false, null, model.msg);
                        }
                    }, (code, msg) => {
                        XDGSDK.Log("登录失败 code: " + code + " msg: " + msg);
                        callback(false, null, msg);
                    });
                } else{
                    callback(false, null, "Login param is null");
                }
            });
        }

        public static void GetLoginParam(LoginType loginType, Action<bool,Dictionary<string, object>> callback){
            if (loginType == LoginType.Guest){
                Dictionary<string, object> param = new Dictionary<string, object>{
                    {"type", (int) loginType},
                    {"token", SystemInfo.deviceUniqueIdentifier}
                };
                callback(true, param);
            } else if (loginType == LoginType.TapTap){
                GetTapToken((success, md) => {
                    if (success){
                        Dictionary<string, object> param = new Dictionary<string, object>{
                            {"type", (int) loginType},
                            {"token", md.kid},
                            {"secret", md.macKey},
                        };
                        callback(true, param);
                    } else{
                        callback(false, null);
                    }
                });
            }
        }

        private async static void GetTapToken(Action<bool, AccessToken> callback){
            try{
                var accessToken = await TapLogin.Login();
                callback(true, accessToken);
            } catch (Exception e){
                callback(false, null);
                if (e is TapException tapError){
                    if (tapError.code == (int) TapErrorCode.ERROR_CODE_BIND_CANCEL){
                        XDGSDK.Log("Tap 登录取消");
                    } else{
                        XDGSDK.Log($"Tap 登录失败: code: {tapError.code},  message: {tapError.message}");
                    }
                }
            }
        }

        private static void SyncTdsUser(Action<bool, string> callback){
            Net.PostRequest(XDG_LOGIN_SYN, null, (data) => {
                var md = XDGSDK.GetModel<SyncTokenModel>(data);
                if (md.code == SUCCESS){
                    XDGSDK.Log("sync token: " + md.data.sessionToken);
                    TDSUser.BecomeWithSessionToken(md.data.sessionToken);
                    callback(true, "");   
                } else{
                    callback(false, md.msg);   
                }
            }, (code, msg) => {
                XDGUserModel.ClearUserData();
                callback(false, msg);
                XDGSDK.Log("SyncTdsUser 失败 code: " + code + " msg: " + msg);
            });
        }

        public static void GetUserInfo(Action<bool, XDGUserModel, string> callback){
            Net.GetRequest(XDG_USER_PROFILE, null, (data) => {
                var model = XDGSDK.GetModel<XDGUserModel>(data);
                if (model.code == SUCCESS){
                    XDGUserModel.SaveToLocal(model);
                    callback(true, model, "");
                } else{
                    callback(false, null, model.msg);
                }
            }, (code, msg) => {
                XDGSDK.Log("获取用户信息失败 code: " + code + " msg: " + msg);
                callback(false, null, msg);
            });
        }

        public static void GetIpInfo(Action<bool, IpInfoModel> callback){
            RequestIpInfo(true, callback);
        }

        private static void RequestIpInfo(bool repeat, Action<bool, IpInfoModel> callback){
            Net.GetRequest(IP_INFO, null, (data) => {
                var model = XDGSDK.GetModel<IpInfoModel>(data);
                    IpInfoModel.SaveToLocal(model);
                    callback(true, model);
            }, (code, msg) => {
                if (repeat){
                    RequestIpInfo(false, callback);
                } else{
                    var oldMd = IpInfoModel.GetLocalModel();
                    if (oldMd != null){
                        callback(true, oldMd);
                    } else{
                        XDGSDK.Log("获取 ip info 失败 code: " + code + " msg: " + msg);
                        callback(false, null);
                    }
                }
            });
        }

        private static void CheckPrivacyAlert(Action<bool> callback){
            if (InitConfigModel.CanShowPrivacyAlert()){
                UIManager.ShowUI<PrivacyAlert>(null, (code, objc) => {
                    if (code == UIManager.RESULT_SUCCESS){
                        callback(true);
                    }
                });
            } else{
                callback(true);
            }
        }

        public static void GetBindList(Action<bool, BindModel, string> callback){
            Net.GetRequest(XDG_BIND_LIST, null, (data) => {
                var md = XDGSDK.GetModel<BindModel>(data);
                if (md.code == SUCCESS){
                    callback(true, md, "");   
                } else{
                    callback(false, null, md.msg);
                }
            }, (code, msg) => { callback(false, null, msg); });
        }

        public static void bind(Dictionary<string, object>param, Action<bool, string> callback){
            Net.PostRequest(XDG_BIND_INTERFACE, param, (data) => {
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    if (md.code == SUCCESS){
                        callback(true, "");   
                    } else{
                        callback(false, md.msg);
                    }
                },
                (code, msg) => {
                    callback(false,msg);
                });
        }

        public static void unbind(LoginType loginType, Action<bool, string> callback){
            var param = new Dictionary<string, object>(){
                {"type", (int) loginType}
            };
            Net.PostRequest(XDG_UNBIND_INTERFACE, param, (data) => {
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    if (md.code == SUCCESS){
                        callback(true, "");   
                    } else{
                        callback(false, md.msg);
                    }
                },
                (code, msg) => {
                    callback(false,msg);
                });
        }

        public static void checkPay(Action<bool, PayCheckModel, string> callback){
            var umd = XDGUserModel.GetLocalModel();
            if (umd == null){
                XDGSDK.Log("checkPay前请先登录！");
                return;;
            }

            var param = new Dictionary<string, object>(){
                {"userId",umd.data.userId },
            };
            Net.GetRequest(XDG_PAYBACK_LIST, param, data => {
                var pmd = XDGSDK.GetModel<PayCheckModel>(data);
                if (pmd.code == SUCCESS){
                    callback(true, pmd, "");
                } else{
                    callback(false, null, pmd.msg);
                }
            }, (code, msg) => {
                callback(false, null, msg);
            });
        }

    }
}