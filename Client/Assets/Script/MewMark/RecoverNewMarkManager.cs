using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 回收界面红点管理
/// </summary>
public class RecoverNewMarkManager
{
    private static RecoverNewMarkManager self = null;
    public static RecoverNewMarkManager Self 
    {
        get 
        {
            if (self == null)
                self = new RecoverNewMarkManager();
            return self;
        }
    }
    private RecoverNewMarkManager() { }

    #region 检测相关属性
    /// <summary>
    /// 检测是否需要提示回收符灵
    /// </summary>
    public bool CheckRecoverPet { set; get; }
    /// <summary>
    /// 检测是否需要提示回收装备
    /// </summary>
    public bool CheckRecoverEquip { set; get; }
    #endregion



    #region 可见性
    /// <summary>
    /// 符灵回收红点是否可见
    /// </summary>
    public bool RecoverPetVisible { get { return CheckRecoverPet; } }
    /// <summary>
    /// 装备回收红点是否可见
    /// </summary>
    public bool RecoverEquipVisible { get { return CheckRecoverEquip; } }
    #endregion


    private int mRecoverPetNumToShowNewMark = 6;    //> 达到该数量则进行提示
    private int mRecoverEquipNumToShowNewMark = 6;
    private int mPetQualityThreshold = 3;           //> 符灵回收品质临界值
    private int mEquipQualityThreshold = 3;         //> 装备回收品质临界值
    
    #region 检测逻辑
    /// <summary>
    /// 检测符灵回收是否达到提示条件
    /// </summary>
    /// <returns></returns>
    public bool CheckRecoverPet_NewMark()
    {
        if (PetLogicData.Self == null)
            return false;
        int _petCount = 0;
        List<PetData> _petDataList = new List<PetData>(PetLogicData.Self.mDicPetData.Values);
        foreach (PetData petData in _petDataList) 
        {
            if(TeamManager.CheckIsExpPet(petData.tid))
                continue;
            int _quality = TableCommon.GetNumberFromActiveCongfig(petData.tid, "STAR_LEVEL");
            if(petData.teamPos < 0 && 1 < _quality && _quality <= mPetQualityThreshold)
            {
                _petCount++;
            }
        }
        CheckRecoverPet = _petCount >= mRecoverPetNumToShowNewMark;
        return CheckRecoverPet;
    }
    /// <summary>
    /// 检测装备是否需要提示回收
    /// </summary>
    /// <returns></returns>
    public bool CheckRecoverEquip_NewMark()
    {
        if (RoleEquipLogicData.Self == null)
            return false;
        int _equipCount = 0;
        List<EquipData> _equipDataList = new List<EquipData>(RoleEquipLogicData.Self.mDicEquip.Values);
        foreach (EquipData equipData in _equipDataList) 
        {
            if (equipData.teamPos >= 0)
                continue;
            int _quality = GameCommon.GetItemQuality(equipData.tid);
            if (_quality <= mEquipQualityThreshold)
            {
                _equipCount++;
            }
        }
        CheckRecoverEquip = _equipCount >= mRecoverEquipNumToShowNewMark;
        return CheckRecoverEquip;
    }
    /// <summary>
    /// 检测回收按钮是否需要显示红点
    /// </summary>
    /// <returns></returns>
    public bool CheckRecoverBtn_NewMark()
    {
        if (CheckRecoverPet_NewMark())
            return true;
        if (CheckRecoverEquip_NewMark())
            return true;
        return false;
    }

    public void CheckRecoverInfoAll_NewMark() 
    {
        CheckRecoverPet_NewMark();
        CheckRecoverEquip_NewMark();
    }
    #endregion

    #region 刷新逻辑
    public void RefreshRecoverNewMark() 
    {
        DataCenter.SetData("RECOVER_WINDOW", "REFRESH_RECOVER_NEWMARK",null);
    }

    #endregion
}

