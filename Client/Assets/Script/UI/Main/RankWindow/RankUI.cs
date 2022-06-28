using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Logic;
using Utilities;


public class RankUI : MonoBehaviour
{
    private void Start()
    {

        tWindow win = new RankWindow() { mGameObjUI = gameObject, mWinName = "RANK_WINDOW" };
        DataCenter.Self.registerData("RANK_WINDOW", win);
        DataCenter.OpenWindow("RANK_WINDOW");
    }

    private void OnDisable()
    {
        DataCenter.CloseWindow("RANK_WINDOW");
    }

    private void OnDestroy()
    {
        if (this.gameObject.activeInHierarchy)
            DataCenter.CloseWindow("RANK_WINDOW");
    }
}


public enum RankType
{
    None,
    Arena,
    Element,
    RoleFight,
    UnionBattle,
    UnionFight,
}


public class RankData
{
    public string mName = "";
    public int mScore = 0;
    public string mPlayerID = "";
    public int mVipLevel = 0;
}


public class RankList
{
    public int mResetTime = 0;
    public List<RankData> mList = new List<RankData>();
}


public class RankWindow : tWindow
{
    public static readonly int MAX_COUNT = 50;

    public RankType currentRank = RankType.None;

    private string lastDeployItemName = "";

    public override void OnOpen()
    {
        lastDeployItemName = "";
        GameObject classList = GetSub("class_list");
        classList.GetComponent<DeployGrid>().onDeploy = OnDeploy;
        currentRank = RankType.None;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch(keyIndex)
        {
            case "REQUEST_RANK":
                RankType rankType = (RankType)objVal;

                if (currentRank != rankType)
                {
                    RequestRank(rankType);
                    RequestMyRank(rankType);
                }
                
                break; 

            case "REFRESH_ARENA_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshRank(RankType.Arena, data);
                }
                break;

            case "REFRESH_MY_ARENA_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshMyRank(RankType.Arena, data);
                }
                break;

