using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 队伍红点状态管理
/// </summary>
public class TeamNewMarkManager
{
    public static bool mNeedCheckLevelUp = false;   //> 是否需要检测符灵升级
    private static TeamNewMarkManager self = null;
    public static TeamNewMarkManager Self
    {
        //set { self = value; }
        get
        {
            if (self == null)
                self = new TeamNewMarkManager();
            return self;
        }
    }

    private TeamNewMarkManager() 
    {
        mTab1NewMarkData = new TeamTab_1_NewMarkData();
        mTab2NewMarkData = new TeamTab_2_NewMarkData();
        mTab3NewMarkData = new TeamTab_3_NewMarkData();
    }
    public TeamTab_1_NewMarkData mTab1NewMarkData;      //> 队伍页签1红点数据
    public TeamTab_2_NewMarkData mTab2NewMarkData;      //> 队伍页签2红点数据
    public TeamTab_3_NewMarkData mTab3NewMarkData;      //> 队伍页签3红点数据

    // 判断是否越界,true为越界,false为没有越界
    private bool __CheckPetTeamPosIndex(int kTeamPos)
    {
        if (kTeamPos < 0 || kTeamPos >= mTab1NewMarkData.mPetNewMarkData.Length)
            return true;
        return false;
    }
    // 判断是否越界,true为越界,false为没有越界
    private bool __CheckEquipTeamPosIndex(int kTeamPos, int kEquipType, ITEM_TYPE kRealType) 
    {
        if (__CheckPetTeamPosIndex(kTeamPos))
            return true;
        if (ITEM_TYPE.EQUIP == kRealType) 
        {
            if (kEquipType < 0 || kEquipType >= mTab1NewMarkData.mPetNewMarkData[kTeamPos].mEquipNewMarkData.Length)
                return true;
        }
        else if (ITEM_TYPE.MAGIC == kRealType) 
        {
            if (kEquipType < 0 || kEquipType >= mTab1NewMarkData.mPetNewMarkData[kTeamPos].mMagicNewMarkData.Length)
                return true;
        }        
        return false;
    }

    /// <summary>
    /// 获得指定符灵的红点相关数据
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public PetNewMarkData GetPetNewMarkData(int kTeamPos)
    {
        if (__CheckPetTeamPosIndex(kTeamPos))
            return mTab1NewMarkData.mPetNewMarkData[(int)TEAM_POS.CHARACTER];
        return mTab1NewMarkData.mPetNewMarkData[kTeamPos];
    }
    public EquipNewMarkData GetEquipNewMarkData(int kTeamPos, int kEquipType, ITEM_TYPE kRealType) 
    {
        if (__CheckEquipTeamPosIndex(kTeamPos, kEquipType, kRealType))
            return null;
        if (ITEM_TYPE.EQUIP == kRealType) 
        {
            return mTab1NewMarkData.mPetNewMarkData[kTeamPos].mEquipNewMarkData[kEquipType];
        }
        else if (ITEM_TYPE.MAGIC == kRealType) 
        {
            return mTab1NewMarkData.mPetNewMarkData[kTeamPos].mMagicNewMarkData[kEquipType];
        }
        return null;
    }

    
    #region 检测

