using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//夺宝战斗

public class GrabBattle : PVP4Battle
{
    public override void OnStart()
    {
        DataCenter.SetData("GRABTREASURE_BATTLE_LOADING_WINDOW", "CONTINUE", true);
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
    //        //战斗结束
    //        GrabTreasureWindow win = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
    //        int tmpFragId = (int)win.getObject("CURRENT_GRAB_FRAG_ID");
    //        string tmpGrabUid = (string)win.getObject("CURRENT_GRAB_UID");
    //        GlobalModule.DoCoroutine(__DoAction(tmpGrabUid, tmpFragId, bWin ? 1 : 0));
    //    }
    //}

    //protected override void OnRequestResult(bool bWin)
    //{
    //    GrabTreasureWindow win = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
    //    int tmpFragId = (int)win.getObject("CURRENT_GRAB_FRAG_ID");
    //    string tmpGrabUid = (string)win.getObject("CURRENT_GRAB_UID");
    //    GlobalModule.DoCoroutine(__DoAction(tmpGrabUid, tmpFragId, bWin ? 1 : 0));
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        GrabTreasureWindow win = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        int tmpFragId = (int)win.getObject("CURRENT_GRAB_FRAG_ID");
        string tmpGrabUid = (string)win.getObject("CURRENT_GRAB_UID");
        GlobalModule.DoCoroutine(__DoAction(tmpGrabUid, tmpFragId, iswin ? 1 : 0));
    }

    private IEnumerator __DoAction(string aimUid, int tid, int isWin)
    {
        GrabTreasure_RobMagicResult_Requester tmpRequester = new GrabTreasure_RobMagicResult_Requester();
        tmpRequester.AimUid = aimUid;
        tmpRequester.Tid = tid;
        tmpRequester.IsWin = isWin;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
            __RetSuccess(aimUid, tid, isWin, tmpRequester.respMsg);
    }

    private void __RetSuccess(string aimUid, int tid, int isWin, SC_RobMagicResult resp)
    {
        GrabTreasureResultData data = new GrabTreasureResultData();
        RoleData tmpMyRoleData = RoleLogicData.GetMainRole();
        data.mIsWin = (isWin == 1) ? true : false;
        data.mFragId = tid;
        data.mPreRoleGold = RoleLogicData.Self.gold;
        data.mPreRoleExp = tmpMyRoleData.exp;
        data.mPreRoleLevel = tmpMyRoleData.level;
        data.mResult = resp;
        data.mGetGold = resp.gold;
        data.mGetExp = resp.exp;

        //精力改在结算时扣除
        int tmpCostSpirit = 2;
        RoleLogicData.Self.AddSpirit(-tmpCostSpirit);

        //加物品
        for (int i = 0, count = resp.allAddItems.Length; i < count; i++)
        {
            if (resp.allAddItems[i].itemId == -1)
                PackageManager.AddItem(resp.allAddItems[i]);
            else
                PackageManager.UpdateItem(resp.allAddItems[i]);
        }

        data.mPostRoleGold = RoleLogicData.Self.gold;
        data.mPostRoleExp = tmpMyRoleData.exp;
        data.mPostRoleLevel = tmpMyRoleData.level;

        string winName = (isWin == 1) ? "GRABTREASURE_BATTLE_RESULT_WIN_WINDOW" : "GRABTREASURE_BATTLE_RESULT_LOSE_WINDOW";
        DataCenter.OpenWindow(winName, data);
    }
}
