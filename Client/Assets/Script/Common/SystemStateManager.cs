using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SystemStateManager
{
    private static Dictionary<SYSTEM_STATE, bool> mDicSystemState = new Dictionary<SYSTEM_STATE, bool>();

    public static void Init()
    {
        //by chenliang
        //begin

        //首先清除之前的数据
        mDicSystemState.Clear();

        //end
        for (int i = (int)SYSTEM_STATE.NOTIF_MAIL; i < (int)SYSTEM_STATE.NOTIF_MAX; i++)
        {
            mDicSystemState.Add((SYSTEM_STATE)i, false);
        }
    }

    public static void SetAllNotifState(Array stateList)
    {
        int iStateListCount = stateList.Length;
        for (int i = (int)SYSTEM_STATE.NOTIF_MAIL; i < (int)SYSTEM_STATE.NOTIF_MAX; i++)
        {
            int m = iStateListCount > 0 ? Array.Find<int>(stateList as int[], x => x == i) : 0;
            if (m > 0)
            {
                SetNotifState((SYSTEM_STATE)i, true);
            }
            //added by xuke 设置活动状态
            else
            {            
                if (CheckIsActivityState((SYSTEM_STATE)i)) 
                {
                    SetNotifState((SYSTEM_STATE)i, false);
                }
                if (CheckNeedSetFalse((SYSTEM_STATE)i)) 
                {
                    SetNotifState((SYSTEM_STATE)i, false);
                }
            }
            //end
        }
    }

    //宗门的全部置为false
    public static void SetUnionNotifStateFalse()
    {
        for (int i = (int)SYSTEM_STATE.NOTIF_GUILD_WORSHIP; i <= (int)SYSTEM_STATE.NOTIF_GUILD_SHOP; i++)
        {
            SetNotifState((SYSTEM_STATE)i, false);
        }
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    /// <param name="stateType">状态类型</param>
    /// <param name="isState">状态值</param>
    /// <param name="isSave">是否保存状态</param>     //by chenliang
    public static void SetNotifState(SYSTEM_STATE stateType, bool isState, bool isSave = true)
    {
        //added by xuke 根据活动结束时间过滤状态
        if(CheckIsActivityState(stateType))
            isState = CheckActivityOpenTime(stateType,isState);
        //end
        if (isState)
        {
            ActivateNotifState(stateType);
        }
        else
        {
            InactiveNotifState(stateType);
        }
        //by chenliang
        //begin

        if (isSave)
        {
            //记录状态到本地
            SaveState(stateType);
        }

        //end

        switch (stateType)
        {
            case SYSTEM_STATE.NOTIF_MAIL:
                GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);
                break;
            case SYSTEM_STATE.NOTIF_BOSS:
                //GameObject obj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu01/ec_ui_tianmo").gameObject;
                //GameObject putongObj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu01/ec_mainputong").gameObject;
                //if (obj != null && putongObj != null)
                //{
                //    obj.SetActive(GetNotifState(stateType));
                //    putongObj.SetActive(!GetNotifState(stateType));
                //}
                break;
            case SYSTEM_STATE.NOTIF_TASK_FINISH:
                break;
            case SYSTEM_STATE.NOTIF_TASK_REFRESH:
                break;
            case SYSTEM_STATE.NOTIF_SHOP:
                break;
            case SYSTEM_STATE.NOTIF_FRIEND_CHANGE:
                GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_FIREND_MARK", true);
                break;
            case SYSTEM_STATE.NOTIF_FRIEND_REQUEST:
                GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_FRIEND_MARK", isState);
                break;
            case SYSTEM_STATE.NOTIF_FRIEND_SPIRIT:
                GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_SPIRIT_MARK", isState);
                break;
			case SYSTEM_STATE.NOTIF_REVELRY:
				break;
            case SYSTEM_STATE.NOTIF_ATLAS:  //> 图鉴
                DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP", "REFRESH_ATLAS_NEWMARK", null);
                break;
            case SYSTEM_STATE.NOTIF_PET_SHOP:   //> 符灵商铺
                DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_MYSTERYSHOP_MARK", isState);
                break;
            case SYSTEM_STATE.NOTIF_REPUTATION_SHOP:    //> 声望商店
                PVPNewMarkManager.Self.CheckReward = isState;
                break;
            case SYSTEM_STATE.NOTIF_CLOTH_SHOP:         //> 灵核商店
                RammbockNewMarkManager.Self.CheckReward = isState;
                break;
            case SYSTEM_STATE.NOTIF_RANK_ACTIVITY:
                DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_RANK_ACTIVITY", isState);                
                break;
        }
    }

    /// <summary>
    /// 判断如果服务器没有传过来状态是否需要置为false
    /// </summary>
    /// <param name="stateType"></param>
    /// <returns></returns>
    private static bool CheckNeedSetFalse(SYSTEM_STATE stateType) 
    {
        if (SYSTEM_STATE.NOTIF_SHOP_NORMAL == stateType)            //> 商店免费抽卡
            return true;
        else if (SYSTEM_STATE.NOTIF_SHOP_ADVANCE == stateType)      //> 商店高级抽卡
            return true;
        else if (SYSTEM_STATE.NOTIF_SHOP_VIP_GIFT == stateType)     //> 商店VIP礼包
            return true;
        else if (SYSTEM_STATE.NOTIF_PET_SHOP == stateType)          //> 符灵商铺
            return true;
        else if (SYSTEM_STATE.NOTIF_REPUTATION_SHOP == stateType)   //> 声望商店
            return true;
        else if (SYSTEM_STATE.NOTIF_CLOTH_SHOP == stateType)        //> 灵核商店
            return true;
        return false;
    }

    #region 活动红点

    private static bool CheckIsActivityState(SYSTEM_STATE stateType) 
    {
        if ((int)SYSTEM_STATE.NOTIF_LUXURY_SIGN <= (int)stateType && (int)stateType <= (int)SYSTEM_STATE.NOTIF_ACTIVITY_MAX)
            return true;
        return false;
    }
    private static bool CheckActivityOpenTime(SYSTEM_STATE stateType,bool kIsState) 
    {
        object objVal = DataCenter.Self.getObject("OPEN_TIME");
        if (objVal == null)
            return kIsState;
        SC_GetActivitiesEndTime _receive = (SC_GetActivitiesEndTime)objVal;
        if (_receive == null)
            return kIsState;
        //判断活动是否在配表中
        if (!ActivityWindow.CheckActivityInConfig(ActivityWindow.GetEventTypeByNotifState(stateType))) 
        {
            return false;
        }
        switch (stateType) 
        {
            case SYSTEM_STATE.NOTIF_CHARGE_AWARD:   //> 充值送礼
                return kIsState && ActivityWindow.CheckActivityIsOpenByTime(_receive.rechargeTime.activityOpenTime, _receive.rechargeTime.activityEndTime, _receive.rechargeTime.rewardEndTime);
            case SYSTEM_STATE.NOTIF_CUMULATIVE:     //> 累计消费
                if (RoleLogicData.Self == null)
                    return kIsState;
                return kIsState && (ActivityCumulativeWindow.ACTIVITY_OPEN_LEVEL <= RoleLogicData.Self.character.level) && ActivityWindow.CheckActivityIsOpenByTime(_receive.cumulativeTime.activityOpenTime, _receive.cumulativeTime.activityEndTime, _receive.cumulativeTime.rewardEndTime);
            case SYSTEM_STATE.NOTIF_FLASH_SALE:     //> 限时折扣
                string[] mtimes = DataCenter.mOpenTime.GetData(111, "Condition").ToString().Split('|');
                return kIsState && ActivityWindow.CheckActivityIsOpenByTime(Int64.Parse(mtimes[0]), Int64.Parse(mtimes[1]), Int64.Parse(mtimes[1]));
            //case SYSTEM_STATE.NOTIF_SHOOTER_PARK:   //> 射手乐园
            //    return kIsState && ActivityShooterParkWindow.GetIsActivityOpen();
        }
        return kIsState;
    }

    #endregion

    /// <summary>
    /// 刷新状态
    /// </summary>
    /// <param name="stateType">状态类型</param>
    private static void ActivateNotifState(SYSTEM_STATE stateType)
    {
        if (mDicSystemState.ContainsKey(stateType))
        {
            mDicSystemState[stateType] |= true;
            DEBUG.Log("mDicSystemState[" + stateType.ToString() + "] = " + mDicSystemState[stateType].ToString());
        }
    }

    /// <summary>
    /// 设置状态为false
    /// </summary>
    /// <param name="stateType">状态类型</param>
    private static void InactiveNotifState(SYSTEM_STATE stateType)
    {
        if (mDicSystemState.ContainsKey(stateType))
        {
            mDicSystemState[stateType] = false;
            DEBUG.Log("mDicSystemState[" + stateType.ToString() + "] = " + mDicSystemState[stateType].ToString());
        }
    }

    /// <summary>
    /// 获得状态
    /// </summary>
    /// <param name="state"></param>
    public static bool GetNotifState(SYSTEM_STATE stateType)
    {
        if (mDicSystemState.ContainsKey(stateType))
        {
            return mDicSystemState[stateType];
        }
        return false;
    }
    //by chenliang
    //begin

    /// <summary>
    /// 获取指定状态对应的本地存储键值
    /// </summary>
    /// <param name="stateType"></param>
    /// <returns></returns>
    private static string __GetPlayerPrefsKey(SYSTEM_STATE stateType)
    {
        string tmpKey = "SYSTEM_STATE_MANAGER_LOCAL_STATE_";
        tmpKey += (CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + "_" + CommonParam.LoginPort);
        tmpKey += ("_" + (int)stateType);
        return tmpKey;
    }
    /// <summary>
    /// 保存指定的状态到本地
    /// </summary>
    /// <param name="stateType"></param>
    public static void SaveState(SYSTEM_STATE stateType)
    {
        string tmpKey = __GetPlayerPrefsKey(stateType);
        bool tmpState = false;
        if (!mDicSystemState.TryGetValue(stateType, out tmpState))
            tmpState = false;
        PlayerPrefs.SetInt(tmpKey, tmpState ? 1 : 0);
    }
    /// <summary>
    /// 保存所有状态到本地
    /// </summary>
    public static void SaveAllStates()
    {
        for (int i = (int)SYSTEM_STATE.NOTIF_MAIL, count = (int)SYSTEM_STATE.NOTIF_MAX; i < count; i++)
            SaveState((SYSTEM_STATE)i);
    }
    /// <summary>
    /// 读取本地指定的状态
    /// </summary>
    /// <param name="stateType"></param>
    public static void ReadState(SYSTEM_STATE stateType)
    {
        string tmpKey = __GetPlayerPrefsKey(stateType);
        bool tmpState = false;
        if (PlayerPrefs.HasKey(tmpKey))
            tmpState = (PlayerPrefs.GetInt(tmpKey) == 1) ? true : false;
        SetNotifState(stateType, tmpState, false);
    }
    /// <summary>
    /// 读取本地所有的状态
    /// </summary>
    public static void ReadAllStates()
    {
        for (int i = (int)SYSTEM_STATE.NOTIF_MAIL, count = (int)SYSTEM_STATE.NOTIF_MAX; i < count; i++)
            ReadState((SYSTEM_STATE)i);
    }

    //end
}