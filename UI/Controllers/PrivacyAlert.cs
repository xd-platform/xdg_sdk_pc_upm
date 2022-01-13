using com.xd.intl.pc;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace com.xd.intl.pc{
    public class PrivacyAlert : UIElement{
        public Text titleText;

        public Button leftCheckButton;
        public Button leftCheckTextBt;
        public TMP_Text leftText;

        public Button rightCheckButton;
        public Button rightCheckTextBt;
        public TMP_Text rightText;

        public Button centerCheckButton;
        public Button centerCheckTextBt;

        public Button sureButton;

        private InitConfigModel cfgModel;
        private LanguageModel langModel;
        private bool leftSelected = false;
        private bool rightSelected = false;
        private bool centerSelected = false;
        private string centerStr = null; //为空则不显示

        private TextMesh a;

        void Start(){
            cfgModel = InitConfigModel.GetLocalModel();
            langModel = LanguageMg.GetCurrentModel();

            cfgModel.GetPrivacyTxt(cfgModel.data.configs.serviceAgreementTxt, (txt) => { leftText.text = txt; });
            cfgModel.GetPrivacyTxt(cfgModel.data.configs.serviceTermsTxt, (txt) => { rightText.text = txt; });
            RefreshLanguage();
        }

        private void RefreshLanguage(){
            titleText.text = langModel.tds_terms_agreement;
            sureButton.transform.Find("Text").GetComponent<Text>().text = langModel.tds_confirm_agreement;
            leftCheckTextBt.transform.Find("Text").GetComponent<Text>().text = langModel.tds_service_terms_agreement;
            rightCheckTextBt.transform.Find("Text").GetComponent<Text>().text = langModel.tds_service_terms_agreement;

            //中间按钮处理
            centerStr = GetCenterText();
            if (string.IsNullOrEmpty(centerStr)){
                centerCheckButton.gameObject.SetActive(false);
                centerCheckTextBt.gameObject.SetActive(false);
            } else{
                centerCheckTextBt.transform.Find("Text").GetComponent<Text>().text = centerStr;
            }

            Invoke("updatePosition", 0.2f);
        }

        private void updatePosition(){
            var x = centerCheckTextBt.transform.GetComponent<RectTransform>().sizeDelta.x / 2;
            centerCheckButton.transform.localPosition = new Vector3(-x, -133, 0);
        }

        public void leftCheckTap(){
            leftSelected = !leftSelected;
            updateCheckState(leftCheckButton, leftSelected);
            updateCenterButton();
        }

        public void rightCheckTap(){
            rightSelected = !rightSelected;
            updateCheckState(rightCheckButton, rightSelected);
            updateCenterButton();
        }

        public void centerCheckTap(){
            centerSelected = !centerSelected;
            updateCheckState(centerCheckButton, centerSelected);
            updateCenterButton();
        }

        public void SureTap(){
            var tmp = leftSelected && rightSelected;
            if (IsInNorthAmerica()){
                tmp = tmp && centerSelected;
            }

            if (tmp){ //按钮可以提交
                if (IsInKrAndPushEnable()){ //保存韩国推送状态
                    XDGSDK.SetPushServiceEnable(centerSelected);
                }

                InitConfigModel.UpdatePrivacyState();
                UIManager.Dismiss();
                OnCallback(UIManager.RESULT_SUCCESS, "点击同意");
            }
        }

        private void updateCheckState(Button button, bool selected){
            if (selected){
                button.GetComponent<Image>().sprite = Resources.Load("Images/check_true", typeof(Sprite)) as Sprite;
            } else{
                button.GetComponent<Image>().sprite = Resources.Load("Images/check_false", typeof(Sprite)) as Sprite;
            }
        }

        private void updateCenterButton(){
            var tmp = leftSelected && rightSelected;
            if (IsInNorthAmerica()){ //年龄必须选中！通知开关选择不是强制的
                tmp = tmp && centerSelected;
            }

            selectCenterBt(tmp);
        }

        private void selectCenterBt(bool selected){
            if (selected){
                sureButton.GetComponent<Image>().sprite =
                    Resources.Load("Images/button_green", typeof(Sprite)) as Sprite;
            } else{
                sureButton.GetComponent<Image>().sprite =
                    Resources.Load("Images/button_gray", typeof(Sprite)) as Sprite;
            }
        }

        private string GetCenterText(){
            string txt = null;
            if (IsInKrAndPushEnable()){
                return langModel.tds_push_agreement;
            } else if (IsInNorthAmerica()){
                return langModel.tds_is_adult_agreement;
            }

            return txt;
        }

        private bool IsInKrAndPushEnable(){ //韩国
            var str = cfgModel.data.configs.region.ToLower();
            bool canPush = cfgModel.data.configs.isKRPushServiceSwitchEnable;
            if (canPush && "kr".Equals(str)){
                return true;
            }

            return false;
        }

        private bool IsInNorthAmerica(){ //北美
            var str = cfgModel.data.configs.region.ToLower();
            return "us".Equals(str);
        }
    }
}