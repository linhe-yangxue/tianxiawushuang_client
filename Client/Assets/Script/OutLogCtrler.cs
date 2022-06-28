using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.NetworkInformation;

public class OutLogCtrler:MonoBehaviour {
    /*
     *手机系统版本
     *mac地址
     *现有内存
     *机型
     *客户端版本号
     *IP
     */
    class OutLogInfo {
        public readonly int logID;
        public readonly string logContent;
        public readonly string stackTrace;
        public readonly LogType logType;

        public OutLogInfo(string logContent,string stackTrace,int logID,LogType logType) {
            this.logID=logID;
            this.logContent=logContent;
            this.stackTrace=stackTrace;
            this.logType=logType;
        }

        public string LogContent {
            get {
                return "LOG_NUM:"+logID+"     LOG_TYPE:"+logType+"\n"+logType+logContent+"\n"+stackTrace;
            }
        }
    }

    
    List<LogType> needCathLogTypeList;
    List<OutLogInfo> outLogList_temp=new List<OutLogInfo>();
    List<OutLogInfo> outLogList=new List<OutLogInfo>();

    int curLogID;
    string outPath;


    void Start() {
        needCathLogTypeList=new List<LogType>() {
            LogType.Error,
            LogType.Exception
        };
        outPath=Application.persistentDataPath+"/outLog.txt";
        if(File.Exists(outPath)) File.Delete(outPath);
        Application.RegisterLogCallback(HandleLog);
    }


    IEnumerator DebugIE() {
        yield return new WaitForSeconds(5);
        DEBUG.LogError(outPath);
    }

    void Update() {
        if(outLogList_temp.Count>0&&outLogList_temp.Count<100) {
            var temp=outLogList_temp.ToArray();
            temp.Foreach(log => {
                using(StreamWriter writer=new StreamWriter(outPath,true,Encoding.UTF8)) {
                    writer.WriteLine("LOG_NUM:"+log.logID.ToString()+"     LOG_TYPE:"+log.logType);
                    writer.WriteLine(log.logContent);
                    writer.WriteLine(log.stackTrace);
                    writer.WriteLine("__________");
                };
                outLogList_temp.Remove(log);
            });
        }
    }

    void HandleLog(string logContent,string stackTrace,LogType logType) {
        if(!needCathLogTypeList.Contains(logType)) return;
        var outLogInfo=new OutLogInfo(logContent,stackTrace,curLogID,logType);
        curLogID++;
        outLogList_temp.Add(outLogInfo);
        outLogList.Add(outLogInfo);
    }

    //void OnGUI() {
    //    if(outLogList.Count==0) return;
    //    var latestLog=outLogList[outLogList.Count-1];
    //    GUI.color=Color.red;
    //    GUILayout.Label("LOG_NUM:"+latestLog.logID.ToString()+"     LOG_TYPE:"+latestLog.logType);
    //    GUILayout.Label(latestLog.logContent);
    //    GUILayout.Label(latestLog.stackTrace);   
    //}

    void SendEmail(OutLogInfo outLogInfo) {
        DEBUG.Log("SendMail");
        string smtp="smtp.gdegame.com";

        string from="lihanchi@gdegame.com";
        string pwd="gdegame";
        string to="lihanchi@gdegame.com";
        string title=outLogInfo.logType.ToString();
        string body=outLogInfo.LogContent;
        SendMailUtil.SendMail(smtp,null,from,pwd,to,title,body);
            
    }
}
