using System.Collections;
using System.Collections.Generic;
using com.xd.intl.pc;
using TapTap.Common;
using UnityEngine;
using UnityEngine.UI;

public class PayHintAlert : UIElement{

    public Text titleTxt;
    public Text subTitleTxt;
    public Text mstTxt;
    public Text bottomTxt;

    private LanguageModel langMd;
    void Start(){
        langMd = LanguageMg.GetCurrentModel();
        titleTxt.text = langMd.tds_refund_login_restrict_title;
        subTitleTxt.text = langMd.tds_refund_login_restrict_sub_title;
        bottomTxt.text = $"<color=#999999>{langMd.tds_refund_custom_service_tip}</color><color=black>{langMd.tds_refund_contact_custom_service}</color>";
       
        var hasIOS = SafeDictionary.GetValue<bool>(extra, "hasIOS");
        var hasAndroid = SafeDictionary.GetValue<bool>(extra, "hasAndroid");
        if (hasIOS  && hasAndroid ){
            mstTxt.text = langMd.tds_refund_all_pay_tip; 
        }else if (hasIOS){
            mstTxt.text = langMd.tds_refund_ios_pay_tip;
        } else{
            mstTxt.text = langMd.tds_refund_android_pay_tip;   
        }
    }

    public void serviceTap(){
        XDGSDK.OpenCustomerCenter("","","");
    }
}