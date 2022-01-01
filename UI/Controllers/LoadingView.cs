using com.xd.intl.pc;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : UIElement{

    public Image LoadingImage;
    public float rotateSpeed = 150;
  
    void Update () {  
        LoadingImage.transform.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime );  
    }  
}