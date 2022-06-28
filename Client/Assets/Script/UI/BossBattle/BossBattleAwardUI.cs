using System;
using Logic;
using DataTable;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天魔功勋奖励窗口
/// </summary>
public class BossBattleAwardWindow : tWindow
{
    public long currMerit;
    public static List<int> getAwardList;
    public static Dictionary<int, DataRecord> meritAwardConfig;
    static Dictionary<int, DataRecord> meritAwardByLevel;
    static int playerLevel = -1;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_adjust_button", new DefineFactoryLog<Button_adjust_button>());
        EventCenter.Self.RegisterEvent("Button_boss_reward_close_button", new DefineFactoryLog<Button_boss_reward_close_button>());
        EventCenter.Self.RegisterEvent("Button_boss_reward_window", new DefineFactoryLog<Button_boss_reward_close_button>());

        //清除数据
        getAwardList = null;

        if (meritAwardConfig == null)
        {
            meritAwardConfig = new Dictionary<int, DataRecord>();
            Dictionary<int, DataRecord> meritAwardRecords = DataCenter.mFeatAwardConfig.GetAllRecord();

            foreach (KeyValuePair<int, DataRecord> pair in meritAwardRecords)
            {
                DataRecord meritRecrod = pair.Value;
                int awardType = 0;
                meritRecrod.get("AWARD_TYPE", out awardType);
                if (awardType != 1)
                    continue;
                meritAwardConfig.Add(pair.Key, pair.Value);
            }
        }
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_LIST":
                {
                    SC_Boss_GetMeritAwardList retAwardList = objVal as SC_Boss_GetMeritAwardList;
                    getAwardList = retAwardList.arr;
                    __RefreshListData();
                }break;
            case "RECEIVE_MERIT_AWARD":
                {
                    int awardIndex = (int)objVal;
                    ___AcceptAward(awardIndex);
                }break;
            case "RESTORE_GET_AWARD_LIST":
                {
                    SC_Boss_GetMeritAwardList retAwardList = objVal as SC_Boss_GetMeritAwardList;
                    getAwardList = retAwardList.arr;
                }break;
        }
    }

    public override bool Refresh(object param)
    {
        __RefreshRoleData();
        BossBattleNetManager.RequestGetMeritAwardList();

        return true;
    }

    /// <summary>
    /// 刷新个人数据
    /// </summary>
    private void __RefreshRoleData()
    {
        //当前功勋
        currMerit = BossRaidWindow.GetCurrentMerit();
        GameCommon.SetUIText(GetSub("cur_feats_label"), "feats_number", currMerit.ToString());
    }

    /// <summary>
    /// 刷新功勋列表
    /// </summary>
    private void __RefreshListData()
    {
        if (meritAwardConfig == null)
            return;

        UIGridContainer meritGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "reward_group");
        if (meritGrid == null || meritGrid.transform.parent == null)
            return;
        UIScrollView tmpSVMerit = meritGrid.transform.parent.GetComponent<UIScrollView>();
        if (tmpSVMerit != null)
            tmpSVMerit.ResetPosition();

        if (meritAwardByLevel == null)
            meritAwardByLevel = new Dictionary<int, DataRecord>();

        int playerLevelNow = RoleLogicData.Self.character.level;
        if (playerLevel != playerLevelNow)
        {
            playerLevel = playerLevelNow;
            meritAwardByLevel.Clear();
            foreach (KeyValuePair<int, DataRecord> pair in meritAwardConfig)
            {
                DataRecord meritRecrod = pair.Value;
                int levelMin = 0;
                int levelMax = 0;
                meritRecrod.get("LEVEL_MIN", out levelMin);
                meritRecrod.get("LEVEL_MAX", out levelMax);

                if (playerLevel >= levelMin && playerLevel <= levelMax)
                {
                    meritAwardByLevel.Add(pair.Key, pair.Value);
                }
            }
        }

        int count = meritAwardByLevel.Count;
        meritGrid.MaxCount = count;
        int index = 0;
        foreach (KeyValuePair<int, DataRecord> pair in meritAwardByLevel)
        {
            if (index >= count)
                break;

            DataRecord meritRecrod = pair.Value;
            int meritNeed = 99999999;        //默认不能领取
            meritRecrod.get("FEAT_NEED", out meritNeed);
            
            //功勋不足
            GameObject item = meritGrid.controlList[index++];

            int tmpMeritAwardId = (int)meritRecrod.getObject("AWARD_ID");

            //Icon
            string tmpItemIconSprite, tmpItemIconAtlas;
            GameCommon.GetItemAtlasSpriteName(tmpMeritAwardId, out tmpItemIconAtlas, out tmpItemIconSprite);
            GameCommon.SetIcon(item, "item_icon", tmpItemIconSprite, tmpItemIconAtlas);

            //奖励个数
            GameCommon.SetUIText(item, "relate_number_label", meritRecrod.getData("AWARD_NUM"));

            //功勋名
            GameCommon.SetUIText(item, "item_name", GameCommon.GetItemName(tmpMeritAwardId));

            //需要功勋值
            GameCommon.SetUIText(item, "feats_number", meritNeed.ToString());

            if (currMerit < meritNeed)
            {
                GameCommon.SetUIVisiable(item, "get_sprite", false);

                UIImageButton btnAccept = GameCommon.FindComponent<UIImageButton>(item, "adjust_button");
                if (btnAccept != null)
                {
                    btnAccept.gameObject.SetActive(true);
                    btnAccept.isEnabled = false;
                }
            }
            else
            {
                bool bIsAwardAccepted = __IsAwardAccepted(pair.Key);

                GameCommon.SetUIVisiable(item, "get_sprite", bIsAwardAccepted);

                UIImageButton btnAccept = GameCommon.FindComponent<UIImageButton>(item, "adjust_button");
                if (btnAccept != null)
                {
                    btnAccept.gameObject.SetActive(!bIsAwardAccepted);
                    btnAccept.isEnabled = !bIsAwardAccepted;
                }
            }

            NiceData buttonData = GameCommon.GetButtonData(item, "adjust_button");
            if (buttonData != null)
            {
                buttonData.set("AWARD_INDEX", pair.Key);
                buttonData.set("AWARD_ID", tmpMeritAwardId);
            }
        }
    }

    private static bool __IsAwardAccepted(int awardIndex)
    {
        if (getAwardList == null)
            return false;

        foreach (int index in getAwardList)
        {
            if (index == awardIndex)
                return true;
        }
        return false;
    }
    private void ___AcceptAward(int awardIndex)
    {
        UIGridContainer meritGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "reward_group");
        if (meritGrid == null)
            return;

        int index = 0;
        foreach (KeyValuePair<int, DataRecord> pair in meritAwardByLevel)
        {
            GameObject item = meritGrid.controlList[index++];

            if (pair.Key == awardIndex)
            {
                GameCommon.SetUIVisiable(item, "get_sprite", true);

                UIImageButton btnAccept = GameCommon.FindComponent<UIImageButton>(item, "adjust_button");
                if (btnAccept != null)
                {
                    btnAccept.gameObject.SetActive(false);
                    btnAccept.enabled = false;
                }
                if (getAwardList.IndexOf(awardIndex) == -1)
                    getAwardList.Add(awardIndex);

                return;
            }
            if (index >= meritGrid.controlList.Count) return;
        }
    }

    /// <summary>
    /// 是否可以检查有效功勋奖励
    /// </summary>
    /// <returns></returns>
    public static bool CanCheckValidMeritAward()
    {
        return (getAwardList != null);
    }
    /// <summary>
    /// 获取是否有效的功勋奖励
    /// </summary>
    /// <returns></returns>
    public static bool HasValidMeritAward()
    {
        int count = meritAwardConfig.Count;
        int index = 0;
        foreach (KeyValuePair<int, DataRecord> pair in meritAwardConfig)
        {
            if (index >= count)
                break;

            DataRecord meritRecrod = pair.Value;
            int meritNeed = 99999999;        //默认不能领取
            meritRecrod.get("FEAT_NEED", out meritNeed);
            long tmpCurrMerit = BossRaidWindow.GetCurrentMerit();

            if (tmpCurrMerit >= meritNeed && !__IsAwardAccepted(pair.Key))
                return true;
        }
        return false;
    }
}

class Button_adjust_button : CEvent
{
    public override bool _DoEvent()
    {
        int awardIndex = 0;
        get("AWARD_INDEX", out awardIndex);
        int awardID = 0;
        get("AWARD_ID", out awardID);

        // 背包判断
        PACKAGE_TYPE type = PackageManager.GetPackageTypeByItemTid(awardID);
        List<PACKAGE_TYPE> packageTypes = new List<PACKAGE_TYPE>();
        packageTypes.Add(type);
        if (!CheckPackage.Instance.CanAddItems(packageTypes))
            return false;

        BossBattleNetManager.RequestReceiveMeritAward(awardIndex);

        return true;
    }
}

class Button_boss_reward_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("BOSS_AWARD_WINDOW");

        return true;
    }
}
