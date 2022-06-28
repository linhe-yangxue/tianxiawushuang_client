using UnityEngine;
using System.Collections;

public class FairylandBattle : PVEStageBattle
{
    //public override void OnFinish(bool bWin)
    //{
    //    if (mbBattleFinish)
    //    {
    //        return;
    //    }

    //    OnStageFinish(bWin);

    //    if (!MainProcess.isOnCheating)
    //    {
    //        if (bWin)
    //        {
    //            GlobalModule.DoCoroutine(FairylandNetManager.RequestConquerFairylandWin());
    //        }
    //        else
    //        {
    //            FairylandConquerResultData tmpResultData = new FairylandConquerResultData()
    //            {
    //                CloseCallback = () =>
    //                {
    //                    //结算窗口关闭
    //                    MainProcess.ClearBattle();
    //                    MainProcess.LoadRoleSelScene();

    //                    GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
    //                }
    //            };
    //            DataCenter.OpenWindow("FAIRYLAND_CONQUER_RESULT_WIN_WINDOW", tmpResultData);
    //        }
    //    }
    //}

    //protected override void OnRequestResult(bool bWin)
    //{
    //    if (bWin)
    //    {
    //        GlobalModule.DoCoroutine(FairylandNetManager.RequestConquerFairylandWin());
    //    }
    //    else
    //    {
    //        FairylandConquerResultData tmpResultData = new FairylandConquerResultData()
    //        {
    //            CloseCallback = () =>
    //            {
    //                //结算窗口关闭
    //                MainProcess.ClearBattle();
    //                MainProcess.LoadRoleSelScene();
    //                DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.WORLD_MAP);

    //                GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
    //            }
    //        };
    //        DataCenter.OpenWindow("FAIRYLAND_CONQUER_RESULT_LOSE_WINDOW", tmpResultData);
    //    }
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        if (iswin)
        {
            GlobalModule.DoCoroutine(FairylandNetManager.RequestConquerFairylandWin());
        }
        else
        {
            FairylandConquerResultData tmpResultData = new FairylandConquerResultData()
            {
                CloseCallback = () =>
                {
                    //结算窗口关闭
                    MainProcess.ClearBattle();
                    MainProcess.LoadRoleSelScene();
                    DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.WORLD_MAP);

                    GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
                }
            };
            DataCenter.OpenWindow("FAIRYLAND_CONQUER_RESULT_LOSE_WINDOW", tmpResultData);
        }
    }
}
