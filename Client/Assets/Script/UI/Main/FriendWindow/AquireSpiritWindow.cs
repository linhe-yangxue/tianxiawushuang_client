using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Logic;

public class AquireSpiritWindow : tWindow {

    private Dictionary<string, FriendData> mDic = new Dictionary<string, FriendData>();
    public static string str_attched_id = "attached_id";
    public static string str_spirit_list = "SPIRIT_LIST";
    public static string str_spirit_left_count = "SPIRIT_LEFT_COUNT";
    public static string str_validate_spirit_list = "validata_spirit_list";

    public AquireSpiritWindow(GameObject objParent)
    {
        mGameObjUI = GameCommon.FindObject(objParent, "aquire_spirit_window");
    }

    public override void Init()
    {
        // 一键领取按钮
        EventCenter.Self.RegisterEvent("Button_aquire_all_button", new DefineFactory<Button_aquire_all_spirit_button>());
        // 精力领取按钮
        EventCenter.Self.RegisterEvent("Button_spirit_aquire_button", new DefineFactory<Button_spirit_aquire_button>());
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        
        //EventCenter.Self.RegisterEvent("", )
    }

    public override void Open(object param)
    {
        base.Open(param);

        // 每次打开重新获取精力赠送列表
        FriendNetEvent.RequestSendMeSpiritList(OnRequestSpiritSendMeSuccess, (string text) => {
            Debug.LogError("requestSpiritSendMeMessage failed -- " + text);
        });
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if (keyIndex == "REFRESH")
        {
            Refresh(objVal);
        }
        else if (keyIndex == "LIGHT")
        {
            GameObject objButton = GameCommon.FindObject(mGameObjUI.transform.parent.gameObject, "aquire_spirit_button");
            GameCommon.SetUIVisiable(objButton, "new_request_friend_icon", (bool)objVal);
        }
        else if (keyIndex == "AQUIRE_SPIRIT_SUCCESS")
        {
            SC_RequestAquireSpirit sc_data = objVal as SC_RequestAquireSpirit;
            if (sc_data == null) return;
            string[] recvList = sc_data.idRcv;
            List<string> spiritList = DataCenter.Get(AquireSpiritWindow.str_spirit_list).mObj as List<string>;
            List<string> validSpiritList = DataCenter.Get(AquireSpiritWindow.str_validate_spirit_list).mObj as List<string>;
            for (int i = 0; i < recvList.Length; ++i)
            {
                spiritList.Remove(recvList[i]);
                validSpiritList.Remove(recvList[i]);
            }
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.SUCCESS_AQUIRE_SPIRIT);

            // 更新数据
            int leftCount = DataCenter.Get(str_spirit_left_count);
            leftCount -= recvList.Length;
            if (leftCount < 0) leftCount = 0;
            DataCenter.Set(str_spirit_left_count, leftCount);
            DataCenter.SetData("AQUIRE_SPIRIT_WINDOW", "REFRESH", spiritList);

            // 添加精力
            int spiritC = DataCenter.mSpiritSendConfig.GetData("1", "EVERY_TIME_NUM");
            RoleLogicData.Self.AddSpirit(spiritC * recvList.Length);
        }
    }

    public override bool Refresh(object val)
    {
        List<string> list = DataCenter.Get(AquireSpiritWindow.str_spirit_list).mObj as List<string>;
        if (list == null) return false;

        int leftCount = DataCenter.Get(AquireSpiritWindow.str_spirit_left_count);
        if (leftCount == null) leftCount = 0;
        GameCommon.SetUIText(mGameObjUI, "amount", leftCount.ToString());

        // 过滤
        mDic.Clear();
        string[] validIDs = SpiritFilter(list);
        int gridLen = validIDs.Length;

        DataCenter.SetData("AQUIRE_SPIRIT_WINDOW", "LIGHT", gridLen > 0 ? true : false);

        UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "aquire_spirit_scrollview_context_grid").GetComponent<UIGridContainer>();

        grid.MaxCount = gridLen;
        if (gridLen == 0)
        {
            GameCommon.SetUIText(mGameObjUI, "label_no_friend_tips", DataCenter.GetDescByStringIndex(STRING_INDEX.ERROR_NO_SPIRIT_AQUIRE_LABEL));
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FRIEND_SPIRIT, false);
            return true;
        }
        GameCommon.SetUIText(mGameObjUI, "label_no_friend_tips", "");

        for (int i = 0; i < gridLen; i++)
        {
            string fID = validIDs[i];
            GameObject subcell = grid.controlList[i];
            FriendData friendInfo = mDic[fID];
            // 上次登录时间
            long iOffTime = CommonParam.NowServerTime() - friendInfo.lastLoginTime;
            GameCommon.SetUIText(subcell, "off_time_label", GetLastLoginDesc(iOffTime));
            // 公会
            GameCommon.SetUIText(subcell, "gonghui_content", friendInfo.guildName);
            // 战斗力
            GameCommon.SetUIText(subcell, "zhandouli_content", friendInfo.power.ToString());
            // 名字
            GameCommon.SetUIText(subcell, "player_name_label", friendInfo.name);
            // 头像
            GameCommon.SetRoleIcon(GameCommon.FindObject(subcell, "pet_icon").GetComponent<UISprite>(), friendInfo.icon, GameCommon.ROLE_ICON_TYPE.PHOTO);
            // 等级
            GameCommon.SetUIText(subcell, "level_label", "Lv " + friendInfo.level);
            // set button data
            NiceData niceData = GameCommon.GetButtonData(GameCommon.FindObject(subcell, "spirit_aquire_button"));
            niceData.set(str_attched_id, friendInfo.friendId);
        }
        return true;
    }

    private void OnRequestSpiritSendMeSuccess(string text)
    {
        SC_RequestSendMeSpiritList list = new SC_RequestSendMeSpiritList();
        list = JCode.Decode<SC_RequestSendMeSpiritList>(text);

        if (list == null) return;

        List<string> listData = new List<string>();
        for (int i = 0; i < list.arr.Length; ++i)
        {
            listData.Add(list.arr[i]);
        }
        DataCenter.Set(str_spirit_list, listData);
        DataCenter.Set(str_spirit_left_count, list.leftCnt);

        DataCenter.SetData("AQUIRE_SPIRIT_WINDOW", "LIGHT", list.arr.Length > 0 ? true : false);
        Refresh(listData);
    }

    private string GetLastLoginDesc(long offlineTime)
    {
        if (offlineTime < 60)
            return "0分钟前";
        if (offlineTime < 3600 && offlineTime >= 60)
            return (offlineTime / 60).ToString() + "分钟前";
        if (offlineTime < 3600 * 24 && offlineTime >= 3600)
            return (offlineTime / 3600).ToString() + "小时前";
        if (offlineTime >= 3600 * 24)
            return (offlineTime / (3600 * 24)).ToString() + "天前";
        return "";
    }

    // 精力过滤 。如果不在好友列表就过滤掉
    // 返回 按照最后登录时间排序的数组
    private string[] SpiritFilter(List<string> data)
    {
        List<FriendData> list = new List<FriendData>();
        FriendLogicData friendLogicData = DataCenter.GetData("GAME_FRIEND_LIST") as FriendLogicData;
        if (friendLogicData == null)
        {
            return new string[0];
        }
        FriendData[] friendData = friendLogicData.arr;
        for (int i = 0; i < data.Count; ++i)
        {
            string fID = data[i];
            for (int j = 0; j < friendData.Length; ++j)
            {
                FriendData d = friendData[j];
                if (d.friendId == fID)
                {
                    list.Add(d);
                    mDic.Add(fID, d);
                    break;
                }
            }
        }
        list.Sort(new SpiritSortRule());
        List<string> fIDList = new List<string>();
        list.ForEach((FriendData fData) =>
        {
            fIDList.Add(fData.friendId);
        });

        // store list
        DataCenter.Set(str_validate_spirit_list, fIDList);

        return fIDList.ToArray();
    }
}

