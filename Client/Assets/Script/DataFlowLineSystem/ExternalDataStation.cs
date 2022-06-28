using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System.IO;
using System;

namespace LHC.PipeLineUIRefreshSyetem {
    public class ExternalDataStation<T, K> where T : struct where K : struct {
        const string windowConfigName = "Resources/UIWindowConfig/";
      

        readonly NiceTable pipeLineConfig = new NiceTable();
        readonly NiceTable dataConfig = new NiceTable();
        readonly Dictionary<T, Tuple<OpToken, OpToken>[]> astTreeDict;

        public ExternalDataStation(string windowName) {
            var textAsset = Resources.Load<TextAsset>("UIWindowConfig/" + windowName + "/" + "SpallScript");
            if (textAsset != null) astTreeDict = SpallInterpreter.GetASTreeDict<T, K>(textAsset.text);
            dataConfig = TableManager.GetTable(windowName + "DataConfig");
            pipeLineConfig = TableManager.GetTable(windowName + "PipeLineConfig");
        }

        public Dictionary<T, Refresher<T,K>> GetRefresherDict() {
            Dictionary<T, Refresher<T,K>> dict = new Dictionary<T, Refresher<T,K>>();
            pipeLineConfig.GetAllRecord().Values.Foreach(record => {
                T pipeLineType = (T)Enum.Parse(typeof(T), record.getData("PIPE_LINE_TYPE"));
                string componentName = record.getData("COMPONENT_NAME");
                OperateType operateType = (OperateType)Enum.Parse(typeof(OperateType),record.getData("OPERATE_TYPE"));
                HandlerFunc<K> handlerFunc = dataGroup => {
                    SpallDataBase<K>.curDataGroup = dataGroup;
                    var tupleArr = astTreeDict[pipeLineType];
                    object result = null;
                    for (int i = 0; i < tupleArr.Length; i++) {
                        var curTuple = tupleArr[i];
                        if (curTuple.field2 != null) {
                            if ((bool)curTuple.field1.GetValue()) {
                                result = curTuple.field2.GetValue();
                                break;
                            }
                        } else result = curTuple.field1.GetValue();
                    }
                    return result;
                };
                dict.Add(pipeLineType, new Refresher<T,K>(pipeLineType, componentName, operateType,handlerFunc));
            });
            return dict;
        }
        public DataGroup<K> GetDataGroup(int index) {
            Func<string, string, int, int> getValueByTable = (configName, colName, _index) => {
                return TableManager.GetTable(configName).GetRecord(_index).getData(colName);
            };

            DataGroup<K> dataGroup = new DataGroup<K>();
            dataConfig.GetAllRecord().Values.Foreach(record => {
                string tablePath = record.getData("TABLE_PATH");
                if (tablePath == "PUBLIC") {

                } else {
                    K dataType = (K)Enum.Parse(typeof(K), record.getData("DATA_TYPE"));
                    if (tablePath == "SERVER") dataGroup.AddDict(dataType, 0);
                    else if (char.IsNumber(tablePath[0])) dataGroup.AddDict(dataType, int.Parse(tablePath));
                     else {
                        string[] pathStrArr = record.getData("TABLE_PATH").ToString().Split('/');
                        if (pathStrArr.Length % 2 != 0) DEBUG.LogError("路径不正确");
                        else {
                            int newValue = index;
                            for (int i = 0; i < pathStrArr.Length / 2; i += 2) {
                                newValue = getValueByTable(pathStrArr[i], pathStrArr[i + 1], newValue);
                            }
                            dataGroup.AddDict(dataType, newValue);
                        }
                    }
                }
            });
            return dataGroup;
        }
    }

    



}