    public void CheckTeamNewMark()
    {
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++)
        {
            TeamManager.CheckOnePetInfoAll_NewMark(i);
        }
        TeamManager.CheckPetRelate();
        TeamManager.CheckTeamPetBagTab_NewMark();
        TeamManager.CheckTeamPetFragTab_NewMark();
    }

    //1.银币相关
    public void CheckCoin() 
    {
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++) 
        {
            TeamManager.CheckPetCanBreakUp(i);
            TeamManager.CheckPetHasCanSkillUp(i);
            TeamManager.CheckPetCanLevelUp(i);
        }
    }
    //2.符灵相关


    public void CheckPet() 
    {
        //1.符灵技能，符灵突破，符灵缘分，装备缘分，换灵，符灵能否升级       
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++)
        {
            TeamManager.CheckHasPetCanSetUp(i);
            TeamManager.CheckHasHighQualityPetCanSetUp(i);

            TeamManager.CheckPetCanLevelUp(i);
        }
        TeamManager.CheckPetRelate();
    }
    /// <summary>
    /// 符灵上下阵的时候进行检测
    /// </summary>
    public void CheckChangePet(int kTeamPos) 
    {
        CheckPet();
        TeamManager.CheckPetHasCanSkillUp(kTeamPos);
        TeamManager.CheckPetCanBreakUp(kTeamPos);
        
        for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; j++)
        {
            TeamManager.CheckHasRelateEquipSetUp(kTeamPos, j, ITEM_TYPE.EQUIP);
            //新符灵上阵
            TeamManager.CheckHasEquipSetUp(kTeamPos,j,ITEM_TYPE.EQUIP); 
        }
        for (int k = (int)MAGIC_TYPE.MAGIC1; k < (int)MAGIC_TYPE.MAX; k++)
        {
            TeamManager.CheckHasRelateEquipSetUp(kTeamPos, k, ITEM_TYPE.MAGIC);
            //新符灵上阵
            TeamManager.CheckHasEquipSetUp(kTeamPos, k, ITEM_TYPE.MAGIC); 
        }
    }

    /// <summary>
    /// 检测合成新的符灵
    /// </summary>
    public void CheckComposeNewPet() 
    {
        TeamManager.CheckTeamPetFragTab_NewMark();
        TeamManager.CheckTeamPetBagTab_NewMark();
        CheckPet();
    }
    /// <summary>
    /// 检测出售符灵
    /// </summary>
    public void CheckSalePet() 
    {
        CheckPet();
        CheckCoin();
        TeamManager.CheckTeamPetBagTab_NewMark();
    }
    /// <summary>
    /// 符灵升级进行检测
    /// </summary>
    public void CheckPetLevelUp() 
    {
        CheckPet();
        CheckCoin();
        TeamManager.CheckTeamPetBagTab_NewMark();
    }

    //3.装备相关
    public void CheckEquip() 
    {
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++) 
        {
            for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; j++) 
            {
                TeamManager.CheckOneEquipInfoAll_NewMark(i, j, ITEM_TYPE.EQUIP);            
            }
        }
    }

    public void CheckMagicEquip() 
    {
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++)
        {
            for (int k = (int)MAGIC_TYPE.MAGIC1; k < (int)MAGIC_TYPE.MAX; k++)
            {
                TeamManager.CheckOneEquipInfoAll_NewMark(i, k, ITEM_TYPE.MAGIC);
            }
        }
    }
    /// <summary>
    /// 检测法器强化
    /// </summary>
    public void CheckMagicEquipStrengthen() 
    {
        CheckMagicEquip();
        CheckCoin();
    }
    #endregion



    /// <summary>
    /// 刷新队伍红点状态,包括符灵头像,标签页,缘分站位,所有装备红点
    /// </summary>
    public void RefreshTeamNewMark()
    {
        DataCenter.SetData("TEAM_WINDOW", "REFRESH_NEW_MARK", null);
    }
    /// <summary>
    /// 刷新队伍红点状态，包括突破，技能按钮，符灵升级按钮
    /// </summary>
    public void RefreshTeamPosInfoNewMark()
    {
        DataCenter.SetData("TEAM_POS_INFO_WINDOW", "REFRESH_TEAM_POS_INFO_NEWMARK", null);
    }
    /// <summary>
    /// 刷新队伍红点状态，包括换灵按钮
    /// </summary>
    public void RefreshTeamInfoNewMark() 
    {
        DataCenter.SetData("TEAM_INFO_WINDOW", "REFRESH_CHANGE_NEWMARK",null);
    }
    /// <summary>
    /// 刷新缘分站位界面 
    /// </summary>
    public void RefreshRelPosNewMark() 
    {
        DataCenter.SetData("TEAM_RELATE_INFO_WINDOW", "REFRESH_RELATION_NEWMARK", null);
    }

    public void RefreshTeam_AllNewMark() 
    {
        RefreshTeamNewMark();
        RefreshTeamPosInfoNewMark();
        RefreshTeamInfoNewMark();
    }

}


/// <summary>
/// 装备红点状态数据
/// </summary>
public class EquipNewMarkData
{
    #region 检测项
    /// <summary>
    /// 当前装备是否达到了穿戴等级
    /// </summary>
    public bool CheckOpenLevel { set; get; }
    /// <summary>
    /// 当前装备位置为空时,是否有新装备可以穿
    /// </summary>
    public bool CheckEmptyEquipSetUp { set; get; }
    /// <summary>
    /// 当前装备是否可以强化
    /// </summary>
    public bool CheckStrengthen { set; get; }
    /// <summary>
    /// 是否有更高等级的装备
    /// </summary>
    public bool CheckHighQuality { set; get; }
    /// <summary>
    /// 是否有缘分推荐装备
    /// </summary>
    public bool CheckRelate { set; get; }
    #endregion
    #region 红点可见性属性
    /// <summary>
    /// 强化红点是否可见
    /// </summary>
    public bool StrengthenVisible
    {
        get { return CheckStrengthen; }
    }
    /// <summary>
    /// 更换按钮红点是否可见
    /// </summary>
    public bool ChangeVisible
    {
        get { return CheckHighQuality; }
    }
    /// <summary>
    /// 装备图标红点是否可见
    /// </summary>
    public bool EquipVisible
    {
        get { return CheckOpenLevel && (CheckEmptyEquipSetUp || StrengthenVisible || ChangeVisible || CheckRelate); }
    }
    #endregion
}

