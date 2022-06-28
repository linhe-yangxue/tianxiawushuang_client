using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HttpModuleForStart : MonoBehaviour {

    ////public static HttpModuleForStart instance;
    //private int mMessageCount=0;
    //public Func<string,CallBack,string,bool> mRespListener=null;


    //public class WWWData {
    //    public WWW www;
    //    public string url;
    //    public string context;
    //    public CallBack successMethod;
    //    public CallBack failMethod;
    //    public bool failed;
    //}


    //void SendMessage(MessageBase message,string url,bool isNeedWaitEffect=true) {

    //    WWWData data=new WWWData();
    //    data.failed=false;
    //    data.url=url;
    //    data.context=JCode.Encode(message);
    //    //data.successMethod=successMethod;
    //    //data.failMethod=failMethod;

    //    if(isNeedWaitEffect)
    //        Net.StartWaitEffect();

    //    StartCoroutine("GetRoutine",data);
    //    StartCoroutine("TimeoutRoutine",data);
    //}




    //IEnumerator GetRoutine(WWWData wwwData) {
    //    //WWWForm form = new WWWForm ();
    //    //foreach (KeyValuePair<string, string> post_arg in post) {
    //    //	form.AddField(post_arg.Key, post_arg.Value);
    //    //}
    //    byte[] data=System.Text.Encoding.UTF8.GetBytes(wwwData.context);
    //    wwwData.www=new WWW(System.Uri.EscapeUriString(wwwData.url),data);
    //    //DEBUG.Log("send message succuss. wwwData.url = " + wwwData.url + "\n wwwData.context" + wwwData.context + "\n messagecount"+  mMessageCount);
    //    mMessageCount++;
    //    DEBUG.Log("send message succuss. wwwData.url = "+wwwData.url+"\n wwwData.context = "+wwwData.context+"\n data.Length = "+data.Length+"\n messagecount = "+mMessageCount);

    //    yield return wwwData.www;
    //    mMessageCount--;
    //    DEBUG.Log("get message succuss. wwwData.www.text = "+wwwData.www.text+"\n messagecount = "+mMessageCount);
    //    if(mMessageCount==0) {
    //        StopCoroutine("TimeoutRoutine");
    //        Net.StopWaitEffect();
    //    }

    //    if(wwwData.www!=null&&wwwData.www.text!="") {
    //        RespMessage respMessage=JCode.Decode<RespMessage>(wwwData.www.text);
    //        if(respMessage.ret!=1) {
    //            //by chenliang
    //            //begin

    //            //                 if (-4 == respMessage.ret)
    //            //                 {
    //            //                     DataCenter.OpenWindow(UIWindowString.connectError);
    //            //                 }
    //            //------------------------
    //            if(-4==respMessage.ret) {
    //                DataCenter.OpenWindow(UIWindowString.connectError);
    //                DataCenter.SetData(UIWindowString.connectError,"WINDOW_CONTENT",STRING_INDEX.ERROR_NET_GAME_TOKEN_INVALID);
    //            } else if(respMessage.ret==-3) {
    //                //登录服务器网络令牌过期
    //                DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
    //                DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW","WINDOW_CONTENT",STRING_INDEX.ERROR_NET_LOGIN_TOKEN_INVALID);
    //            } else if(respMessage.ret==-10000) {
    //                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HOTUPDATE_VERSION_TOO_LOW);
    //            }
    //                /*Temp+++++++++++++++++++++++++++++++++++++++++*/
    //              else if(respMessage.ret==1314) {
    //                DataCenter.OpenMessageWindow("不能弹劾会长");
    //            } else if(respMessage.ret==-7) {
    //                DataCenter.OpenMessageWindow("外挂被检测");
    //            }
    //                //end
    //            else {
    //                DataCenter.OpenMessageWindow(respMessage.ret.ToString());
    //            }
    //            DEBUG.LogError("error is "+respMessage.ret.ToString());

    //            //wwwData.failMethod(respMessage.ret.ToString());
    //            wwwData.failed=true;
    //        } else if(respMessage.ret==1) {
    //            //DEBUG.Log("request ok" + wwwData.www.text);
    //            if(mRespListener!=null) {
    //                bool reset=mRespListener(respMessage.pt,wwwData.successMethod,wwwData.www.text);

    //                if(reset) {
    //                    mRespListener=null;
    //                }
    //            } else {
    //                //wwwData.successMethod(wwwData.www.text);
    //            }
    //        }
    //    } else {
    //        //by chenliang
    //        //begin

    //        if(wwwData.www!=null&&wwwData.www.error!=null&&wwwData.www.error!="") {
    //            string tmpWWWError=wwwData.www.error;
    //            if(tmpWWWError.IndexOf("500 ")!=-1)          //显示500错误
    //                DataCenter.OpenMessageWindow("500");
    //            else if(tmpWWWError.IndexOf("404 ")!=-1)     //显示404错误
    //                DataCenter.OpenMessageWindow("404");
    //        }

    //        //end
    //        DEBUG.LogError("request error"+wwwData.www.text);
    //    }
    //}


    //private IEnumerator TimeoutRoutine(WWWData wwwData) {
    //    yield return new WaitForSeconds(15f);

    //    mMessageCount--;
    //    if(!wwwData.failed) {
    //        if(mMessageCount==0) {
    //            StopCoroutine("GetRoutine");
    //        }
    //        Net.StopWaitEffect();
    //        wwwData.failed=true;

    //        //wwwData.failMethod(string.Empty);
    //        //by chenliang
    //        //begin

    //        //            DataCenter.OpenWindow(UIWindowString.connectError);
    //        //------------------
    //        //OpenTokenErrorWindow();

    //        //end
    //        //StopCoroutine("GetRoutine");
    //    }
    //}
    ////by chenliang
    

}
