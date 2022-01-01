using UnityEngine;
using System.Collections;


namespace com.xd.intl.pc{
    public class UIBase : MonoBehaviour{
        public virtual void OnEnter(){
        }

        public virtual void OnPause(){
            gameObject.SetActive(false);
        }

        public virtual void OnResume(){
            gameObject.SetActive(true);
        }

        public virtual void OnExit(){
            Destroy(gameObject);
        }
    }
}