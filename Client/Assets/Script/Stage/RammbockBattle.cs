using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// 群魔乱舞PVE战斗
/// </summary>

public class RammbockBattle : PVEStageBattle
{
    private static bool mIsInRammbockBattle = false;        //是否在封灵塔战斗中
    public static bool IsInRammbockBattle
    {
        set { mIsInRammbockBattle = value; }
        get { return mIsInRammbockBattle; }
    }

    public override void InitBattleUI()
    {
        BaseUI.OpenWindow("RAMMBOCK_BATTLE_WINDOW", null, false);
		ShowGuankaName();
        //DataCenter.CloseWindow("BATTLE_BOSS_HP_WINDOW");
    }

    //public override void OnFinish(bool bWin)
    //{
    //    if (mbBattleFinish)
    //    {
    //        return;
    //    }

    //    OnStageFinish(bWin);

    //    if (!MainProcess.isOnCheating)
    //    {
    //        RammbockNetManager.RequestClimbTowerResult(bWin ? 1 : 0);
    //    }
    //}

    //protected override void OnRequestResult(bool bWin)
    //{
    //    RammbockNetManager.RequestClimbTowerResult(bWin ? 1 : 0);
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        List<RammbockAttriBuffData> tmpBuffData = RammbockWindow.GetCurrentValidBuffData();
        ClimbTowerBuffInfo[] tmpBuffInfo = new ClimbTowerBuffInfo[tmpBuffData != null ? tmpBuffData.Count : 0];
        for (int i = 0, count = tmpBuffData.Count; i < count; i++)
        {
            tmpBuffInfo[i] = new ClimbTowerBuffInfo();
            tmpBuffInfo[i].buffType = tmpBuffData[i].m_affectType.ToString();
            tmpBuffInfo[i].buffValue =
                tmpBuffData[i].m_isRate ?
                Mathf.FloorToInt(tmpBuffData[i].m_affectValue * 100.0f) :
                Convert.ToInt32(tmpBuffData[i].m_affectValue);
        }
        RammbockNetManager.RequestClimbTowerResult(iswin ? 1 : 0, tmpBuffInfo);
    }
}

public class RammbockBattleWindow : PveBattleWindow
{
    public override void Open(object param)
    {
        GameObject obj = GameCommon.FindUI("back_ground");
        if (obj != null)
            obj.SetActive(false);

        DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
        DataCenter.OpenWindow("BATTLE_PLAYER_EXP_WINDOW");
        //DataCenter.OpenWindow("BATTLE_BOSS_HP_WINDOW");
        //DataCenter.OpenWindow("BATTLE_AUTO_FIGHT_BUTTON");
        DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
	    DataCenter.OpenWindow("PVE_TOP_RIGHT_WINDOW");

        GlobalModule.DoCoroutine(__OnStartBattle());

        DataCenter.OpenWindow("MASK_OPERATE_WINDOW");

        DataCenter.OpenWindow("PET_ATTACK_BOSS_WINDOW"); // 宠物强制跟随窗口

        if (MiniMap.Self != null)
        {
            MiniMap.Self.InitData();
        }
    }

    private IEnumerator __OnStartBattle()
    {
        yield return new WaitForEndOfFrame();
//         yield return new WaitForEndOfFrame();
//         yield return new WaitForEndOfFrame();
//         yield return new WaitForSeconds(0.1f);
//         MainProcess.mStage.Start();
    }
}
