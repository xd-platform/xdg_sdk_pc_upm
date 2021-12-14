using UnityEngine;
using System.Collections;


namespace SDK.PC{
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