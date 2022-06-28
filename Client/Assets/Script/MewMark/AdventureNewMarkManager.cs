using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 冒险红点管理
/// </summary>
public class AdventureNewMarkManager : ManagerSingleton<AdventureNewMarkManager>
{
    #region 检测数据
    Dictionary<int,List<bool>> mHasRewardDic = new Dictionary<int,List<bool>>();


    public bool CheckLeftReward { set; get; }
    public bool CheckRightReward { set; get; }
    public bool CheckAdventure { set; get; }
    #endregion

    #region 可见性
    public bool LeftRewardVisible { get { return CheckLeftReward; } }
    public bool RightRewardVisible { get { return CheckRightReward; } }
    public bool AdventureBtnVisible { get { return CheckAdventure; } }
    #endregion

    #region 检测逻辑
    /// <summary>
    /// 检测指定章节是否有奖励可以领取
    /// </summary>
    /// <param name="kPageIndex"></param>
    public bool CheckPage_NewMark(int kDifficulty,int kPageIndex) 
    {
        int _totalStarNum = TaskRewardBox.GetTotalStarNum(kDifficulty, kPageIndex);
        bool _hasReward = false;
        for (int rewardIndex = 1; rewardIndex <= 3; rewardIndex++)
        {
            _hasReward = (ReceiveState.CanReceive == RewardHelper.GetReceiveState(kDifficulty, kPageIndex, _totalStarNum, rewardIndex)) || _hasReward;
            if (_hasReward) 
            {
                break;
            }         
        }
        CheckAdventure = _hasReward || CheckLeftReward || CheckRightReward;
        mHasRewardDic[kDifficulty][kPageIndex] = _hasReward;
        return mHasRewardDic[kDifficulty][kPageIndex];
    }

    public bool CheckCurPage_NewMark() 
    {
        return CheckPage_NewMark(ScrollWorldMapWindow.mDifficulty,ScrollWorldMapWindow.mPage);
    }
    /// <summary>
    /// 检测所有章节,判断是否有奖励可以领取
    /// </summary>
    /// <returns></returns>
    public bool CheckDifficultyReward_NewMark(int kDifficulty)
    {
        return mHasRewardDic[kDifficulty].Contains(true);
    }
    /// <summary>
    /// 检测各难度关卡是否有奖励可以领取
    /// </summary>
    /// <returns></returns>
    public bool CheckReward_NewMark() 
    {
        for (int dif = 1; dif <= 2; dif++) 
        {
            mHasRewardDic[dif] = new List<bool>();
            //遍历当前所有开启章节
            for (int page = 0; page < ScrollWorldMapWindow.MAP_COUNT; page++)
            {
                int _totalStarNum = TaskRewardBox.GetTotalStarNum(dif,page);
                bool _hasReward = false;
                for (int rewardIndex = 1; rewardIndex <= 3; rewardIndex++) 
                {
                    _hasReward = (ReceiveState.CanReceive == RewardHelper.GetReceiveState(dif, page, _totalStarNum, rewardIndex)) || _hasReward;
                    if (_hasReward) 
                    {
                        CheckAdventure = true;
                        break;
                   }                      
                }
                mHasRewardDic[dif].Add(_hasReward);
            }
        }
            return mHasRewardDic[1].Contains(true) || mHasRewardDic[2].Contains(true);
    }
    #endregion

    #region 刷新逻辑
    private bool CheckCurPageReward() 
    {
        return mHasRewardDic[ScrollWorldMapWindow.mDifficulty][ScrollWorldMapWindow.mPage];
    }
    /// <summary>
    /// 刷新箭头红点
    /// </summary>
    public void RefreshArrowNewMark(int kDifficulty,int kPageIndex) 
    {
        CheckLeftReward = false;
        for (int i = 0; i < kPageIndex; i++) 
        {
            if (mHasRewardDic[kDifficulty][i])
            {
                CheckLeftReward = true;
                break;
            }
        }
        CheckRightReward = false;
        for (int j = kPageIndex + 1; j < ScrollWorldMapWindow.MAP_COUNT; j++) 
        {
            if (mHasRewardDic[kDifficulty][j])
            {
                CheckRightReward = true;
                break;
            }   
        }
        CheckAdventure = CheckRightReward || CheckLeftReward || CheckCurPageReward();
        DataCenter.SetData("SCROLL_WORLD_MAP_LEFT", "REFRESH_NEWMARK", null);
        DataCenter.SetData("SCROLL_WORLD_MAP_RIGHT", "REFRESH_NEWMARK", null);
    }
  
    #endregion
}
