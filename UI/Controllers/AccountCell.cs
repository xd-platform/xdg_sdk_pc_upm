using System;
using System.Collections;
using System.Collections.Generic;
using SDK.PC;
using UnityEngine;
using UnityEngine.UI;

public class AccountCell : UIElement{
    // Start is called before the first frame update

    private BindModel.Data cellModel;
    private LanguageModel langMd;

    public Image iconImage;
    public Text nameText;
    public Button bindBt;
    public Image arrowImage;

    public void refreshModel(BindModel.Data model, LanguageModel langMd){
        this.cellModel = model;
        this.langMd = langMd;
    }

    private void Start(){
        if (cellModel != null){
            nameText.text = cellModel.loginName;
            if (cellModel.loginType == (int) LoginType.TapTap){
                iconImage.sprite = Resources.Load("Images/type_icon_tap", typeof(Sprite)) as Sprite;
            }

            if (cellModel.status == 1){ //已绑定
                bindBt.transform.Find("Text").GetComponent<Text>().text = langMd.tds_unbind;
                bindBt.transform.Find("Text").GetComponent<Text>().color = new Color(15, 153, 153);
                arrowImage.sprite = Resources.Load("Images/arrow_gray", typeof(Sprite)) as Sprite;
            } else{
                bindBt.transform.Find("Text").GetComponent<Text>().text = langMd.tds_bind;
                bindBt.transform.Find("Text").GetComponent<Text>().color = Color.black;
                arrowImage.sprite = Resources.Load("Images/arrow_black", typeof(Sprite)) as Sprite;
            }
        }
    }

    public void bindButtonTap(){
        if (cellModel.status == 1){ //当前是绑定，开始解绑
            XDGSDK.Log("解绑");
        } else{ //开始绑定
        }
    }
}