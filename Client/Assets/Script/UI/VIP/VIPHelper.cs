using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

/// <summary>
/// VIP配置辅助类
/// </summary>
public class VIPHelper
{
    /// <summary>
    /// 判断当前VIP等级是否达到指定等级
    /// </summary>
    /// <param name="vipLevel"></param>
    /// <returns></returns>
    public static bool IsReachStandard(int vipLevel)
    {
        return (RoleLogicData.Self.vipLevel >= vipLevel);
    }

    /// <summary>
    /// 获取指定VIP等级的数据
    /// </summary>
    /// <param name="vipLevel"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<object> GetVIPValueByField(int vipLevel, List<VIP_CONFIG_FIELD> types)
    {
        if (DataCenter.mVipListConfig == null)
        {
            DEBUG.LogWarning("VipList is null.");
            return null;
        }

        DataRecord tmpVIPConfig = DataCenter.mVipListConfig.GetRecord(vipLevel);
        if (tmpVIPConfig == null)
            return null;
        string tmpStrFieldName = "";
        List<object> tmpRet = new List<object>();
        for (int i = 0, count = types.Count; i < count; i++)
        {
            tmpStrFieldName = "";
            switch (types[i])
            {
                case VIP_CONFIG_FIELD.DESC: tmpStrFieldName = "VIPDESC"; break;
                case VIP_CONFIG_FIELD.SHOP_STAMINA_DAN: tmpStrFieldName = "MANUAL_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_SPIRIT_DAN: tmpStrFieldName = "ENERGY_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_BEATDEMONCARD: tmpStrFieldName = "CRUSADE_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_EXP_EGG: tmpStrFieldName = "EXP_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_GOLD: tmpStrFieldName = "MONEY_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_ORANGE_EQUIP: tmpStrFieldName = "EUIP_NUM"; break;
                case VIP_CONFIG_FIELD.SHOP_ORANGE_MAGIC: tmpStrFieldName = "TREASURE_NUM"; break;
                case VIP_CONFIG_FIELD.UNION_SACRIFICE: tmpStrFieldName = "WORSHIP_OPEN"; break;
                case VIP_CONFIG_FIELD.EQUIP_STRENGTH_PROBABILITY: tmpStrFieldName = "STR_CRI_RATE"; break;
                case VIP_CONFIG_FIELD.EQUIP_STRENGTH_CRITICAL_LV: tmpStrFieldName = "STR_CRI_LV"; break;
                case VIP_CONFIG_FIELD.DAILY_TASK_RESET_COUNT: tmpStrFieldName = "COPYRESET_NUM"; break;
                case VIP_CONFIG_FIELD.DAILY_TASK_DESTINY_STONE: tmpStrFieldName = "DESTINY"; break;
                case VIP_CONFIG_FIELD.DAILY_TASK_EQUIP_REFINE_STONE: tmpStrFieldName = "REFINE"; break;
                case VIP_CONFIG_FIELD.DAILY_TASK_BREAK_STONE: tmpStrFieldName = "BREACH"; break;
                case VIP_CONFIG_FIELD.FAIRYLAND_RIOT_COUNT: tmpStrFieldName = "EVENT_NUM"; break;
                case VIP_CONFIG_FIELD.FAIRYLAND_20MINUTES_INCOME: tmpStrFieldName = "TREASURE_PROFIT_1"; break;
                case VIP_CONFIG_FIELD.FAIRYLAND_10MINUTES_INCOME: tmpStrFieldName = "TREASURE_PROFIT_2"; break;
                case VIP_CONFIG_FIELD.RAMMBOCK_RESET_COUNT: tmpStrFieldName = "CLIMBING_NUM"; break;
                case VIP_CONFIG_FIELD.BLACK_SHOP_REFRESH_COUNT: tmpStrFieldName = "PART_NUM"; break;
                case VIP_CONFIG_FIELD.AWAKEN_SHOP_REFRESH_COUNT: tmpStrFieldName = "AWAKEN_NUM"; break;
                case VIP_CONFIG_FIELD.BAG_MAX_PET: tmpStrFieldName = "PET_BAG_NUM"; break;
                case VIP_CONFIG_FIELD.BAG_MAX_EQUIP: tmpStrFieldName = "EQUIP_BAG_NUM"; break;
                case VIP_CONFIG_FIELD.BAG_MAX_MAGIC: tmpStrFieldName = "MAGIC_BAG_NUM"; break;
            }
            tmpRet.Add(tmpVIPConfig.getObject(tmpStrFieldName));
        }
        return tmpRet;
    }
    public static object GetVIPValueByField(int vipLevel, VIP_CONFIG_FIELD type)
    {
        List<object> tmpRet = GetVIPValueByField(vipLevel, new List<VIP_CONFIG_FIELD>() { type });
        if (tmpRet == null || tmpRet.Count < 1)
            return null;
        return tmpRet[0];
    }
    public static List<object> GetCurrVIPValueByField(List<VIP_CONFIG_FIELD> types)
    {
        return GetVIPValueByField(RoleLogicData.Self.vipLevel, types);
    }
    public static object GetCurrVIPValueByField(VIP_CONFIG_FIELD type)
    {
        List<object> tmpRet = GetVIPValueByField(RoleLogicData.Self.vipLevel, new List<VIP_CONFIG_FIELD>() { type });
        if (tmpRet == null || tmpRet.Count < 1)
            return null;
        return tmpRet[0];
    }

    /// <summary>
    /// 获取最大VIP等级
    /// </summary>
    /// <returns></returns>
    public static int GetMaxVIPLevel()
    {
        int tmpMaxVIPLevel = 0;
        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mVipListConfig.GetAllRecord())
        {
            if (tmpPair.Key > tmpMaxVIPLevel)
                tmpMaxVIPLevel = tmpPair.Key;
        }
        return tmpMaxVIPLevel;
    }
}
