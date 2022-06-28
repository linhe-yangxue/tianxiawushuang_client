using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//符灵探险好友仙境选择界面

public class FairylandFriendSelWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_way_visit_button", new DefineFactoryLog<Button_explore_way_visit_button>());
        EventCenter.Self.RegisterEvent("Button_explore_friend_affirm_button", new DefineFactoryLog<Button_explore_friend_affirm_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandFriendList());
    }

    public override bool Refresh(object param)
    {
        SC_Fairyland_GetFairylandFriendList resp = param as SC_Fairyland_GetFairylandFriendList;

        UIGridContainer gridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "friend_grid");
        int count = resp.ffList.Length;
        gridContainer.MaxCount = count;
        for (int i = 0; i < count; i++)
            __RefreshItem(gridContainer.controlList[i], resp.ffList[i]);

        return true;
    }

    private void __RefreshItem(GameObject item, FairylandFriend friendData)
    {
        //好友名称
        GameCommon.SetUIText(item, "name_label", friendData.name);

        //好友图标
        GameCommon.SetPalyerIcon(GameCommon.FindComponent<UISprite>(item, "item_icon"), friendData.iconIndex);

        //已征服仙境
        GameCommon.SetUIText(item, "yet_name_label", friendData.conquerdNum.ToString());
        //正在征服仙境
        GameCommon.SetUIText(item, "cur_name_label", friendData.exploringNum.ToString());
        //暴动中仙境
        GameCommon.SetUIText(item, "aim_name_label", friendData.riotingNum.ToString());

        //是否有暴动
        GameCommon.SetUIVisiable(item, "riot_sprite", friendData.riotingNum > 0);

        //按钮数据
        NiceData tmpBtnData = GameCommon.GetButtonData(item, "explore_way_visit_button");
        if (tmpBtnData != null)
        {
            tmpBtnData.set("FRIEND_ID", friendData.zuid);
            tmpBtnData.set("FRIEND_LEVEL", friendData.level);
			tmpBtnData.set("FRIEND_NAME", friendData.name);
			tmpBtnData.set ("FRIEND_TID", friendData.iconIndex);
        }
    }
}

/// <summary>
/// 拜访好友
/// </summary>
class Button_explore_way_visit_button : CEvent
{
    public override bool _DoEvent()
    {
        string tmpFriendId = (string)getObject("FRIEND_ID");
        int tmpFriendLevel = (int)getObject("FRIEND_LEVEL");
		string tmpFriendName = (string)getObject ("FRIEND_NAME");

		int tmpFriendTid = (int)getObject ("FRIEND_TID");
		GlobalModule.DoCoroutine(__DoAction(tmpFriendId, tmpFriendLevel, tmpFriendName, tmpFriendTid));

        return true;
    }

    private IEnumerator __DoAction(string friendId, int friendLevel, string friendName, int friendTid)
    {
        DataCenter.Self.set("FAIRYLAND_CURRENT_VISIT_TARGET_LEVEL", friendLevel);
		DataCenter.Self.set("VISIT_FRIEND_NAME", friendName);
		DataCenter.Self.set("VISIT_FRIEND_TID", friendTid);
        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(friendId));
        DataCenter.CloseWindow("FAIRYLAND_FRIEND_SEL_WINDOW");
    }
}

/// <summary>
/// 确认
/// </summary>
class Button_explore_friend_affirm_button : CEvent
{
    public override bool _DoEvent()
    {
        // By XiaoWen
        // Bug #13543【寻仙】同时点击仙境+号和拜访好友按钮时会造成图中问题
        // Begin
        DataCenter.SetData("FAIRYLAND_FRIEND_SEL_WINDOW", "isOpen", false);
        // End
        DataCenter.CloseWindow("FAIRYLAND_FRIEND_SEL_WINDOW");    

        return true;
    }
}
