using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LHC.PipeLineUIRefreshSyetem {
    public enum OperateType {
        SetIcon,
        SetName,
        SetLabel,
        SetSlider,
        SwitchGameObject,
        SwitchBtnState,
        DoAction,
    }
    
    public static class UIRefreshFunction {
        public static RefreshFunc GetRefreshFunc(string componentName,OperateType compType) {
            RefreshFunc func = null;
            Func<GameObject, UILabel> getLabel = go => GameCommon.FindComponent<UILabel>(go, componentName);
            Func<GameObject, UISprite> getUISprite = go => GameCommon.FindComponent<UISprite>(go, componentName);

            Func<GameObject, UISlider> getUISlider = go => GameCommon.FindComponent<UISlider>(go, componentName);
                    
            switch (compType) { 
                case OperateType.SwitchGameObject:
                    func = (go, value) => GameCommon.FindObject(go, componentName).SetActive((bool)value); break;
                case OperateType.SwitchBtnState:
                    func = (go, value) => GameCommon.FindComponent<UIImageButton>(go, componentName).isEnabled = (bool)value; 
                    break;
                case OperateType.SetLabel:
                    func = (go, value) => getLabel(go).text = value.ToString(); break;
                case OperateType.SetName:
                    func = (go, value) => getLabel(go).text = GameCommon.GetItemStringField((int)value, GET_ITEM_FIELD_TYPE.NAME);
                    break;
                case OperateType.SetIcon:
                    //func = (go, value) =>GameCommon.SetItemIcon(getUISprite(go), PackageManager.GetItemTypeByTableID((int)value), (int)value); 
					func = (go, value) => GameCommon.SetOnlyItemIcon(go,componentName,(int)value);   
				break;

                case OperateType.SetSlider:
                    func = (go, value) => getUISlider(go).value=(float)value;
                    break;
            }
            return func;
        }
    }


    
}
