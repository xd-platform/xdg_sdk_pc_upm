﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace com.xd.intl.pc{
    public class UserCenterAlert : UIElement{
        public Text titleTxt;
        public Text infoTitleTxt;
        public Text typeTxt;
        public Text idTxt;
        public GameObject errorView;
        public GameObject loadingView;
        public GameObject scrollView;

        private LanguageModel langModel;
        private XDGUserModel userMd;

        public GameObject Content;
        private float cellHeight = 36.0f;

        private List<BindModel.Data> dataList;
        private List<AccountCell> cellList;

        void Start(){
            userMd = XDGUserModel.GetLocalModel();
            langModel = LanguageMg.GetCurrentModel();
            titleTxt.text = langModel.tds_account_safe_info;

            infoTitleTxt.text = langModel.tds_account_info;
            typeTxt.text = $"{langModel.tds_current_account_prefix} ({GetLoginTypeName()})";
            idTxt.text = $"ID: {userMd.data.userId}";
            errorView.GetComponentInChildren<Text>().text = langModel.tds_network_error_retry;
            requestList();
        }

        private string GetLoginTypeName(){
            var result = langModel.tds_guest;
            LoginType type = userMd.data.GetLoginType();
            if (type == LoginType.TapTap){
                result = "TapTap";
            }

            var unitStr = langModel.tds_account_format;
            return unitStr.Replace("%s", result);
        }

        public void CloseTaped(){
            UIManager.DismissAll();
        }

        public void CopyTaped(){
            GUIUtility.systemCopyBuffer = userMd.data.userId;
            UIManager.ShowToast(langModel.tds_copy_success);
        }

        private void requestList(){
            if (dataList == null){
                loadingView.SetActive(true);
                Api.GetBindList((bindModel) => {
                    loadingView.SetActive(false);
                    scrollView.GetComponentInChildren<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);

                    dataList = new List<BindModel.Data>();
                    var supportList = GetSupportTypes();
                    foreach (var st in supportList){ //本地支持的都要显示
                        var md = new BindModel.Data();
                        md.loginType = st.typeValue;
                        md.loginName = st.typeName;
                        md.status = (int) BindType.UnBind;

                        foreach (var netMd in bindModel.data){ //插入对应的状态
                            if (st.typeValue == netMd.loginType && netMd.status == (int) BindType.Bind){
                                md.status = netMd.status; //1未绑定
                                md.bindDate = netMd.bindDate;
                                break;
                            }
                        }

                        dataList.Add(md);
                    }

                    addCellView();
                    errorView.SetActive(false);
                }, (error) => {
                    Invoke("delayDismissLoading", 0.5f);
                    errorView.SetActive(true);
                    XDGSDK.LogError("列表请求失败" + error.error_msg);
                });
            }
        }

        private void delayDismissLoading(){
            loadingView.SetActive(false);
        }

        private void addCellView(){
            cellList = new List<AccountCell>();
            for (int i = 0; i < dataList.Count; i++){
                GameObject gameObj = Instantiate(Resources.Load("Prefabs/AccountCell")) as GameObject;
                gameObj.name = "AccountCell" + i;
                gameObj.transform.SetParent(Content.transform);
                gameObj.transform.localPosition = new Vector3(350, -cellHeight / 2 - (cellHeight * i), 0);
                gameObj.transform.localScale = Vector3.one;

                AccountCell cell = gameObj.GetComponent<AccountCell>();
                cell.cellIndex = i;
                cell.setModel(dataList[i], langModel);
                cell.Callback += (cellIndex, msg) => { //cell 事件回调
                    var md = dataList[cellIndex];
                    if (md.status == (int) BindType.Bind){ //开始解绑
                        var dic = new Dictionary<string, object>(){
                            {"loginType", md.loginType},
                            {"alertType", (int) getAlertType()},
                        };
                        UIManager.ShowUI<DeleteAccountAlert>(dic,
                            (code, data) => { unbind((LoginType) md.loginType, cellIndex); });
                    } else{ //开始绑定
                        bind((LoginType) md.loginType, cellIndex);
                    }
                };
                cellList.Add(cell);
            }

            if (userMd.data.loginType == (int) LoginType.Guest){ //游客添加删除
                GameObject gameObj = Instantiate(Resources.Load("Prefabs/DeleteCell")) as GameObject;
                gameObj.name = "DeleteCell";
                gameObj.transform.SetParent(Content.transform);
                gameObj.transform.localPosition =
                    new Vector3(350, -cellHeight / 2 - (cellHeight * cellList.Count) - 10, 0);
                gameObj.transform.localScale = Vector3.one;

                DeleteCell cell = gameObj.GetComponent<DeleteCell>();
                cell.setDeleteText(langModel.tds_delete_account);
                cell.Callback += (code, msg) => { //delete 事件回调
                    unbind(LoginType.Guest, -1);
                };
            }
        }


        private void bind(LoginType loginType, int cellIndex){
            XDGSDK.Log("绑定： " + loginType);
            if (loginType == LoginType.TapTap){
                Api.GetLoginParam(loginType, (param) => {
                    Api.bind(param, () => {
                        var cellMd = dataList[cellIndex];
                        var cellView = cellList[cellIndex];
                        cellMd.status = (int) BindType.Bind;
                        cellView.refreshState(cellMd);
                        UIManager.ShowToast(langModel.tds_bind_success);
                    }, (error) => {
                        if (string.IsNullOrEmpty(error.error_msg)){
                            UIManager.ShowToast(langModel.tds_bind_error);
                        } else{
                            UIManager.ShowToast(error.error_msg);
                        }
                    });
                }, (error) => {
                    if (string.IsNullOrEmpty(error.error_msg)){
                        UIManager.ShowToast(langModel.tds_bind_error);
                    } else{
                        UIManager.ShowToast(error.error_msg);
                    }
                });
            }
        }

        private void unbind(LoginType loginType, int cellIndex){ 
            XDGSDK.Log("解绑： " + loginType);
            Api.unbind(loginType, () => {
                if (loginType == LoginType.Guest){
                    UIManager.ShowToast(langModel.tds_unbind_guest_return);
                    XDGSDK.Logout();
                    UIManager.DismissAll();
                } else{
                    if (dataList.Count == 1){ //解绑最后一个绑定的第三方，和删除账号一个逻辑
                        var cellMd = dataList[cellIndex];
                        var cellView = cellList[cellIndex];
                        cellMd.status = (int) BindType.UnBind;
                        cellView.refreshState(cellMd);
                        UIManager.ShowToast(langModel.tds_unbind_delete_success_return_sign);
                        XDGSDK.Logout();
                        UIManager.DismissAll();
                    } else{ //不是唯一账号
                        var cellMd = dataList[cellIndex];
                        var cellView = cellList[cellIndex];
                        cellMd.status = (int) BindType.UnBind;
                        cellView.refreshState(cellMd);
                        UIManager.ShowToast(langModel.tds_unbind_success);
                    }
                }
            }, (error) => {
                if (string.IsNullOrEmpty(error.error_msg)){
                    UIManager.ShowToast(langModel.tds_unbind_error);
                } else{
                    UIManager.ShowToast(error.error_msg);
                }
            });
        }

        private DeleteAlertType getAlertType(){ //解绑最后一个第三方？和删除账号一样
            var num = 0;
            foreach (var md in dataList){
                if (md.status == (int) BindType.Bind){
                    num = num + 1;
                }
            }

            if (num == 1){
                return DeleteAlertType.DeleteThird;
            }

            return DeleteAlertType.Unbindthird;
        }


        private List<LoginTypeModel> GetSupportTypes(){ //网络和本地支持的过滤一下
            var list = new List<LoginTypeModel>();
            var sdkList = GetSdkTypes();
            var md = InitConfigModel.GetLocalModel();
            foreach (var a in sdkList){
                foreach (var b in md.data.configs.bindEntries){
                    if (a.typeName.ToLower().Equals(b.ToLower())){
                        list.Add(a);
                        break;
                    }
                }
            }

            return list;
        }

        private List<LoginTypeModel> GetSdkTypes(){ //sdk本地支持的第三方类型，目前只有tap，
            var list = new List<LoginTypeModel>();
            list.Add(new LoginTypeModel(LoginType.TapTap));
            return list;
        }

        public void errorViewTap(){
            XDGSDK.Log("点击了 errorViewTap");
            requestList();
        }
    }
}