using System;
using System.Collections.Generic;
using System.Threading;
using LeanCloud.Storage;
using TapTap.Bootstrap;
using TapTap.Common;
using TapTap.Login;
using UnityEngine;

namespace com.xd.intl.pc{
    public class Api{
        private readonly static string BASE_URL = "https://test-xdsdk-intnl-6.xd.com"; //测试
        // private readonly static string BASE_URL = "https://xdsdk-intnl-6.xd.com"; //正式

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
        private readonly static string XDG_PAYBACK_LIST = BASE_URL + "/order/v1/user/repayOrders";

        private readonly static string TDSG_GLOBAL_SDK_DOMAIN = "https://xdg-1c20f-intl.xd.com";

        private readonly static int SUCCESS = 200;

        public static void InitSDK(string sdkClientId, Action<bool, string> callback){
            DataStorage.SaveString(DataStorage.ClientId, sdkClientId);
            Net.GetRequest(INIT_SDK, null, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                if (model.code == SUCCESS){
                    InitConfigModel.SaveToLocal(model);
                    InitBootstrap(model, callback, "");
                } else{
                    InitBootstrap(InitConfigModel.GetLocalModel(), callback, model.msg);
                }
            }, (code, msg) => {
                InitBootstrap(InitConfigModel.GetLocalModel(), callback, msg);
                XDGSDK.LogError($"初始化失败：code:{code} msg:{msg}");
            });
        }

        private static void InitBootstrap(InitConfigModel infoMd, Action<bool, string> callback, string msg){
            if (infoMd != null){
                var tapCfg = infoMd.data.configs.tapSdkConfig;
                TapLogin.Init(tapCfg.clientId, false, false);
                var config = new TapConfig.Builder()
                    .ClientID(tapCfg.clientId)
                    .ClientToken(tapCfg.clientToken)
                    .ServerURL(tapCfg.serverUrl)
                    .RegionType(RegionType.IO) //IO：海外 
                    .TapDBConfig(tapCfg.enableTapDB, tapCfg.tapDBChannel, Application.version)
                    .ConfigBuilder();
                TapBootstrap.Init(config);

                callback(true, msg);
                XDGSDK.Tmp_IsInited = true;
            } else{
                callback(false, msg);
            }

            XDGSDK.Tmp_IsInitSDK_ing = false;
        }

        public static void LoginTyType(LoginType loginType, Action<XDGUser> callback, Action<XDGError> errorCallback){
            var lmd = LanguageMg.GetCurrentModel();
            if (loginType == LoginType.Default){
                var localUser = XDGUserModel.GetLocalModel();
                if (localUser != null){
                    AsyncLocalTdsUser(localUser.data, SyncTokenModel.GetLocalToken(), callback, errorCallback);
                } else{
                    errorCallback(XDGError.msg(lmd.tds_login_failed));
                }
            } else{
                GetLoginParam(loginType, (param) => {
                    UIManager.ShowLoading();
                    Net.PostRequest(XDG_COMMON_LOGIN, param, (data) => {
                        UIManager.DismissLoading();
                        Thread.Sleep(TimeSpan.FromSeconds(0.2f));
                        var model = XDGSDK.GetModel<TokenModel>(data);
                        if (model.code == SUCCESS){
                            TokenModel.SaveToLocal(model);
                            RequestUserInfo((userMd) => {
                                AsyncNetworkTdsUser(userMd.userId, (sessionToken) => {
                                    CheckPrivacyAlert(isPass => {
                                        if (isPass){
                                            callback(userMd);
                                        }
                                    });
                                }, errorCallback);
                            }, (error) => {
                                errorCallback(XDGError.msg(error.error_msg));
                            });
                        } else{
                            errorCallback(XDGError.msg(model.msg));
                        }
                    }, (code, msg) => {
                        XDGSDK.LogError("登录失败 code: " + code + " msg: " + msg);
                        UIManager.DismissLoading();
                        errorCallback(XDGError.msg(msg));
                    });
                }, errorCallback);
            }
        }

        public static void GetLoginParam(LoginType loginType, Action<Dictionary<string, object>> callback, Action<XDGError> errorCallback){
            if (loginType == LoginType.Guest){
                Dictionary<string, object> param = new Dictionary<string, object>{
                    {"type", (int) loginType},
                    {"token", SystemInfo.deviceUniqueIdentifier}
                };
                callback(param);
            } else if (loginType == LoginType.TapTap){
                RequestTapToken((md) => {
                    Dictionary<string, object> param = new Dictionary<string, object>{
                        {"type", (int) loginType},
                        {"token", md.kid},
                        {"secret", md.macKey},
                    };
                    callback(param);
                }, errorCallback);
            } else{
                errorCallback(XDGError.msg("No Login Param"));
            }
        }

        private static async void RequestTapToken(Action<AccessToken> callback, Action<XDGError> errorCallback){
            try{
                var accessToken = await TapLogin.Login();
                callback(accessToken);
            } catch (Exception e){
                var msg = "登录失败";
                if (e is TapException tapError){
                    msg = tapError.message;
                    if (tapError.code == (int) TapErrorCode.ERROR_CODE_BIND_CANCEL){ //取消登录
                        XDGSDK.Log($"取消登录：{tapError.code}  {msg}");
                        errorCallback(new XDGError(tapError.code, msg));
                        return;
                    } else{
                        XDGSDK.LogError($"Tap 登录失败: code: {tapError.code},  message: {msg}");
                    }
                }
                errorCallback(XDGError.msg(msg));
            }
        }

