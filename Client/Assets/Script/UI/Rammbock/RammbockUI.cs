using System;
using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

/// <summary>
/// 群魔乱舞界面
/// </summary>

/// <summary>
/// 属性加成Buff数据
/// </summary>
public class RammbockAttriBuffData
{
    public AFFECT_TYPE m_affectType = AFFECT_TYPE.NONE;   //效果Buff类型
    public string m_affectName = "";                      //效果Buff名称
    public bool m_isRate = true;                          //是否为百分比
    public float m_affectValue = 0.0f;                    //效果Buff加成值

    public RammbockAttriBuffData()
    {
    }

    public void Init()
    {
        m_affectType = AFFECT_TYPE.NONE;
        m_affectName = "";
        m_isRate = true;
        m_affectValue = 0.0f;
    }

    public string Name
    {
        get
        {
            return m_affectName;
        }
    }
    public string ValueString
    {
        get
        {
            string str = "+";
            if (m_isRate)
            {
                str += (m_affectValue * 100).ToString("0");
                str += "%";
            }
            else
                str += m_affectValue.ToString();
            return str;
        }
    }

    public void AddBuff(int buffID)
    {
        DataRecord affectConfig = DataCenter.mAffectBuffer.GetRecord(buffID);
        if(affectConfig == null)
            return;

        string strAffectType = affectConfig.getData("AFFECT_TYPE");
        AFFECT_TYPE tmpAffectType = GameCommon.ToAffectTypeEnum(strAffectType);
        if (tmpAffectType != m_affectType)
        {
            Init();
            m_affectType = tmpAffectType;
            //DataRecord equipAttri = DataCenter.mEquipAttributeIconConfig.GetRecord((int)tmpAffectType);
            //if (equipAttri != null)
			m_affectName = affectConfig.getData("NAME");
//                m_affectName = equipAttri.getData("NAME");
            m_isRate = (strAffectType.IndexOf("RATE") != -1);
        }
        float tmpAffectValue = affectConfig.getData("AFFECT_VALUE");
        tmpAffectValue /= 10000;
        m_affectValue += tmpAffectValue;
    }

    public static AFFECT_TYPE GetBuffType(int buffID)
    {
        DataRecord affectConfig = DataCenter.mAffectBuffer.GetRecord(buffID);
        if (affectConfig == null)
            return AFFECT_TYPE.NONE;

        string strAffectType = affectConfig.getData("AFFECT_TYPE");
        AFFECT_TYPE tmpAffectType = GameCommon.ToAffectTypeEnum(strAffectType);
        return tmpAffectType;
    }
}
/// <summary>
/// 管理Buff数据
/// </summary>
public class RammbockAttriBuffManager
{
    private List<RammbockAttriBuffData> m_listBuffData = new List<RammbockAttriBuffData>();     //Buff数据列表

    public RammbockAttriBuffManager()
    {
    }

    public void Init()
    {
        if (m_listBuffData != null)
            m_listBuffData.RemoveRange(0, m_listBuffData.Count);
    }

    public void AddBuff(int buffID)
    {
        AFFECT_TYPE affectType = RammbockAttriBuffData.GetBuffType(buffID);
        RammbockAttriBuffData buffData = GetBuffDataByType(affectType);
        if (buffData == null)
        {
            buffData = new RammbockAttriBuffData();
            m_listBuffData.Add(buffData);
        }
        buffData.AddBuff(buffID);
    }
    public RammbockAttriBuffData GetBuffDataByType(AFFECT_TYPE affectType)
    {
        if (m_listBuffData == null || m_listBuffData.Count <= 0)
            return null;

        RammbockAttriBuffData buffData = m_listBuffData.Find((RammbockAttriBuffData tmpBuffdata) => {
            return (tmpBuffdata.m_affectType == affectType);
        });
        return buffData;
    }


	// 合并同类型的BUFF效果
	public  List<RammbockAttriBuffData> __CombineSameBuffData()
	{
		List<RammbockAttriBuffData> _buffList = new List<RammbockAttriBuffData>();
		for(int i = 0;i < BuffDataCount;i++)
		{
			RammbockAttriBuffData _buffData = GetBuffDataByIndex(i);
			int _count = 0;
			for(int j = 0;j < _buffList.Count;j++)
			{
				if(_buffData.m_affectType == _buffList[j].m_affectType)
				{
					_buffList[j].m_affectValue += _buffData.m_affectValue;
					break;
				}
				_count++;
			}
			if(_count == _buffList.Count)
				_buffList.Add(_buffData);
		}
		return _buffList;
		
	}

