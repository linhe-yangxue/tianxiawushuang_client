//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.Linq;

//public delegate bool ConditionHandler(int value);
//public delegate string StringHandler(int value);
//public delegate void RefreshLogic(GameObject board,int value);
//public struct LogicDataHandler<T>
//{
//    public readonly T key;
//    public List<RefreshLogic> logicList;
//    public LogicDataHandler(T key,RefreshLogic logic){
//        this.key = key;
//        logicList = new List<RefreshLogic>();
//        logicList.Add(logic);
//    }
//}

//public static class RefreshLogicHelper
//{
  
//    static void AddButtonAction(GameObject button, Action action) {
//        var evt = button.GetComponent<UIButtonEvent>();
//        if (evt == null) DEBUG.LogError("No exist button event > " + button.name);
//        else evt.AddAction(action);
        
//    }
//    public static RefreshLogic GetLabelSwitch(string labelName,ConditionHandler condition ,string trueContent, string falseContent = null) {
//        return (board,value) => {
//            var label = GameCommon.FindComponent<UILabel>(board,labelName);
//            if (condition(value)) label.text = trueContent;
//            else label.text = falseContent;
//        };
//    }
   
//    public static RefreshLogic GetGameObjectSwitch(string goName, ConditionHandler condition) {
//        return (board, value) => {
//            GameCommon.FindObject(board, goName).SetActive(condition(value));

            
//        };
//    }

//    public static RefreshLogic GetBtnActionSwitch(string btnName, ConditionHandler condition, Action trueAction, Action falseAction = null) {
//        return (board,value) => {
//            if (condition(value)) AddButtonAction(GameCommon.FindObject(board, btnName), trueAction);
//            else AddButtonAction(GameCommon.FindObject(board, btnName), falseAction);
//        };
//    }

//    public static RefreshLogic GetBtnActionSetter(string btnName, Action<int> action) {
//        return (board, value) => AddButtonAction(GameCommon.FindObject(board, btnName), () => action(value));
//    }

//    //public static RefreshLogic GetBtnActionSetter(string btnName, ActionParams action) {
//    //    return (board, valueArr) => AddButtonAction(GameCommon.FindObject(board, btnName), () => action(valueArr));
//    //}


//    public static RefreshLogic GetLabelSetter(string labelName, StringHandler handler) {
//        return (grid, value) => {
//            GameCommon.FindComponent<UILabel>(grid, labelName).text = handler(value);
//        };

//    }

//    public static RefreshLogic GetUISpriteSetter(string spriteName) {
//        return (grid, value) => GameCommon.SetItemIcon(grid, spriteName, value);
//    }

//    public static void RefreshUIBoard<T, K>(T data, Dictionary<K,LogicDataHandler<K>> logicDict, GameObject board) where T : GetLogicData<K> {
//        var dataDict = data.GetDataDict();
//        dataDict.Foreach(dictItem => {
//            if (logicDict.ContainsKey(dictItem.Key)) {

//                logicDict[dictItem.Key].logicList.ForEach(logic=>logic(board, dictItem.Value));

//            }
//        });
//    }

//    public static Action<LogicDataHandler<T>> GetRefreshLogicListAdder<T>(Dictionary<T,LogicDataHandler<T>> dict) where T:struct{
//        return logicDataHandler => {
            
//            if (dict == null) dict = new Dictionary<T, LogicDataHandler<T>>();
//            else {
//                if (dict.ContainsKey(logicDataHandler.key)) 
//                    dict[logicDataHandler.key].logicList.Add(logicDataHandler.logicList[0]);
//                else dict.Add(logicDataHandler.key, logicDataHandler);
//            }
//        }; 
//    }
//}

