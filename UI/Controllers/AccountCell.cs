﻿using UnityEngine;
using UnityEngine.UI;

namespace com.xd.intl.pc{
    public class AccountCell : UIElement{
        // Start is called before the first frame update

        private BindModel.Data cellModel;
        private LanguageModel langMd;

        public int cellIndex = 0;
        public Image iconImage;
        public Text nameText;
        public Button bindBt;
        public Image arrowImage;

        public void setModel(BindModel.Data model, LanguageModel langMd){
            this.cellModel = model;
            this.langMd = langMd;
        }

        public void refreshState(BindModel.Data model){
            this.cellModel = model;
            Start();
        }

        private void Start(){
            if (cellModel != null){
                nameText.text = langMd.tds_account_format.Replace("%s", cellModel.loginName);
                if (cellModel.loginType == (int) LoginType.TapTap){
                    iconImage.sprite = Resources.Load("Images/type_icon_tap", typeof(Sprite)) as Sprite;
                }

                if (cellModel.status == (int) BindType.Bind){
                    bindBt.transform.Find("Text").GetComponent<Text>().text = langMd.tds_unbind;
                    bindBt.transform.Find("Text").GetComponent<Text>().color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    arrowImage.sprite = Resources.Load("Images/arrow_gray", typeof(Sprite)) as Sprite;
                } else{
                    bindBt.transform.Find("Text").GetComponent<Text>().text = langMd.tds_bind;
                    bindBt.transform.Find("Text").GetComponent<Text>().color = Color.black;
                    arrowImage.sprite = Resources.Load("Images/arrow_black", typeof(Sprite)) as Sprite;
                }

                processShowOrNot();
            }
        }

        public void bindButtonTap(){
            OnCallback(cellIndex, "code 是cell index");
        }

        private void processShowOrNot(){ //处理绑定按钮显示或隐藏
            if (cellModel != null){
                var md = InitConfigModel.GetLocalModel();
                foreach (var netMd in md.data.configs.bindEntriesConfig){
                    if (cellModel.loginName.ToLower() == netMd.entryName.ToLower()){
                        if (cellModel.status == (int) BindType.Bind){
                            showBindBt(netMd.canUnbind); //可以显示解绑按钮
                        } else{
                            showBindBt(netMd.canBind);  //可以显示绑定按钮
                        }
                        break;
                    }
                }   
            }
        }

        private void showBindBt(int show){
            if (show == 1){
                bindBt.gameObject.SetActive(true);
                arrowImage.gameObject.SetActive(true);
            } else{
                bindBt.gameObject.SetActive(false);
                arrowImage.gameObject.SetActive(false);
            }
        }
    }
}