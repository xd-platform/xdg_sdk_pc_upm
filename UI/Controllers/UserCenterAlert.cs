using System;
using System.Collections;
using System.Collections.Generic;
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

    public GameObject accountCellPrefab;
    private int cellCount = 6;
    public GameObject Content;
        
    void Start(){
        userMd = XDGUserModel.GetLocalModel();
        langModel = LanguageMg.GetCurrentModel();
        titleTxt.text = langModel.tds_account_safe_info;
        
        infoTitleTxt.text = langModel.tds_account_info;
        typeTxt.text = $"{langModel.tds_current_account_prefix} ({GetLoginTypeName()})";
        idTxt.text = $"ID: {userMd.data.userId}";
        
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
        InitView();
    }

    private List<GameObject> cellList = new List<GameObject>();
    public void InitView(){
        // for (int i = 0; i < cellCount; i++){
            GameObject gameObj = Instantiate(Resources.Load("Prefabs/AccountCell")) as GameObject;
            gameObj.name = "prefab1";
            gameObj.transform.localScale= Vector3.one * 2;
            gameObj.transform.SetParent(Content.transform);
            gameObj.transform.localPosition = new Vector3(353, -17, 0);
            cellList.Add(gameObj);
        // }
    }

}