using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LHC.PipeLineUIRefreshSyetem {
    /*
        <T>：数据类别
        <K>：流水线类别
    */

    public delegate void RefreshFunc(GameObject uiBoard, object param);
    public delegate object HandlerFunc<K>(DataGroup<K> dataGroup) where K:struct;

    public class DataGroup<K> where K : struct {
        public Dictionary<K, ValueType> dict { get;private set; }

        public DataGroup() {
            dict = new Dictionary<K, ValueType>();
        }
        public void AddDict(K dataType,ValueType dataValue) {
            dict.Add(dataType, dataValue);
        }
        public int GetInt(K dataType) {
            return (int)dict[dataType];
        }
        public float GetFloat(K dataType) {
            if (dataType is int) return (float)(int)dict[dataType];
            else return (float)dict[dataType];
        }
        public bool GetBool(K dataType) {
            return (bool)dict[dataType];
        }

        public ValueType GetValueType(K dataType) {
            return dict[dataType];
        }
        public void SetDataValue(DataGroup<K> newGrop) {
            foreach (var newData in newGrop.dict) {
                foreach (var data in dict) {
                    if (newData.Key.Equals(data.Key))
                        dict[newData.Key] = newData.Value;
                }
            }
        }
        


        public void SetDictValue(K dataType,ValueType dataValue) {
            dict[dataType] = dataValue;
        }
    }
    public struct DataGameObject<T, K>where T : struct where K : struct {
        public DataGroup<K> dataGroup;
        public GameObject go { get; private set; }

        public DataGameObject(DataGroup<K> dataGroup, GameObject go) {
            this.dataGroup = dataGroup;
            this.go = go;
        }

        public void SetData(DataGroup<K> newDataGroup) {
            dataGroup.SetDataValue(newDataGroup);
        }

        public void SetData(K dataType, ValueType value) {
            dataGroup.SetDictValue(dataType, value);
        }
    }
    public class Handler<T, K> where T : struct where K : struct {
        public readonly T lineType;
        public HandlerFunc<K> func { get; private set; }

        public Handler(T lineType, HandlerFunc<K> func) {
            this.lineType = lineType;
            this.func = func;
        }
    }
    public class Refresher<T,K> where T : struct {
        public readonly T lineType;
        public readonly string componentName;
        public RefreshFunc refrsherFunc { get; set; }
        public HandlerFunc<K> handlerFunc { get; set; }
        /* 
         * 除了Button上的Action，所有UI组件的刷新逻辑全由策划提供的相应参数获得
         * 而Action需要程序自行传入
         * Action的构建也通过UIRefreshFunction的接口
         */
        public Refresher(T lineType, string componentName, OperateType operateType,HandlerFunc<K> handlerFunc) {
            this.lineType = lineType;
            this.componentName = componentName;
            refrsherFunc = UIRefreshFunction.GetRefreshFunc(componentName, operateType);
            this.handlerFunc = handlerFunc;
        }

        public Refresher(T lineType, string componentName) {
            this.lineType = lineType;
        }
    }
    public class PipeLineFactory<T, K> where T : struct where K : struct {
        public Dictionary<T, Refresher<T,K>> refresherDict;
        public List<DataGameObject<T, K>> dataGoList;

        public void GetRefresherDict(Dictionary<T, Refresher<T,K>> refresherDict) {
            this.refresherDict = refresherDict;
        }
        public void GetDataGoList(List<DataGameObject<T, K>> dataGoList) {
            this.dataGoList = dataGoList;    
        }

        public void GetDataGo(DataGameObject<T, K> dataGo) {
            dataGoList = new List<DataGameObject<T,K>>();
            dataGoList.Add(dataGo);
        }

        

        /// <summary>
        /// 通过流水线类型获得Refresher的序列
        /// </summary>
        /// <param name="lineType">流水线类型</param>
        /// <returns>Refresher的序列</returns>
        public IEnumerable<Refresher<T,K>> FindRefreshersByLineType(T lineType) {
            //by chenliang
            //begin

//            return refresherDict.Where(dict => dict.Key.Equals(lineType)).Select(dict => dict.Value);
//-------------------
            //去除Linq相关
            List<Refresher<T, K>> tmpList = new List<Refresher<T, K>>();
            foreach (KeyValuePair<T, Refresher<T, K>> tmpPair in refresherDict)
            {
                if (tmpPair.Key.Equals(lineType))
                    tmpList.Add(tmpPair.Value);
            }
            return tmpList;

            //end
        }

        /*启动流水线相关方法*/
        public void ExecutePipeLine(T lineType) {
            ExecutePipeLine(lineType, dataGoList[0]);
        }

        public void ExecutePipeLine(T lineType, int index) {
            ExecutePipeLine(lineType, dataGoList[index]);
        }


        public void ExecutePipeLine(T lineType, int index, DataGroup<K> newDataGroup) {
            ExecutePipeLine(lineType, dataGoList[index], newDataGroup);
        }

        public void ExecutePipeLine(T lineType, int index, K dataType, ValueType value) {
            ExecutePipeLine(lineType, dataGoList[index], dataType,value);
        }

        public void ExecuteAllPipeLine() {
            dataGoList.ForEach(goData => {
                var enumerator = Enum.GetValues(typeof(T)).GetEnumerator();
                while (enumerator.MoveNext()) {
                    ExecutePipeLine((T)enumerator.Current, goData);
                }
            });
        }

        //by chenliang
        //begin

//         public void ExecuteAllPipeLineExcept(params T[] notExecutePipeline) {
//             dataGoList.ForEach(goData => {
//                 var enumerator = Enum.GetValues(typeof(T)).GetEnumerator();
//                 while (enumerator.MoveNext()) {
//                     var curTypeLine = (T)enumerator.Current;
//                     if(!notExecutePipeline.Contains(curTypeLine))
//                         ExecutePipeLine(curTypeLine, goData);
//                 }
//             });
//         }
//------------------
        //去除Linq相关
        public void ExecuteAllPipeLineExcept(List<T> notExecutePipeline)
        {
            dataGoList.ForEach(goData =>
            {
                var enumerator = Enum.GetValues(typeof(T)).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var curTypeLine = (T)enumerator.Current;
                    if (!notExecutePipeline.Contains(curTypeLine))
                        ExecutePipeLine(curTypeLine, goData);
                }
            });
        }

        //end
        void ExecutePipeLine(T lineType, DataGameObject<T, K> dataGo) {
            if (!refresherDict.ContainsKey(lineType)) {
                DEBUG.LogError("没有找到流水线:" + lineType);
                return;
            }
            var handlerFunc = refresherDict[lineType].handlerFunc;
            var needRefresher = refresherDict[lineType];

            needRefresher.refrsherFunc(dataGo.go, handlerFunc(dataGo.dataGroup));

          
        }
        void ExecutePipeLine(T lineType, DataGameObject<T, K> goData, DataGroup<K> newDataGroup) {
            if (!refresherDict.ContainsKey(lineType)) return;
            goData.SetData(newDataGroup);
            ExecutePipeLine(lineType, goData);
        }

        void ExecutePipeLine(T lineType, DataGameObject<T, K> goData, K dataType,ValueType value) {
            if (!refresherDict.ContainsKey(lineType)) return;
            goData.SetData(dataType,value);
            ExecutePipeLine(lineType, goData);
        }
    }

}