        private static async void AsyncLocalTdsUser(XDGUser xdgUser, string sessionToken, Action<XDGUser> callback, Action<XDGError> errorCallback){
            if (!string.IsNullOrEmpty(sessionToken)){
                LCUser user = LCObject.CreateWithoutData(LCUser.CLASS_NAME, xdgUser.userId) as LCUser;
                user.SessionToken = sessionToken;
                await user.SaveToLocal();
                callback(xdgUser);
            } else{ //不可能进这里，有user肯定有token
                XDGSDK.LogError("内建账号失败，缓存sessionToken是空");
                errorCallback(XDGError.msg("内建账号失败"));
            }
        }

        private static void AsyncNetworkTdsUser(string userId, Action<string> callback, Action<XDGError> errorCallback){
            Net.PostRequest(XDG_LOGIN_SYN, null, async (data) => {
                var md = XDGSDK.GetModel<SyncTokenModel>(data);
                if (md.code == SUCCESS){
                    XDGSDK.Log("sync token: " + md.data.sessionToken);
                    SyncTokenModel.SaveToLocal(md.data.sessionToken);

                    LCUser lcUser = LCObject.CreateWithoutData(LCUser.CLASS_NAME, userId) as LCUser;
                    lcUser.SessionToken = md.data.sessionToken;
                    await lcUser.SaveToLocal();

                    callback(md.data.sessionToken);
                } else{
                    errorCallback(XDGError.msg(md.msg));
                }
            }, async (code, msg) => {
                var token = SyncTokenModel.GetLocalToken();
                if (!string.IsNullOrEmpty(token)){
                    LCUser lcUser = LCObject.CreateWithoutData(LCUser.CLASS_NAME, userId) as LCUser;
                    lcUser.SessionToken = token;
                    await lcUser.SaveToLocal();
                    callback(token);
                } else{
                    errorCallback(new XDGError(code, msg));
                    XDGSDK.LogError("内建账号失败 code: " + code + " msg: " + msg);
                }
            });
        }

        public static void RequestUserInfo(Action<XDGUser> callback, Action<XDGError> errorCallback){
            Net.GetRequest(XDG_USER_PROFILE, null, (data) => {
                var model = XDGSDK.GetModel<XDGUserModel>(data);
                if (model.code == SUCCESS){
                    XDGUserModel.SaveToLocal(model);
                    callback(model.data);
                } else{
                    errorCallback(XDGError.msg(model.msg));
                }
            }, (code, msg) => {
                XDGSDK.LogError("获取用户信息失败 code: " + code + " msg: " + msg);
                errorCallback(new XDGError(code, msg));
            });
        }

        public static void GetIpInfo(Action<IpInfoModel> callback, Action<XDGError> errorCallback){
            RequestIpInfo(true, callback, errorCallback);
        }

        public static void RequestIpInfo(bool repeat, Action<IpInfoModel> callback, Action<XDGError> errorCallback){
            Net.GetRequest(IP_INFO, null, (data) => {
                var model = XDGSDK.GetModel<IpInfoModel>(data);
                IpInfoModel.SaveToLocal(model);
                callback(model);
            }, (code, msg) => {
                if (repeat){
                    RequestIpInfo(false, callback, errorCallback);
                } else{
                    var oldMd = IpInfoModel.GetLocalModel();
                    if (oldMd != null){
                        callback(oldMd);
                    } else{
                        XDGSDK.LogError("获取 ip info 失败 code: " + code + " msg: " + msg);
                        errorCallback(new XDGError(code, msg));
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

        public static void GetBindList(Action<BindModel> callback, Action<XDGError> errorCallback){
            Net.GetRequest(XDG_BIND_LIST, null, (data) => {
                var md = XDGSDK.GetModel<BindModel>(data);
                if (md.code == SUCCESS){
                    callback(md);
                } else{
                    errorCallback(new XDGError(md.code, md.msg));
                }
            }, (code, msg) => { errorCallback(new XDGError(code, msg)); });
        }

        public static void bind(Dictionary<string, object> param, Action callback, Action<XDGError> errorCallback){
            Net.PostRequest(XDG_BIND_INTERFACE, param, (data) => {
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    if (md.code == SUCCESS){
                        callback();
                    } else{
                        errorCallback(new XDGError(md.code, md.msg));
                    }
                },
                (code, msg) => {
                    errorCallback(new XDGError(code, msg));
                    XDGSDK.LogError($"绑定失败 param:{param} code:{code} msg:{msg}");
                });
        }

        public static void unbind(LoginType loginType, Action callback, Action<XDGError> errorCallback){
            var param = new Dictionary<string, object>(){
                {"type", (int) loginType}
            };
            Net.PostRequest(XDG_UNBIND_INTERFACE, param, (data) => {
                    var md = XDGSDK.GetModel<BaseModel>(data);
                    if (md.code == SUCCESS){
                        callback();
                    } else{
                        errorCallback(new XDGError(md.code, md.msg));
                    }
                },
                (code, msg) => {
                    errorCallback(new XDGError(code, msg));
                    XDGSDK.LogError($"解绑失败 param:{param} code:{code} msg:{msg}");
                });
        }

        public static void checkPay(Action<PayCheckModel> callback, Action<XDGError> errorCallback){
            var umd = XDGUserModel.GetLocalModel();
            if (umd == null){
                errorCallback(XDGError.msg("Please Login First"));
                return;
            }

            var param = new Dictionary<string, object>(){
                {"userId", umd.data.userId},
            };
            Net.GetRequest(XDG_PAYBACK_LIST, param, data => {
                var pmd = XDGSDK.GetModel<PayCheckModel>(data);
                if (pmd.code == SUCCESS){
                    callback(pmd);
                } else{
                    errorCallback(new XDGError(pmd.code, pmd.msg));
                }
            }, (code, msg) => { errorCallback(new XDGError(code, msg)); });
        }
    }
}