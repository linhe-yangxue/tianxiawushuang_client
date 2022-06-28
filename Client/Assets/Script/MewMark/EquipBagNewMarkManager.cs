using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 装备界面红点管理
/// </summary>
public class EquipBagNewMarkManager 
{
    private static EquipBagNewMarkManager self = null;
    public static EquipBagNewMarkManager Self 
    {
        get 
        {
            if (self == null)
                self = new EquipBagNewMarkManager();
            return self;
        }
    }
    private EquipBagNewMarkManager() 
    {

    }



    #region 检测相关属性
    /// <summary>
    /// 检测装备数量上限
    /// </summary>
    public bool CheckEquipNumLimit { set; get; }
    /// <summary>
    /// 检测装备碎片数量上限
    /// </summary>
    public bool CheckEquipFragNumLimit { set; get; }
    /// <summary>
    /// 检测装备碎片是否可以合成
    /// </summary>
    public bool CheckEquipCompose { set; get; }
    /// <summary>
    /// 检测法器数量上限
    /// </summary>
    public bool CheckMagicNumLimit { set; get; }
    #endregion


    #region 可见性
    /// <summary>
    /// 装备页签红点是否可见
    /// </summary>
    public bool EquipTabVisible { get { return CheckEquipNumLimit; } }
    /// <summary>
    /// 装备碎片红点页签是否可见
    /// </summary>
    public bool EquipFragTabVisible { get { return CheckEquipFragNumLimit || CheckEquipCompose; } }
    /// <summary>
    /// 法器页签红点是否可见
    /// </summary>
    public bool MagicTabVisible { get { return CheckMagicNumLimit; } }
    /// <summary>
    /// 装备背包红点是否可见
    /// </summary>
    public bool EquipBagVisible { get { return EquipTabVisible || EquipFragTabVisible || MagicTabVisible; } }
    #endregion

    private int mEquipFragNumLimit = 200;   //> 装备碎片数量上限
    #region 检测逻辑
    /// <summary>
    /// 检测装备数量是否达到上限
    /// </summary>
    public bool CheckEquipNumLimit_NewMark() 
    {
        if (RoleEquipLogicData.Self == null)
            return false;
        int _equipCount = RoleEquipLogicData.Self.mDicEquip.Count;
        int _equipNumLimit = PackageManager.GetMaxEquipPackageNum();
        CheckEquipNumLimit = _equipCount >= _equipNumLimit;
        return CheckEquipNumLimit;
    }
    /// <summary>
    /// 检测装备碎片数量是否达到上限
    /// </summary>
    public bool CheckEquipFragNumLimit_NewMark() 
    {
        if (RoleEquipFragmentLogicData.Self == null)
            return false;
        int _equipFragCount = RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Count;
        CheckEquipFragNumLimit = _equipFragCount >= mEquipFragNumLimit;
        return CheckEquipFragNumLimit;
    }
    /// <summary>
    /// 检测法器数量是否达到上限
    /// </summary>
    public bool CheckMagicNumLimit_NewMark() 
    {
        if (MagicLogicData.Self == null)
            return false;
        int _magicCount = MagicLogicData.Self.mDicEquip.Count;
        int _magicNumLimit = PackageManager.GetMaxMagicPackageNum();
        CheckMagicNumLimit = _magicCount >= _magicNumLimit;
        return CheckMagicNumLimit;
    }
    /// <summary>
    /// 检测装备是否可合成
    /// </summary>
    public bool CheckEquipCompose_NewMark() 
    {
        if (RoleEquipFragmentLogicData.Self == null)
            return false;
        Dictionary<int, EquipFragmentData> _dicEquipFrag = RoleEquipFragmentLogicData.Self.mDicEquipFragmentData;
        foreach (var equipFragData in _dicEquipFrag) 
        {
            int _costFragNum = TableCommon.GetNumberFromFragment(equipFragData.Value.tid, "COST_NUM");
            if (equipFragData.Value.itemNum >= _costFragNum)
            {
                CheckEquipCompose = true;
                return CheckEquipCompose;
            }
        }
        CheckEquipCompose = false;
        return CheckEquipCompose;
    }

    /// <summary>
    /// 检测装备背包按钮是否需要显示红点
    /// </summary>
    /// <returns></returns>
    public bool CheckEquipBagNewMark()
    {
        if (CheckEquipNumLimit_NewMark())
            return true;
        if (CheckEquipFragNumLimit_NewMark())
            return true;
        if (CheckMagicNumLimit_NewMark())
            return true;
        if (CheckEquipCompose_NewMark())
            return true;
        return false;
    }
    /// <summary>
    /// 检测背包的所有信息
    /// </summary>
    public void CheckEquipBagInfoAll_NewMark() 
    {
        CheckEquipNumLimit_NewMark();
        CheckEquipFragNumLimit_NewMark();
        CheckMagicNumLimit_NewMark();
        CheckEquipCompose_NewMark();
    }
    #endregion

    #region 刷新逻辑
    /// <summary>
    /// 刷新装备背包界面标签红点
    /// </summary>
    public void RefreshEquipBagTabNewMark() 
    {
        DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "REFRESH_EQUIP_BAG_NEWMARK", null);
    }
    #endregion

}