    public int BuffDataCount
    {
        get
        {
            if (m_listBuffData == null)
                return 0;
            return m_listBuffData.Count;
        }
    }
    public RammbockAttriBuffData GetBuffDataByIndex(int index)
    {
        if (index < 0 || index >= m_listBuffData.Count)
            return null;
        return m_listBuffData[index];
    }
    /// <summary>
    /// 遍历所有Buff
    /// </summary>
    /// <param name="action">参数：Buff数据在列表里索引值，Buff数据</param>
    public void ForEachBuffData(Action<int, RammbockAttriBuffData> action)
    {
        int i = 0;
        m_listBuffData.ForEach((RammbockAttriBuffData buffData) =>
        {
            action(i++, buffData);
        });
    }
}

/// <summary>
/// 群魔乱舞主界面
/// </summary>
public class RammbockWindow : tWindow
{
    public SC_Rammbock_GetTowerClimbingInfo m_climbingInfo;
    public RammbockAttriBuffManager m_buffDataManager = new RammbockAttriBuffManager();

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_reset_btn", new DefineFactoryLog<Button_reset_btn>());
        EventCenter.Self.RegisterEvent("Button_one_key_three_star_btn", new DefineFactoryLog<Button_one_key_three_star_btn>());
        EventCenter.Self.RegisterEvent("Button_rammbock_window_back_btn", new DefineFactoryLog<Button_rammbock_window_back_btn>());
        EventCenter.Self.RegisterEvent("Button_rammbock_renown_btn", new DefineFactory<Button_renown_btn>());
        EventCenter.Self.RegisterEvent("Button_rammbock_rank_btn", new DefineFactoryLog<Button_rammbock_rank_btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

//        DataCenter.OpenWindow("INFO_GROUP_WINDOW");
        DataCenter.OpenWindow("BACK_GROUP_RAMMBOCK_WINDOW");


        RammbockNetManager.RequestGetTowerClimbingInfo();
        //added by xuke 灵核商店红点相关
        RammbockNewMarkManager.Self.RequestTowerShopInfoAndCheckReward(() => { DataCenter.SetData("RAMMBOCK_WINDOW", "REFRESH_NEWMARK", null); });
        //end
    }

    public void SetButtonShow(bool visible)
    {
        if (visible && GameCommon.IsFuncCanShow(mGameObjUI, "one_key_three_star_btn"))
        {
            SetVisible("one_key_three_star_btn", true);
            SetVisible("reset_btn", false);
            SetVisible("need_ingot_free", false);
            SetVisible("need_ingot", false);
        }
        else
        {
            SetVisible("one_key_three_star_btn", false);
            SetVisible("reset_btn", true);
        }
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("BACK_GROUP_RAMMBOCK_WINDOW");
//        DataCenter.CloseWindow("INFO_GROUP_WINDOW");

        //刷新快捷入口界面
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
    }

