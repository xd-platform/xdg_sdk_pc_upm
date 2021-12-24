using SDK.PC;
using UnityEngine.UI;

public class DeleteCell : UIElement{
    public Button deleteBt;

    public void setDeleteText(string text){
        deleteBt.transform.Find("Text").GetComponent<Text>().text = text;
    }

    public void deleteTaped(){
        OnCallback(UIManager.RESULT_SUCCESS, "点击删除");
    }
}