using com.xd.intl.pc;
using UnityEngine;
using UnityEngine.UI;

namespace com.xd.intl.pc{
    public class SmallLoadingView: UIElement{

        public Image LoadingImage;
        public float rotateSpeed = 250;

        void Update(){
            LoadingImage.transform.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime);
        }
    }
}