    public override bool Refresh(object param)
    {
        m_climbingInfo = param as SC_Rammbock_GetTowerClimbingInfo;
        DataRecord nextClimbConfig = DataCenter.mClimbingTowerConfig.GetRecord(m_climbingInfo.nextTier);
        if (nextClimbConfig != null)
        {
            //通关条件
            string missionCondition = GetMissionCondition(nextClimbConfig, nextClimbConfig.getData("STAR_1"));
            GameCommon.SetUIText(GetSub("condition_sprite"), "Label", missionCondition);

            //历史最高
            GameCommon.SetUIText(GetSub("highest_label"), "highest_number", m_climbingInfo.rankStars.ToString());

            //本次挑战
            GameCommon.SetUIText(mGameObjUI, "cur_number", m_climbingInfo.currentStars.ToString());

            //威名
            GameCommon.SetUIText(GetSub("renown"), "renown_num", RoleLogicData.Self.prestige.ToString());

            __RefreshBattleAttribute(nextClimbConfig);

            //按钮-一键3星
            SetButtonShow(m_climbingInfo.tierState == 1 ? true : false);

            //一键挑战的关卡
            NiceData btnThreeData = GameCommon.GetButtonData(mGameObjUI, "one_key_three_star_btn");
            if (btnThreeData != null)
                btnThreeData.set("Climb_Index", m_climbingInfo.nextTier);

            //是否可挑战
            if (m_climbingInfo.tierState == 1)
            {
                //可以挑战
                if (GameCommon.bIsWindowOpen("RAMMBOCK_OVER_WINDOW"))
                    DataCenter.CloseWindow("RAMMBOCK_OVER_WINDOW");
                DataCenter.OpenWindow("RAMMBOCK_CHOICE_WINDOW", m_climbingInfo);
            }
            else
            {
                //不可以挑战，群魔乱舞结束
                if (GameCommon.bIsWindowOpen("RAMMBOCK_CHOICE_WINDOW"))
                    DataCenter.CloseWindow("RAMMBOCK_CHOICE_WINDOW");
                DataCenter.OpenWindow("RAMMBOCK_OVER_WINDOW", m_climbingInfo);
                //added by xuke
                //if (RammbockWindow.IsPassAllMission())
                //{
                RammbockWindow rammbockWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
                RammbockBattleResultData resultData = rammbockWin.getObject("RAMMBOCK_AWARD") as RammbockBattleResultData;
                if (resultData != null)
                {
                    rammbockWin.set("RAMMBOCK_AWARD", null);
                    DataCenter.OpenWindow("RAMMBOCK_CHAPTER_AWARD_WINDOW", null);
                }
                //}
                //end
            }
        }
        else
        {
            if (GameCommon.bIsWindowOpen("RAMMBOCK_CHOICE_WINDOW"))
            {
                DataCenter.CloseWindow("RAMMBOCK_CHOICE_WINDOW");
            }
            else
            {
                RammbockWindow rammbockWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
                RammbockBattleResultData resultData = rammbockWin.getObject("RAMMBOCK_AWARD") as RammbockBattleResultData;
                if (resultData != null)
                {
                    rammbockWin.set("RAMMBOCK_AWARD", null);
                    DataCenter.OpenWindow("RAMMBOCK_CHAPTER_AWARD_WINDOW", null);
                }
            }
            //历史最高
            GameCommon.SetUIText(GetSub("highest_label"), "highest_number", m_climbingInfo.rankStars.ToString());

            //本次挑战
            GameCommon.SetUIText(mGameObjUI, "cur_number", m_climbingInfo.currentStars.ToString());

            //威名
            GameCommon.SetUIText(GetSub("renown"), "renown_num", RoleLogicData.Self.prestige.ToString());
            DataRecord mNextClimbConfig = DataCenter.mClimbingTowerConfig.GetRecord(m_climbingInfo.nextTier - 1);
            __RefreshBattleAttribute(mNextClimbConfig);
            SetButtonShow(false);
            DataCenter.OpenMessageWindow("恭喜少侠通关！");
        }
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "REFRESH_NEWMARK":
                RefreshNewMark();
                break;
            case "JUST_RESTORE_REQUEST_DATA":
                {
                    if (objVal is SC_Rammbock_GetTowerClimbingInfo)
                    {
                        //仅储存爬塔信息
                        m_climbingInfo = objVal as SC_Rammbock_GetTowerClimbingInfo;
                    }
                } break;
            case "UPDATE_RENOWN_NUM": 
                GameCommon.SetUIText(GetSub("renown"), "renown_num", RoleLogicData.Self.prestige.ToString());
                break;
        }
    }
    private void RefreshNewMark()
    {
        if (mGameObjUI == null)
            return;
        GameObject _rammbockShopBtnObj = GameCommon.FindObject(mGameObjUI, "rammbock_renown_btn");
        GameCommon.SetNewMarkVisible(_rammbockShopBtnObj, RammbockNewMarkManager.Self.RammbockShopBtnVisible);
    }
    private void __RefreshBattleAttribute(DataRecord nextClimbConfig)
    {
        //可用星
        GameCommon.SetUIText(GetSub("attribute_label"), "star_number", m_climbingInfo.remainStars.ToString());

        //加成效果
        int buffDataCount = 0;
        if (m_buffDataManager != null)
        {
            m_buffDataManager.Init();
            for (int i = 0, count = m_climbingInfo.buffList.Count; i < count; i++)
                m_buffDataManager.AddBuff(m_climbingInfo.buffList[i]);
            buffDataCount = m_buffDataManager.BuffDataCount;
        }

        List<RammbockAttriBuffData> _buffList = m_buffDataManager.__CombineSameBuffData();
        buffDataCount = _buffList.Count;
        if (buffDataCount == 0)
        {
            GameCommon.SetUIText(mGameObjUI, "Label_no_effect_add_tips", DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_RAMMBOCK_NO_EFFECT_ADD, "STRING_CN"));
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "Label_no_effect_add_tips", "");
        }
        for (int i = 0; i < 9; i++)
        {
            string buffName = "无加成";
            string buffValue = "";
            if (i < buffDataCount)
            {
                //                RammbockAttriBuffData buffData = m_buffDataManager.GetBuffDataByIndex(i);
                RammbockAttriBuffData buffData = _buffList[i];
                buffName = buffData.Name;
                buffValue = buffData.ValueString;
            }
            GameObject subItem = GetSub("group(Clone)_" + i.ToString());
            GameCommon.SetUIVisiable(mGameObjUI, "group(Clone)_" + i.ToString(), i < buffDataCount);
            GameCommon.SetUIText(subItem, "effect_label", buffName);
            GameCommon.SetUIText(subItem, "effect_number", buffValue);

        }

        //重置消耗
        DataRecord resetConfig = DataCenter.mClimbingConsumeConfig.GetRecord(m_climbingInfo.resetTimes + 1);
        UIImageButton btnReset = GameCommon.FindComponent<UIImageButton>(mGameObjUI, "reset_btn");
        int tmpMaxResetCount = (int)VIPHelper.GetCurrVIPValueByField(new List<VIP_CONFIG_FIELD>() { VIP_CONFIG_FIELD.RAMMBOCK_RESET_COUNT })[0];

        GameObject needIngotObj = GameCommon.FindObject(mGameObjUI, "need_ingot");
        GameObject needIngotFreeObj = GameCommon.FindObject(mGameObjUI, "need_ingot_free");
        needIngotObj.SetActive(false);
        needIngotFreeObj.SetActive(false);

        if (resetConfig != null && m_climbingInfo.resetTimes < tmpMaxResetCount)
        {
            //有重置记录
            int nResetCount = 0;
            resetConfig.get("RESET_CONSUME", out nResetCount);
            if (nResetCount != 0)
            {
                needIngotObj.SetActive(true);
                needIngotFreeObj.SetActive(false);
                GameCommon.SetUIText(needIngotObj, "need_ingot_num", "x" + nResetCount.ToString());
            }
            else
            {
                needIngotObj.SetActive(false);
                needIngotFreeObj.SetActive(true);
                needIngotFreeObj.GetComponent<UILabel>().text = "可免费重置1次";
            }
            //            GameCommon.SetUIText(GetSub("need_ingot"), "need_ingot_num", (nResetCount != 0) ? nResetCount.ToString() : "可免费重置1次");
            btnReset.isEnabled = true;

            //重置花费信息
            NiceData btnData = GameCommon.GetButtonData(mGameObjUI, "reset_btn");
            if (btnData != null)
                btnData.set("RESET_COST", nResetCount);
        }
        else
        {
            //没有重置记录，不能重置
            //            GameCommon.SetUIText(GetSub("need_ingot"), "need_ingot_num", "0");
            if (RoleLogicData.Self.vipLevel == 12)
            {
                needIngotObj.SetActive(false);
                needIngotFreeObj.SetActive(true);
                needIngotFreeObj.GetComponent<UILabel>().text = "重置次数已用尽";
                btnReset.isEnabled = false;
            }
            else
            {
                int nResetCount = 0;
                resetConfig.get("RESET_CONSUME", out nResetCount);
                needIngotObj.SetActive(true);
                needIngotFreeObj.SetActive(false);
                GameCommon.SetUIText(needIngotObj, "need_ingot_num", "x" + nResetCount.ToString());
                btnReset.isEnabled = true;
                NiceData btnData = GameCommon.GetButtonData(mGameObjUI, "reset_btn");
                if (btnData != null)
                    btnData.set("RESET_COST", nResetCount);
            }
        }
    }

    public static string GetMissionCondition(DataRecord nextClimbConfig, string stageIndex)
    {
        string condition = "无条件";

        if (nextClimbConfig != null)
        {
            stageIndex = nextClimbConfig.getData("STAR_1");
        }

        DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);

        if (stageConfig != null)
        {
            DataRecord stageStarConfig = DataCenter.mStageStar.GetRecord(stageConfig.getData("ADDSTAR_0").ToString());
            if (stageStarConfig != null)
                condition = string.Format(stageStarConfig.getData("STARNAME"), stageStarConfig.getData("STARVAR"));
        }
        return condition;
    }

    public static int GetChapterTotalStarsNumber(int chapter)
    {
        RammbockWindow win = DataCenter.Self.getObject("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win == null)
            return 0;

        SC_Rammbock_GetTowerClimbingInfo retClimbingInfo = win.m_climbingInfo;
        if (retClimbingInfo == null)
            return 0;
        int[] starList = retClimbingInfo.starList;
        int firstTier = chapter * 3;
        int totalStars = 0;
        for (int i = 0; i < starList.Length; i++)
        {
            //            if (firstTier + i >= starList.Length)
            //                break;

            totalStars += starList[i];
        }
        return totalStars;
    }

    public static List<ItemData> GetGroupItems(int groupID)
    {
        List<ItemData> items = new List<ItemData>();

        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mGroupIDConfig.GetAllRecord())
        {
            DataRecord value = pair.Value;
            int tmpGroupID = (int)value.getObject("GROUP_ID");
            if (tmpGroupID == groupID)
            {
                int itemID = (int)value.getObject("ITEM_ID");
                int itemCount = (int)value.getObject("ITEM_COUNT");
                int lootTime = (int)value.getObject("LOOT_TIME");
                DataRecord groupConfig = DataCenter.mItemIcon.GetRecord(itemID);
                ItemData itemData = new ItemData() { mID = itemID, mType = (int)PackageManager.GetItemTypeByTableID(itemID), mNumber = itemCount * lootTime };
                items.Add(itemData);
            }
        }

        return items;
    }

    /// <summary>
    /// 当前星星总数
    /// </summary>
    /// <returns></returns>
    public static int CurrentStarsNumber()
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win.m_climbingInfo == null)
            return 0;
        return win.m_climbingInfo.currentStars;
    }

    /// <summary>
    /// 历史最高星星总数
    /// </summary>
    /// <returns></returns>
    public static int HistoryMaxStarsNumber()
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win.m_climbingInfo == null)
            return 0;
        return win.m_climbingInfo.rankStars;
    }

    public static bool IsPassAllMission()
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win.m_climbingInfo == null)
            return false;
        DataRecord climbConfig = DataCenter.mClimbingTowerConfig.GetRecord(win.m_climbingInfo.nextTier);
        return (climbConfig == null);
    }

    /// <summary>
    /// 是否可以查看免费重置
    /// </summary>
    /// <returns></returns>
    public static bool CanCheckFreeReset()
    {
        RammbockWindow tmpWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (tmpWin.m_climbingInfo == null)
            return false;
        return true;
    }
    /// <summary>
    /// 是否可以免费重置
    /// </summary>
    /// <returns></returns>
    public static bool CanFreeReset()
    {
        RammbockWindow tmpWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (tmpWin.m_climbingInfo == null)
            return false;
        if (tmpWin.m_climbingInfo.tierState == 1)
            return false;

        //获取重置记录
        DataRecord resetConfig = DataCenter.mClimbingConsumeConfig.GetRecord(tmpWin.m_climbingInfo.resetTimes + 1);
        int tmpMaxResetCount = (int)VIPHelper.GetCurrVIPValueByField(new List<VIP_CONFIG_FIELD>() { VIP_CONFIG_FIELD.RAMMBOCK_RESET_COUNT })[0];

        bool tmpCanFreeReset = false;
        if (resetConfig != null && tmpWin.m_climbingInfo.resetTimes < tmpMaxResetCount)
        {
            //有重置记录
            int nResetCount = 0;
            if (resetConfig.get("RESET_CONSUME", out nResetCount) && nResetCount == 0)
                tmpCanFreeReset = true;
        }
        return tmpCanFreeReset;
    }

    public static List<RammbockAttriBuffData> GetCurrentValidBuffData()
    {
        RammbockWindow tmpWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (tmpWin == null)
            return null;

        if (tmpWin.m_buffDataManager != null)
        {
            tmpWin.m_buffDataManager.Init();
            for (int i = 0, count = tmpWin.m_climbingInfo.buffList.Count; i < count; i++)
                tmpWin.m_buffDataManager.AddBuff(tmpWin.m_climbingInfo.buffList[i]);
        }

        return tmpWin.m_buffDataManager.__CombineSameBuffData();
    }
}

