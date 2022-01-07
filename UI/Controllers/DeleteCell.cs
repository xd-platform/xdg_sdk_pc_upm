using System.Collections.Generic;
using com.xd.intl.pc;
using UnityEngine.UI;

namespace com.xd.intl.pc{
    public class DeleteCell : UIElement{
        public Button deleteBt;

        public void setDeleteText(string text){
            deleteBt.transform.Find("Text").GetComponent<Text>().text = text;
        }

        public void deleteTaped(){
            var dic = new Dictionary<string, object>(){
                {"loginType", 0},
            };
            UIManager.ShowUI<DeleteAccountAlert>(dic, (code, msg) => { OnCallback(UIManager.RESULT_SUCCESS, "删除账号"); });
        }
    }
}