public class SpiritSortRule : IComparer<FriendData>
{
    public int Compare(FriendData left, FriendData right)
    {
        if (left == null || right == null)
            return 0;
        return left.lastLoginTime - right.lastLoginTime > 0 ? -1 : 1;
    }
}


public class Button_aquire_all_spirit_button : CEvent
{
    public override bool _DoEvent()
    {
        int leftCount = DataCenter.Get(AquireSpiritWindow.str_spirit_left_count);
        if (leftCount == 0)
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_ZERO_SPIRIT_COUNT);
            return false;
        }

        List<string> list = DataCenter.Get(AquireSpiritWindow.str_validate_spirit_list).mObj as List<string>;
        if (list.Count == 0)
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_NO_SPIRIT_AQUIRE);
            return false;
        }

        int sendCount = leftCount > list.Count ? list.Count : leftCount;
        List<string> newList = new List<string>();
        for (int i = 0; i < sendCount; ++i)
        {
            newList.Add(list[i]);
        }

        FriendNetEvent.RequestAquireSpirit(newList.ToArray(), (string text) =>
        {
            SC_RequestAquireSpirit recvList = JCode.Decode<SC_RequestAquireSpirit>(text);
            DataCenter.SetData("AQUIRE_SPIRIT_WINDOW", "AQUIRE_SPIRIT_SUCCESS", recvList);

        }, (string text) =>
        {
            Debug.LogError("aquire_spirit -- " + text);
        });

        return true;
    }
}

public class Button_spirit_aquire_button : CEvent
{
    public override bool _DoEvent()
    {
        int leftCount = DataCenter.Get(AquireSpiritWindow.str_spirit_left_count);
        if (leftCount == 0)
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_ZERO_SPIRIT_COUNT);
            return false;
        }

        GameObject obj = get("BUTTON").mObj as GameObject;
        NiceData nData = GameCommon.GetButtonData(obj);
        string[] fID = {nData.get(AquireSpiritWindow.str_attched_id)};
        FriendNetEvent.RequestAquireSpirit(fID, (string text) =>
        {
            SC_RequestAquireSpirit recvList = JCode.Decode<SC_RequestAquireSpirit>(text);
            DataCenter.SetData("AQUIRE_SPIRIT_WINDOW", "AQUIRE_SPIRIT_SUCCESS", recvList);

        }, (string text) =>
        {
            Debug.LogError("aquire_spirit -- " + text);
        });
        return true;
    }
}