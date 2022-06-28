using UnityEngine;
using System;
using Logic;
using System.Collections.Generic;
using DataTable;
using System.Diagnostics;

//读取推送表的数据
public class ReadPushInfo
{
    public int mId;                                 //id
    public int mPushId;                             //推送的id
    public string mPushTitle;                       //推送的标题
    public string mPushInfo;                        //推送的内容
    public string mPushTicker;                      //
    public string mPushAlertDate;                   //重复的时间间隔
}

// 推送类型
public enum PUSH_TYPE
{
    NONE = -1,
    STAMINA = 1010, // 体力恢复
    SPIRIT = 1011, // 精力恢复
    BEAT_DEMON_CARD = 1012, // 降魔令
    BLACK_MARKET_UPDATE = 1013, // 黑市刷新
    FAIRYLAND_FINISH = 1014, // 寻仙结束
    LAST_LOGIN = 1015, // 上次登录
}
public class PushMessageManager : MonoBehaviour
{
    public static PushMessageManager Self = null;

    //推送的队列
    public List<ReadPushInfo> mReadPushInfoList = new List<ReadPushInfo>();

    void Awake()
    {
        Self = this;
    }

    public void InitReadPushInfo()
    {
        mReadPushInfoList.Clear();
        int iCount = DataCenter.mLocalPushConfig.GetAllRecord().Count;
        for (int i = 1; i <= iCount; i++)
        {
            DataRecord record = DataCenter.mLocalPushConfig.GetRecord(i);
            ReadPushInfo rpi = new ReadPushInfo();
            rpi.mId = record.getData("ID");
            rpi.mPushId = record.getData("PUSH_ID");
            rpi.mPushTitle = record.getData("PUSH_TITLE");
            rpi.mPushInfo = record.getData("PUSH_INFO");
            rpi.mPushTicker = record.getData("PUSH_TICKER");
            rpi.mPushAlertDate = record.getData("PUSH_ALERT_DATE");
            mReadPushInfoList.Add(rpi);
        }

        if (!CommonParam.isUseSDK)
            DEBUG.LogError("AddLocalPush failed -- no use SDK");

        if (!CommonParam.isNeedPushMessage)
            DEBUG.LogError("AddLocalPush failed -- no need local push");

#if !UNITY_EDITOR && !NO_USE_SDK
        //U3DSharkSDK.Instance.RemoveAllLocalPush();
        for (int i = 0; i < mReadPushInfoList.Count; ++i)
        {
            if (!string.IsNullOrEmpty(mReadPushInfoList[i].mPushAlertDate) &&
                !mReadPushInfoList[i].mPushAlertDate.Equals("0"))
            {
                AddLocalPush(i);
            }
        }
#endif
    }

    public bool AddLocalPush(int i)
    {
#if !UNITY_EDITOR && !NO_USE_SDK        
        if (i >= 0 && i < mReadPushInfoList.Count)
        {
            U3DSharkBaseData pushData = new U3DSharkBaseData();
            pushData.SetData(U3DSharkAttName.PUSH_ID, mReadPushInfoList[i].mPushId.ToString());

            pushData.SetData(U3DSharkAttName.PUSH_TYPE, "0");
            pushData.SetData(U3DSharkAttName.PUSH_TYPE_DATA, mReadPushInfoList[i].mPushInfo);

            pushData.SetData(U3DSharkAttName.PUSH_TITLE, mReadPushInfoList[i].mPushTitle);
            pushData.SetData(U3DSharkAttName.PUSH_INFO, mReadPushInfoList[i].mPushTicker);
            pushData.SetData(U3DSharkAttName.PUSH_REPEAT_INTERVAL, ((int)GDEPushRepeatIntervalType.kDAY).ToString());

            pushData.SetData(U3DSharkAttName.PUSH_ALERT_DATE, mReadPushInfoList[i].mPushAlertDate);

            pushData.SetData(U3DSharkAttName.PUSH_NEED_NOTIFY, "0");
            pushData.SetData(U3DSharkAttName.PUSH_RECEIVE_TYPE, mReadPushInfoList[i].mPushId.ToString());
            pushData.SetData(U3DSharkAttName.PUSH_RECEIVE_INFO, mReadPushInfoList[i].mPushId.ToString());
            U3DSharkSDK.Instance.AddLocalPush(pushData);

            return true;

        }
#endif
        return false;
    }