            case "REFRESH_ELEMENT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshRank(RankType.Element, data);
                }
                break;

            case "REFRESH_MY_ELEMENT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshMyRank(RankType.Element, data);
                }
                break;

            case "REFRESH_ROLE_FIGHT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshRank(RankType.RoleFight, data);
                }
                break;

            case "REFRESH_MY_ROLE_FIGHT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshMyRank(RankType.RoleFight, data);
                }
                break;

            case "REFRESH_UNION_BATTLE_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshRank(RankType.UnionBattle, data);
                }
                break;

            case "REFRESH_MY_UNION_BATTLE_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshMyRank(RankType.UnionBattle, data);
                }
                break;

            case "REFRESH_UNION_FIGHT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshRank(RankType.UnionFight, data);
                }
                break;

            case "REFRESH_MY_UNION_FIGHT_RANK":
                {
                    tNiceData data = objVal as tNiceData;

                    if (data != null)
                        RefreshMyRank(RankType.UnionFight, data);
                }
                break;
        }
    }

    //private void RequestRank(RankType rankType)
    //{
    //    if (currentRank == rankType)
    //        return;
    //
    //    _RequestRank(rankType);
    //}

    private void RequestRank(RankType rankType)
    {
        switch (rankType)
        {
            case RankType.Arena:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVPRank");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;

            case RankType.Element:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVPRank");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.set("IS_ATTRIBUTE_WAR", true);
                    evt.DoEvent();
                }             
                break;

            case RankType.RoleFight:
                {
                    tEvent evt = Net.StartEvent("CS_RequestFightPowerRank");
                    evt.DoEvent();
                }
                break;

            case RankType.UnionBattle:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVPRank");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;

            case RankType.UnionFight:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVPRank");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;
        }

        currentRank = rankType;
    }

    private void RequestMyRank(RankType rankType)
    {
        switch (rankType)
        {
            case RankType.Arena:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;

            case RankType.Element:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
                    evt.set("IS_ATTRIBUTE_WAR", true);
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;

            case RankType.RoleFight:
                {
                    tEvent evt = Net.StartEvent("CS_RequestMyFightPowerRank");
                    evt.DoEvent();
                }
                break;

            case RankType.UnionBattle:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;

            case RankType.UnionFight:
                {
                    tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
                    evt.set("WINDOW_NAME", "RANK_WINDOW");
                    evt.DoEvent();
                }
                break;
        }
    }

    private UIGridContainer GetContainer()
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, "ranks", "grid");
        UIGridContainer container = obj.GetComponent<UIGridContainer>();
        return container;
    }

    private RankList GetRankList(RankType type, tNiceData data)
    {
        DataBuffer buffer = data.getObject("RANK_DATA") as DataBuffer;

        if (buffer == null)
        {
            DEBUG.LogError("get rank data fail");
            return null;
        }

        buffer.seek(0);
        int count = 0;

        if (!buffer.read(out count))
            return null;

        RankList rankList = new RankList();
        rankList.mResetTime = data.get("PVP_RANK_RESET_TIME");

        for (int i = 0; i < count; ++i)
        {
            string name = "";
            if (!buffer.readOne(out name))
                break;

            int score = 0;
            if (!buffer.read(out score))
                break;

            string playerID = "";
            if (!buffer.readOne(out playerID))
                break;

            int vipLevel = 0;
            if (!buffer.read(out vipLevel))
                break;

            RankData rankData = new RankData();
            rankData.mName = name;
            rankData.mScore = score;
            rankData.mPlayerID = playerID;
            rankData.mVipLevel = vipLevel;
            rankList.mList.Add(rankData);
        }

//        rankList.mList = rankList.mList.OrderByDescending(d => d.mScore).ToList();
		rankList.mList = GameCommon.SortList (rankList.mList , SortList);

        if (rankList.mList.Count > MAX_COUNT)
            rankList.mList = rankList.mList.GetRange(0, MAX_COUNT);

        return rankList;
    }

	private int SortList(RankData a, RankData b)
	{
		return GameCommon.Sort (a.mScore, b.mScore, true);
	}

    private void RefreshMyRank(RankType type, tNiceData data)
    {
        SetText("my_score", data.get("SCORE"));
        SetText("my_rank", data.get("RANKING"));

        if (type == RankType.RoleFight)
        {
            SetText("my_score_tiitle", "战斗力");
        }
        else
        {
            SetText("my_score_tiitle", "当前积分");
        }
    }

    private void RefreshRank(RankType type, tNiceData data)
    {
        RankList rankList = GetRankList(type, data);

        if (rankList != null)
        {
            RefreshRank(type, rankList);
        }
    }

    private void RefreshRank(RankType type, RankList rankList)
    {
        SetTime(rankList.mResetTime);

        UIGridContainer container = GetContainer();
        container.SetMaxCountAsync(rankList.mList.Count, 1, i => RefreshItem(container.controlList[i], type, i, rankList.mList[i]), null);
        //container.MaxCount = MAX_COUNT;
        //
        //for (int i = 0; i < rankList.mList.Count; ++i)
        //{
        //    GameObject item = container.controlList[i];
        //    item.SetActive(true);
        //    RefreshItem(item, type, i, rankList.mList[i]);
        //}
        //for (int i = rankList.mList.Count; i < MAX_COUNT; ++i)
        //{
        //    container.controlList[i].SetActive(false);
        //}
        //
        container.Reposition();
        UIScrollView view = GameCommon.FindObject(mGameObjUI, "ranks", "scrollview").GetComponent<UIScrollView>();
        view.ResetPosition();     
    }

    private void RefreshItem(GameObject item, RankType type, int index, RankData data)
    {
        GameCommon.SetUIText(item, "rank_label", (index + 1).ToString());
        GameCommon.SetUIText(item, "name_label", data.mName);
        GameCommon.SetUIText(item, "pvp_score_label", data.mScore.ToString());
        int vipLevel = data.mVipLevel;

        if (vipLevel == 0)
        {
            GameCommon.SetUIVisiable(item, "vip", false);
        }
        else
        {
            GameCommon.SetUIVisiable(item, "vip", true);
            GameCommon.SetUIText(item, "vip", "VIP " + vipLevel);
        }

        if (type == RankType.RoleFight)
        {
            GameCommon.SetUIText(item, "current_credit_text_lable", "战斗力:");
        }
        else
        {
            GameCommon.SetUIText(item, "current_credit_text_lable", "当前积分:");
        }

        SetVisitButtonData(item, data.mPlayerID, data.mName);
        SetFlag(item, index);
    }

    //private void RefreshPVPRank(tNiceData data)
    //{
    //    SetTime(data.get("PVP_RANK_RESET_TIME"));
    //           
    //    UIGridContainer container = GetContainer();
    //
    //    DataBuffer randData = data.getObject("RANK_DATA") as DataBuffer;
    //
    //    if (randData == null)
    //    {
    //        DEBUG.LogError("get rank data fail");
    //        return;
    //    }
    //
    //    randData.seek(0);
    //
    //    int count = 0;
    //    if (!randData.read(out count))
    //        return;
    //
    //    List<PvpRankData> rankList = new List<PvpRankData>();
    //
    //    for (int i = 0; i < count; ++i)
    //    {
    //        string name = "";
    //        if (!randData.readTwo(out name))
    //            break;
    //
    //        int pvpScore = 0;
    //        if (!randData.read(out pvpScore))
    //            break;
    //
    //        int playerID = 0;
    //        if (!randData.read(out playerID))
    //            break;
    //
    //        int vipLevel = 0;
    //        if (!randData.read(out vipLevel))
    //            break;
    //
    //        PvpRankData rankData = new PvpRankData();
    //        rankData.mName = name;
    //        rankData.mScore = pvpScore;
    //        rankData.mPlayerID = playerID;
    //        rankData.mVipLevel = vipLevel;
    //        rankList.Add(rankData);
    //    }
    //
    //    rankList = rankList.OrderByDescending(d => d.mScore).ToList();
    //    
    //    if(rankList.Count > 50)
    //        rankList = rankList.GetRange(0, 50);
    //
    //    int num = rankList.Count;
    //    container.MaxCount = num;
    //    for (int j = 0; j < num; j++)
    //    {
    //        GameObject item = container.controlList[j];
    //        GameCommon.SetUIText(item, "rank_label", (j + 1).ToString());
    //        GameCommon.SetUIText(item, "name_label", rankList[j].mName);
    //        GameCommon.SetUIText(item, "pvp_score_label", rankList[j].mScore.ToString());
    //        int vipLevel = rankList[j].mVipLevel;
    //		if(vipLevel == 0)
    //		{
    //			GameCommon.SetUIVisiable (item, "vip", false);
    //		}else
    //		{
    //			GameCommon.SetUIVisiable (item, "vip", true);
    //			GameCommon.SetUIText(item, "vip","VIP " + vipLevel);
    //		}
    //  //          GameCommon.SetUIText(item, "vip", vipLevel == 0 ? "" : "VIP " + vipLevel);
    //
    //        SetFlag(item, j);
    //        SetVisitButtonData(item, rankList[j].mPlayerID, rankList[j].mName);
    //    }
    //
    //    UIScrollView view = GameCommon.FindObject(mGameObjUI, "ranks", "scrollview").GetComponent<UIScrollView>();
    //    view.ResetPosition();
    //    return;
    //}

    public void RefreshData(object obj)
    {
        RequestRank(currentRank);
    }

    private void SetTime(Int64 time)
    {
        GameObject countdown = GetSub("rank_reset_time");

        if (time == 0)
        {
            countdown.SetActive(false);
        }
        else
        {
            countdown.SetActive(true);
            CountdownUI countdownUI = countdown.GetComponent<CountdownUI>();

            if (countdownUI != null)
                MonoBehaviour.DestroyImmediate(countdownUI);

            if (time <= CommonParam.NowServerTime())
                time = CommonParam.NowServerTime();

            SetCountdownTime("rank_reset_time", time + 5, new CallBack(this, "RefreshData", null));
        }
    }

    private void SetFlag(GameObject item, int index)
    {
        GameObject flag = GameCommon.FindObject(item, "flag");

        if (index < 3)
        {
            flag.SetActive(true);

            switch (index)
            {
                case 0:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_111";
                    break;
                case 1:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_222";
                    break;
                case 2:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_333";
                    break;
            }
        }
        else
        {
            flag.SetActive(false);
        }
    }

    //private void RefreshArenaRank(tNiceData data)
    //{
    //    RequestMyRank(RankType.Arena);
    //    RefreshPVPRank(data);
    //}
    //
    //private void RefreshElementRank(tNiceData data)
    //{
    //    RequestMyRank(RankType.Element);
    //    RefreshPVPRank(data);
    //}
    //
    //private void RefreshRoleFightRank(tNiceData data)
    //{
    //    RequestMyRank(RankType.Arena);
    //    RefreshPVPRank(data);
    //}
    //
    //private void RefreshUnionBattleRank(tNiceData data)
    //{
    //    RequestMyRank(RankType.Arena);
    //    RefreshPVPRank(data);
    //}
    //
    //private void RefreshUnionFightRank(tNiceData data)
    //{
    //    RequestMyRank(RankType.Arena);
    //    RefreshPVPRank(data);
    //}

    public virtual void SetVisitButtonData(GameObject obj, string playerID, string name)
    {
        NiceData visitButtonData = GameCommon.GetButtonData(obj, "visit_button");
        visitButtonData.set("FRIEND_ID", playerID);
        visitButtonData.set("FRIEND_NAME", name);
        visitButtonData.set("WINDOW_NAME", "RANK_WINDOW");
    }

    private void OnDeploy(GameObject item, GameObject deployed)
    {
        if (lastDeployItemName == item.name)
            return;

        lastDeployItemName = item.name;

        GameObject classList = GetSub("class_list");

        switch (item.name)
        {
            case "arena_class":
                GameCommon.ToggleTrue(GameCommon.FindObject(classList, "arena_rank_btn"));
                EventCenter.Start("Button_arena_rank_btn").DoEvent();
                break;

            case "role_class":
                GameCommon.ToggleTrue(GameCommon.FindObject(classList, "role_fight_rank_btn"));
                EventCenter.Start("Button_role_fight_rank_btn").DoEvent();
                break;

            case "pet_class":
                GameCommon.ToggleTrue(GameCommon.FindObject(classList, "union_fight_rank_btn"));
                EventCenter.Start("Button_union_fight_rank_btn").DoEvent();
                break;
        }
    }
}


public class Button_rank_window_back : CEvent
{
    public override bool _DoEvent()
    {
        if (MainUIScript.Self.mWindowBackAction != null)
        {
            MainUIScript.Self.mWindowBackAction();
            MainUIScript.Self.mWindowBackAction = null;
        }
        else
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        }
        return true;
    }
}

public class Button_arena_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RANK_WINDOW", "REQUEST_RANK", RankType.Arena);
        return true;
    }
}

public class Button_element_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RANK_WINDOW", "REQUEST_RANK", RankType.Element);
        return true;
    }
}

public class Button_role_fight_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RANK_WINDOW", "REQUEST_RANK", RankType.RoleFight);
        return true;
    }
}

public class Button_union_battle_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RANK_WINDOW", "REQUEST_RANK", RankType.UnionBattle);
        return true;
    }
}

public class Button_union_fight_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RANK_WINDOW", "REQUEST_RANK", RankType.UnionFight);
        return true;
    }
}