/// <summary>
/// 重置按钮
/// </summary>
class Button_one_key_three_star_btn : CEvent
{
    public override bool _DoEvent()
    {
        int fightPoint = RammbockBase.GetMaxFightPoint();
        float power = GameCommon.GetPower();
        if (power < fightPoint)
        {
            string str = TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_FIGHTPOINT_NOT_ENOUGH);
            DataCenter.ErrorTipsLabelMessage(str);
            return true;
        }
        DataCenter.OpenWindow(UIWindowString.rammbock_list_window);
        return true;
    }
}

/// <summary>
/// 重置按钮
/// </summary>
class Button_reset_btn : CEvent
{
    public override bool _DoEvent()
    {
        //检查是否可以重置
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
		DataRecord r = DataCenter.mClimbingTowerConfig.GetRecord (win.m_climbingInfo.nextTier);
        if (win.m_climbingInfo.tierState == 1 && r != null)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_RAMMBOCK_STILL_CAN_CHALLENGE, false);
            return true;
        }
        int nResetCount = (int)getObject("RESET_COST");
        if (nResetCount > RoleLogicData.Self.diamond)
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND, false);
			GameCommon.ToGetDiamond();
            return true;
        }
		int tmpMaxResetCount = (int)VIPHelper.GetCurrVIPValueByField(new List<VIP_CONFIG_FIELD>() { VIP_CONFIG_FIELD.RAMMBOCK_RESET_COUNT })[0];
        if (win.m_climbingInfo.resetTimes >= tmpMaxResetCount)
        {
            if (RoleLogicData.Self.vipLevel < GameCommon.GetMaxVipLevel())
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SHOP_VIP_LEVEL_LOW, () =>
                {
                    GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
                    {
                        DataCenter.SetData("INFO_GROUP_WINDOW", "PRE_WIN", 1);
                        DataCenter.OpenWindow("RAMMBOCK_WINDOW");
                        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.RAMMBOCK);
                    });
                });
            }
            else
            {
                DataCenter.ErrorTipsLabelMessage(TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_BUY_TIMES_TIPS));
            }
			return true;
		}
        //确认是否要重置
        DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_RAMMBOCK_CONFIRM_START_RESET, "", __OnConfirmReset);
        return true;
    }

    private void __OnConfirmReset()
    {
        RammbockNetManager.RequestRammbockResetClimbTower();
		GameCommon.RoleChangeDiamond (-(int)getObject("RESET_COST"));
		if (GameObject.Find ("Label_no_effect_add_tips").GetComponent <UILabel> () != null) {
			GameObject.Find ("Label_no_effect_add_tips").GetComponent <UILabel> ().text = DataCenter.mStringList.GetData ((int)STRING_INDEX.ERROR_RAMMBOCK_NO_EFFECT_ADD,"STRING_CN");
		}
    }
}

/// <summary>
/// 退出群魔乱舞按钮
/// </summary>
class Button_rammbock_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_WINDOW");
        //added by xuke 红点相关
        TrialNewMarkManager.Self.RefreshTrialNewMark();
        //end
        return true;
    }
}

class Button_renown_btn : CEvent {
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow("SHOP_RENOWN_WINDOW");
		return true;
	}
}

/// <summary>
/// 进入排行
/// </summary>
class Button_rammbock_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RANKLIST_RAMMBOCK_WINDOW");

        return true;
    }
}