    /// <summary>
    /// 添加推送
    /// </summary>
    /// <param name="pushiType">推送类型</param>
    /// <param name="type"></param>
    public bool AddLocalPush(PUSH_TYPE pushiType, long endTime)
    {
        if (!CommonParam.isUseSDK || !CommonParam.isNeedPushMessage)
            return false;
        
        ReadPushInfo readPushInfo = PushMessageManager.Self.GetLocalPush(pushiType);

        if (readPushInfo == null)
        {
            DEBUG.LogError("AddLocalPush failed -- read push info is empty");
            return false;
        }

        System.DateTime desTime = GameCommon.ConvertServerSecTimeTo1970(endTime);
        readPushInfo.mPushAlertDate = string.Format("{0:HH:mm:ss}", desTime);
        AddLocalPush(readPushInfo);

        return true;
    }

    public bool AddLocalPush(ReadPushInfo readPushInfo)
    {
        DEBUG.LogError("AddLocalPush -- pushID : " + readPushInfo.mPushId.ToString());
        if (CommonParam.isUseSDK)
        {
            if (readPushInfo != null)
            {
                ReadPushInfo info = mReadPushInfoList.Find(value => value.mPushId == readPushInfo.mPushId);
                if (info == null) return false;

                U3DSharkBaseData pushData = new U3DSharkBaseData();
                pushData.SetData(U3DSharkAttName.PUSH_ID, info.mPushId.ToString());

                pushData.SetData(U3DSharkAttName.PUSH_TYPE, "0");
                pushData.SetData(U3DSharkAttName.PUSH_TYPE_DATA, info.mPushInfo);

                pushData.SetData(U3DSharkAttName.PUSH_TITLE, info.mPushTitle);
                pushData.SetData(U3DSharkAttName.PUSH_INFO, info.mPushTicker);
                pushData.SetData(U3DSharkAttName.PUSH_REPEAT_INTERVAL, ((int)GDEPushRepeatIntervalType.kDAY).ToString());

                pushData.SetData(U3DSharkAttName.PUSH_ALERT_DATE, info.mPushAlertDate);

                pushData.SetData(U3DSharkAttName.PUSH_NEED_NOTIFY, "0");
                pushData.SetData(U3DSharkAttName.PUSH_RECEIVE_TYPE, info.mPushId.ToString());
                pushData.SetData(U3DSharkAttName.PUSH_RECEIVE_INFO, info.mPushId.ToString());
#if !UNITY_EDITOR && !NO_USE_SDK        
            U3DSharkSDK.Instance.AddLocalPush(pushData);
#endif
                return true;
            }
        }
        return false;
    }

    public ReadPushInfo GetLocalPush(PUSH_TYPE PushType)
    {
        if (CommonParam.isUseSDK)
        {
            int iIndex = mReadPushInfoList.FindIndex(x => x.mPushId == (int)PushType);
            switch (PushType)
            {
                case PUSH_TYPE.STAMINA:
                case PUSH_TYPE.SPIRIT:
                case PUSH_TYPE.BEAT_DEMON_CARD:
                case PUSH_TYPE.BLACK_MARKET_UPDATE:
                case PUSH_TYPE.FAIRYLAND_FINISH:
                case PUSH_TYPE.LAST_LOGIN:
                    if (iIndex < 0 || iIndex >= mReadPushInfoList.Count)
                    {
                        DEBUG.LogError("getLocalPush -- iIndex = " + iIndex + " is not exist. puthType = " + PushType.ToString());
                        DEBUG.LogError("Stack:" + new StackTrace().ToString());
                        return null;
                    }
                    else
                    {
                        return mReadPushInfoList[iIndex];
                    }
            }
        }
        return null;
    }

    public void RemoveLocalPush(string id)
    {
#if !UNITY_EDITOR && !NO_USE_SDK    
        U3DSharkSDK.Instance.RemoveLocalPush(id);    
#endif
    }

    public void RemoveAllLocalPush()
    {
#if !UNITY_EDITOR && !NO_USE_SDK        
        U3DSharkSDK.Instance.RemoveAllLocalPush();
#endif
    }

    // 接受/处理来自服务器的消息推送
    public class SC_PushMessage : DefaultNetEvent
    {
        public override bool _DoEvent()
        {
            var data = getObject("DATA") as NiceData;

            string msg = get("MSG");
            string tag = get("TAG");

            var notifyData = new NiceData();
            notifyData.set("TYPE", 6);
            notifyData.set("PARAM4", msg);

            Notification.Notify(NotifyType.Announcement, notifyData);
            return true;
        }
    }
}