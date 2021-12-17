using System.Collections;
using System.Collections.Generic;
using SDK.PC;
using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

public class PrivacyAlert : UIElement{
    public Text titleText;

    public Browser leftWeb;
    public Browser rightWeb;

    public Button leftCheckButton;
    public Button leftCheckTextBt;
    
    public Button rightCheckButton;
    public Button rightCheckTextBt;
    
    public Button centerCheckButton;
    public Button centerCheckTextBt;

    public Button sureButton;

    
    // Start is called before the first frame update
    void Start(){
        leftWeb.Url = "https://protocol.xd.com/sdk-privacy-1.0.0.html";
        rightWeb.Url = "https://protocol.xd.com/sdk-agreement-1.0.0.html";
        
    }

    public void leftCheckTap(){
        XDGSDK.Log("左边");
        
    }

    public void rightCheckTap(){
        XDGSDK.Log("右边");
        
    }

    public void centerCheckTap(){
        XDGSDK.Log("中间");
    }
    
    public void SureTap(){
        XDGSDK.Log("确定");
    }

}
