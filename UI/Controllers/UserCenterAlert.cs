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
    }
}