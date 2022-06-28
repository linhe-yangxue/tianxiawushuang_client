using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

//夺宝记录

public class GrabTreasureRecordWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_snatch_btn", new DefineFactoryLog<Button_grab_snatch_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_record_close_button", new DefineFactoryLog<Button_grab_record_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_record_window", new DefineFactoryLog<Button_grab_record_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
    }

    public override bool Refresh(object param)
    {
        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grab_record_grid");
        if (grid == null)
            return true;

        SC_GetRobbedHistoryList tmpResp = param as SC_GetRobbedHistoryList;
        int count = tmpResp.arr.Length;
        grid.MaxCount = count;
		if (grid .MaxCount == 0) {
			GameCommon .SetUIText (mGameObjUI, "Label_no_grab_record_tips",DataCenter.mStringList.GetData ((int)STRING_INDEX.ERROR_NO_GRAB_RECORD_TIPS,"STRING_CN"));
			GameCommon .SetUIVisiable (mGameObjUI, "Label_no_grab_record_tips", true);
		} else {
			GameCommon .SetUIVisiable (mGameObjUI, "Label_no_grab_record_tips", false);
		}
        for (int i = 0; i < count; i++)
        {
            GameObject tmpGOItem = grid.controlList[i];
            HistoryOfRobbed tmpHistoryData = tmpResp.arr[i];

            //碎片名称
            GameCommon.SetUIText(tmpGOItem, "fight_label", GameCommon.GetItemName(tmpHistoryData.tid));

            //玩家名
            GameCommon.SetUIText(tmpGOItem, "tips_label03", tmpHistoryData.name);

            //时间
            string tmpTimeString = __GetLeftTimeString(tmpHistoryData.robTime);
            GameCommon.SetUIText(tmpGOItem, "time_label", tmpTimeString);

            NiceData tmpBtnGrabData = GameCommon.GetButtonData(tmpGOItem, "grab_snatch_btn");
            if (tmpBtnGrabData != null)
            {
                tmpBtnGrabData.set("TARGET_UID", tmpHistoryData.uid);
                tmpBtnGrabData.set("TARGET_FRAG_ID", tmpHistoryData.tid);
            }
        }

        return true;
    }

    /// <summary>
    /// 获取被抢夺记录开始时间
    /// </summary>
    /// <returns></returns>
    private string __GetLeftTimeString(long grabTime)
    {
        string strGrabTime = "";
        DateTime currDateTime = BossRaidWindow.Get1970TimeFromServer(CommonParam.NowServerTime());
        DateTime grabDateTime = BossRaidWindow.Get1970TimeFromServer(grabTime);
        TimeSpan delta = currDateTime - grabDateTime;
        if (delta.Days >= 3)
            strGrabTime = "3天之前";
        else if (delta.Days > 0)
            strGrabTime = delta.Days.ToString() + "天之前";
        else if (delta.Hours > 0)
            strGrabTime = delta.Hours.ToString() + "小时之前";
        else if (delta.Hours == 0)
            strGrabTime = "1小时之前";
        return strGrabTime;
    }
}

/// <summary>
/// 去抢夺按钮
/// </summary>
class Button_grab_snatch_btn : CEvent
{
    public override bool _DoEvent()
    {
        int targetFragID = (int)getObject("TARGET_FRAG_ID");
        DataCenter.CloseWindow("GRABTREASURE_RECORD_WINDOW");
        DataCenter.SetData("GRABTREASURE_WINDOW", "GO_TO_FRAGMENT", targetFragID);

        //TODO 是否去抢指定玩家碎片

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_record_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_RECORD_WINDOW");

        return true;
    }
}
