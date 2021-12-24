﻿using System.Collections.Generic;
using SDK.PC;
using UnityEngine;
using UnityEngine.UI;

public class UserCenterAlert : UIElement{
    public Text titleTxt;
    public Text infoTitleTxt;
    public Text typeTxt;
    public Text idTxt;

    private LanguageModel langModel;
    private XDGUserModel userMd;

    public GameObject Content;
    private float cellHeight = 36.0f;

    private List<BindModel.Data> dataList;
    private List<GameObject> cellList;

    void Start(){
        userMd = XDGUserModel.GetLocalModel();
        langModel = LanguageMg.GetCurrentModel();
        titleTxt.text = langModel.tds_account_safe_info;

        infoTitleTxt.text = langModel.tds_account_info;
        typeTxt.text = $"{langModel.tds_current_account_prefix} ({GetLoginTypeName()})";
        idTxt.text = $"ID: {userMd.data.userId}";
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
        XDGSDK.Log("点击复制");
    }

    private void requestList(){
        Api.GetBindList((success, bindModel) => {
            if (success){
                dataList = new List<BindModel.Data>();
                var supportList = GetSupportTypes();
                foreach (var st in supportList){ //本地支持的都要显示
                    var md = new BindModel.Data();
                    md.loginType = st.typeValue;
                    md.loginName = st.typeName;

                    foreach (var netMd in bindModel.data){ //插入对应的状态
                        if (st.typeValue == netMd.loginType){
                            md.status = netMd.status;
                            md.bindDate = netMd.bindDate;
                            break;
                        }
                    }
                    dataList.Add(md);
                }
                addCellView();
            } else{
                XDGSDK.Log("列表请求失败");
            }
        });
    }

    private void addCellView(){
        cellList = new List<GameObject>();
        for (int i = 0; i < dataList.Count; i++){
            GameObject gameObj = Instantiate(Resources.Load("Prefabs/AccountCell")) as GameObject;
            gameObj.name = "AccountCell" + i;
            gameObj.transform.SetParent(Content.transform);
            gameObj.transform.localPosition = new Vector3(350, -cellHeight / 2 - (cellHeight * i), 0);
            gameObj.transform.localScale = Vector3.one;
            cellList.Add(gameObj);

            AccountCell cell = gameObj.GetComponent<AccountCell>();
            cell.refreshModel(dataList[i], langModel);
            cell.Callback += (code, msg) => { //cell 事件回调

            };
        }

        if (userMd.data.loginType == (int) LoginType.Guest){ //游客添加删除
            GameObject gameObj = Instantiate(Resources.Load("Prefabs/DeleteCell")) as GameObject;
            gameObj.name = "DeleteCell";
            gameObj.transform.SetParent(Content.transform);
            gameObj.transform.localPosition = new Vector3(350, -cellHeight / 2 - (cellHeight * cellList.Count) - 10, 0);
            gameObj.transform.localScale = Vector3.one;
            
            DeleteCell cell = gameObj.GetComponent<DeleteCell>();
            cell.setDeleteText(langModel.tds_delete_account);
            cell.Callback += (code, msg) => { //delete 事件回调

            };
        }
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
}