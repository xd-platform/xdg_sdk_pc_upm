﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace com.xd.intl.pc{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour{
        public static readonly int RESULT_FAILED = -1;
        public static readonly int RESULT_SUCCESS = 0;
        public static readonly int RESULT_BACK = 1;
        public static readonly int RESULT_CLOSE = 2;

        private static GameObject managerObject;
        private GameObject containerObj;
        private Button toastBt;
        private readonly List<UIElement> uiElements = new List<UIElement>();

        public static void Dismiss(){
            Instance().PopUIElement(null);
        }

        public static void Dismiss(string targetName){
            Instance().PopUIElement(targetName);
        }

        public static void DismissAll(){
            UIManager mg = Instance();
            if (mg.containerObj != null){
                UIElement element = null;
                if (mg.uiElements.Count > 0){
                    element = mg.uiElements[mg.uiElements.Count - 1];
                }

                UIAnimator animator = UI.GetComponent<UIAnimator>(mg.containerObj);
                animator.DoExitAnimation(element, null, () => {
                    for (int i = mg.uiElements.Count - 1; i >= 0; i--){
                        mg.uiElements[i].OnExit();
                    }

                    mg.uiElements.Clear();
                    mg.DestroyContainer();
                });
            }
        }

        public static void ShowUI<T>(Dictionary<string, object> configs, Action<int, object> callback)
            where T : UIElement{
            Instance().PushUIElement<T>(typeof(T).Name, configs, callback);
        }

        public static void ShowToast(string msg){
            var mg = Instance();
            if (mg.containerObj == null){
                mg.CreateContainer();
            }

            if (mg.toastBt != null){
                mg.toastBt.transform.Find("Text").GetComponent<Text>().text = msg;
                mg.toastBt.transform.SetSiblingIndex(1000);
                mg.toastBt.gameObject.SetActive(true);
                mg.Invoke("hideToast", 1);
            } else{
                XDGSDK.Log("提示控件是空");
            }
        }

        public void hideToast(){
            var mg = Instance();
            if (mg.toastBt != null){
                mg.toastBt.transform.SetSiblingIndex(0);
                mg.toastBt.gameObject.SetActive(false);
            }

            if (mg.uiElements.Count == 0){
               DismissAll();   
            }
        }

        public static void ShowLoading(){
            ShowUI<LoadingView>(null, null);
        }
        
        public static void DismissLoading(){
            DismissAll();
        }

        private static UIManager Instance(){
            if (managerObject == null){
                managerObject = new GameObject{
                    name = "UIManager"
                };
                managerObject.AddComponent<UIManager>();
                DontDestroyOnLoad(managerObject);
            }

            return managerObject.GetComponent<UIManager>();
        }

        private void CreateContainer(){
            containerObj = Instantiate(Resources.Load("Prefabs/ContainerWindow")) as GameObject;
            containerObj.name = "ContainerWindow";
            DontDestroyOnLoad(containerObj);
            UIElement containerElement = UI.GetComponent<ContainerWindow>(containerObj);
            UIAnimator containerAnimator = UI.GetComponent<UIAnimator>(containerObj);
            containerElement.OnEnter();
            containerAnimator.DoEnterAnimation(null, containerElement, () => { });

            toastBt = containerObj.gameObject.GetComponentInChildren<Button>(); //toast button
            toastBt.gameObject.SetActive(false);
        }

        private void DestroyContainer(){
            if (containerObj != null){
                UIElement containerElement = UI.GetComponent<ContainerWindow>(containerObj);
                UIAnimator containerAnimator = UI.GetComponent<UIAnimator>(containerObj);
                containerElement.OnEnter();
                containerAnimator.DoExitAnimation(containerElement, null, () => {
                    Destroy(containerObj);
                    containerObj = null;
                });
            }
        }

        private void PushUIElement<T>(
            string prefabName,
            Dictionary<string, object> extra,
            Action<int, object> callback) where T : UIElement{
            GameObject gameObj = Instantiate(Resources.Load("Prefabs/" + prefabName)) as GameObject;
            if (gameObj == null){
                XDGSDK.Log("没找到 prefab named： \"" + prefabName + "\"");
            } else{
                if (uiElements.Count == 0 && containerObj == null){
                    CreateContainer();
                }

                gameObj.name = prefabName;
                DontDestroyOnLoad(gameObj);
                UIElement element = UI.GetComponent<T>(gameObj);
                element.Extra = extra;
                element.Callback += callback;
                element.transform.SetParent(containerObj.transform, false);

                UIElement lastElement = null;
                if (uiElements.Count > 0){
                    lastElement = uiElements[uiElements.Count - 1];
                }

                uiElements.Add(element);

                UIAnimator animator = UI.GetComponent<UIAnimator>(containerObj);
                element.OnEnter();
                animator.DoEnterAnimation(lastElement, element, () => {
                    //隐藏前一个弹框
                    // if (lastElement != null){
                    //     lastElement.OnPause();
                    // }
                });
            }
        }

        private void PopUIElement(string targetName){
            if (containerObj == null || uiElements.Count == 0){
                XDGSDK.Log("没有 UIElement 子类可处理.");
            } else{
                UIElement element = uiElements[uiElements.Count - 1];

                if (targetName != null && !targetName.Equals(element.name)){
                    XDGSDK.Log("没找到 UIElement 子类: " + targetName);
                    return;
                }

                uiElements.RemoveAt(uiElements.Count - 1);

                UIElement lastElement = null;
                if (uiElements.Count > 0){
                    lastElement = uiElements[uiElements.Count - 1];
                }

                UIAnimator animator = UI.GetComponent<UIAnimator>(containerObj);
                if (lastElement != null){
                    lastElement.OnResume();
                }

                animator.DoExitAnimation(element, lastElement, () => {
                    element.OnExit();
                    if (lastElement != null){
                    }

                    if (uiElements.Count == 0){
                        DestroyContainer();
                    }
                });
            }
        }
    }
}