using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace com.xd.intl.pc{
    public static class UI{
        public static T GetComponent<T>(GameObject obj) where T : Component{
            T component = obj.GetComponent<T>();

            if (component == null){
                component = obj.AddComponent<T>();
            }

            return component;
        }
    }
}