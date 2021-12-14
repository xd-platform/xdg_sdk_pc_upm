using System.Collections.Generic;
using UnityEngine.UI;

namespace SDK.PC{
    public class TestAlert : UIElement{
        public Button CloseButton;

        public override Dictionary<string, object> Extra{
            get{ return extra; }
            set{ extra = value; }
        }

        public void CloseTaped(){
            UIManager.Dismiss("TestAlert");
        }
    }
}