/// <summary>
/// 符灵红点状态数据
/// </summary>
public class PetNewMarkData
{
    private int mEquipCount = 4;
    private int mMagicCount = 2;
    public PetNewMarkData()
    {
        for (int i = 0; i < mEquipCount; i++)
        {
            mEquipNewMarkData[i] = new EquipNewMarkData();
        }
        for (int j = 0; j < mMagicCount; j++)
        {
            mMagicNewMarkData[j] = new EquipNewMarkData();
        }
    }

    #region 检测项
    public bool CheckOpenLevel { set; get; }
    /// <summary>
    /// 检测是否可以突破
    /// </summary>
    public bool CheckBreak { set; get; }
    /// <summary>
    /// 检测是否可以升级技能
    /// </summary>
    public bool CheckSkillUp { set; get; }
    /// <summary>
    /// 检测是否有空位可以上阵新的符灵
    /// </summary>
    public bool CheckEmptyPetSetUp { get; set; }
    /// <summary>
    /// 装备可见性数据
    /// </summary>
    public EquipNewMarkData[] mEquipNewMarkData = new EquipNewMarkData[4];
    public EquipNewMarkData[] mMagicNewMarkData = new EquipNewMarkData[2];
    /// <summary>
    /// 是否有更高品质的符灵
    /// </summary>
    public bool CheckHighQuality { set; get; }

    /// <summary>
    /// 检测符灵当前是否有足够经验可以升级
    /// </summary>
    public bool CheckLevelUp { set; get; }
    #endregion

    #region 红点可见性
    /// <summary>
    /// 换灵按钮红点是否可见
    /// </summary>
    public bool ChangeVisible
    {
        get { return CheckHighQuality; }
    }
    /// <summary>
    /// 符灵升级按钮红点是否可见
    /// </summary>
    public bool LevelUpVisible 
    {
        get { return CheckLevelUp; }
    }
    /// <summary>
    /// 符灵头像是否需要显示红点
    /// </summary>
    public bool PetIconVisible
    {
        get { return CheckOpenLevel && (CheckEmptyPetSetUp || ChangeVisible || CheckBreak || CheckSkillUp || GetEquipHasNewMark() || LevelUpVisible); }
    }
    #endregion
    private bool GetEquipHasNewMark()
    {
        for (int i = 0; i < mEquipCount; i++)
        {
            if (mEquipNewMarkData[i].EquipVisible)
                return true;
        }
        for (int j = 0; j < mMagicCount; j++)
        {
            if (mMagicNewMarkData[j].EquipVisible)
                return true;
        }
        return false;
    }


}


/// <summary>
/// 队伍页签1相关红点数据
/// </summary>
public class TeamTab_1_NewMarkData
{
    private int mPetCount = 4;
    public PetNewMarkData[] mPetNewMarkData = new PetNewMarkData[4];

    public TeamTab_1_NewMarkData()
    {
        for (int i = 0; i < mPetCount; i++)
        {
            mPetNewMarkData[i] = new PetNewMarkData();
        }
    }

    #region 检测项
    private bool mCheckRelatePos = false;
    /// <summary>
    /// 检测缘分站位是否有符灵需要上阵
    /// </summary>
    public bool CheckRelatePos 
    {
        set 
        {
            mCheckRelatePos = value;
            TeamManager.mIsRelateDirty = false;
        }
        get 
        {
            return mCheckRelatePos;
        }
    }

    #endregion

    #region 红点可见性
    public bool RelatePosVisible { get { return CheckRelatePos; } }
    /// <summary>
    /// 队伍页签1红点是否可见
    /// </summary>
    public bool TabVisible
    {
        get { return GetPetHasNewMark() || CheckRelatePos; }
    }
    #endregion
    private bool GetPetHasNewMark()
    {
        for (int i = 0; i < mPetCount; i++)
        {
            if (mPetNewMarkData[i].PetIconVisible)
                return true;
        }
        return false;
    }
}
/// <summary>
/// 队伍页签2相关红点数据
/// </summary>
public class TeamTab_2_NewMarkData
{
    #region 检测项
    /// <summary>
    /// 符灵背包是否已满
    /// </summary>
    public bool CheckPetBagFull { get; set; }
    #endregion

    #region 红点可见性
    /// <summary>
    /// 队伍页签2红点是否可见
    /// </summary>
    public bool TabVisible { get { return CheckPetBagFull; } }
    #endregion
}
/// <summary>
/// 队伍页签3相关红点数据
/// </summary>
public class TeamTab_3_NewMarkData
{
    #region 检测项
    /// <summary>
    /// 检测是否有可以合成的符灵碎片
    /// </summary>
    public bool CheckCanCompose { get; set; }

    #endregion

    #region 红点可见性
    public bool TabVisible { get { return CheckCanCompose; } }
    #endregion
}