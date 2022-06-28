using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckPackage
{
    private static CheckPackage msInstance;

    public static CheckPackage Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new CheckPackage();
            return msInstance;
        }
    }

    /// <summary>
    /// 是否可以放进背包
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private bool __CanPutInPackage(IEnumerable<PACKAGE_TYPE> types)
    {
        List<PACKAGE_TYPE> tmpFullPackage = PackageManager.GetFullPackage(types);
        if (tmpFullPackage.Count <= 0)
            return true;

        if (Guide.isActive)  // 如果触发了新手引导，则不再打开提示窗口
            return false;

        if (tmpFullPackage[0] == PACKAGE_TYPE.PET)
        {
            DataCenter.OpenMessageOkWindow("当前" + GameCommon.GetPackageName(tmpFullPackage[0]) + "背包已满", () =>
            {
                //去出售
                CloseWorldMapWindow();
                DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.PET_PACKAGE);
            },
            () =>
            {
                //去升级
                CloseWorldMapWindow();
                GetPathHandlerDic.ExecuteDelegate(GET_PARTH_TYPE.TEAM);
            });
            DataCenter.RefreshMessageOkText("去升级", "去出售");
        }
        else
        {
            DataCenter.OpenMessageWindow("当前" + GameCommon.GetPackageName(tmpFullPackage[0]) + "背包已满");
        }

        return false;
    }

    void CloseWorldMapWindow()
    {
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
        CheckCloseWindow();
        if (GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window") != null)
            MonoBehaviour.Destroy(GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window"));
        if (GameCommon.FindObject(GameObject.Find("CenterAnchor"), "new_func_open_window") != null)
            MonoBehaviour.Destroy(GameCommon.FindObject(GameObject.Find("CenterAnchor"), "new_func_open_window"));
    }

    private bool CheckWinOpen(string kWinKeyName) 
    {
        object _winObj = DataCenter.Self.getObject(kWinKeyName);
        if (_winObj == null)
            return false;
        tWindow _tWin = _winObj as tWindow;
        return _tWin.mGameObjUI != null && _tWin.mGameObjUI.activeInHierarchy;
    }
    private void CheckCloseWindow() 
    {
        //邮件
        if (CheckWinOpen("MAIL_WINDOW")) 
        {
            Logic.EventCenter.Start("Button_mail_back").DoEvent();
        }
        //队伍界面-符灵碎片页签
        else if (CheckWinOpen("TEAM_PET_FRAGMENT_PACKAGE_WINDOW")) 
        {
            Logic.EventCenter.Start("Button_team_window_back_btn").DoEvent();
        }
        //符灵商铺
        else if (CheckWinOpen("NEW_MYSTERIOUS_SHOP_WINDOW")) 
        {
            Logic.EventCenter.Start("Button_mystery_window_back").DoEvent(); ;
        }
        //包裹
        else if (CheckWinOpen("PACKAGE_CONSUME_WINDOW")) 
        {
            Logic.EventCenter.Start("Button_package_consume_close").DoEvent();
        }
		//任务成就
        else if (CheckWinOpen("TASK_WINDOW"))
		{
			Logic.EventCenter.Start("Button_task_back_btn").DoEvent();
		}
        //天魔战绩奖励界面
        else if (CheckWinOpen("BOSS_AWARD_WINDOW")) 
        {
            DataCenter.CloseWindow("BOSS_AWARD_WINDOW");
            Logic.EventCenter.Start("Button_boss_world_map_back").DoEvent();
            Logic.EventCenter.Start("Button_trial_window_back_btn").DoEvent();
        }

        //十连抽
        else if (CheckWinOpen("shop_gain_item_window"))
        {
            Logic.EventCenter.Start("Button_shop_gain_item_window_close_btn").DoEvent();
            Logic.EventCenter.Start("Button_shop_window_back").DoEvent();
        }
        //商城获得符灵界面 单抽
        else if (CheckWinOpen("shop_gain_pet_window"))
        {
            Logic.EventCenter.Start("Button_shop_gain_pet_window_close_btn").DoEvent();
            Logic.EventCenter.Start("Button_shop_window_back").DoEvent();
        }
        //商城
        else if (CheckWinOpen("SHOP_WINDOW"))
        {
            Logic.EventCenter.Start("Button_shop_window_back").DoEvent();
        }
    }

    /// <summary>
    /// 是否可以进入关卡
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanEnterBattle(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以购买物品
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanAddItems(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以合成物品
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanCompose(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以开宝箱
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanOpenChest(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以领取邮件
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanGetEmail(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以进行任务
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanStartMission(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以进行任务
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanGetStageTaskReward(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以进行活动
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanEnterActivity(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以进入每日副本
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanStartDailyTask(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以夺宝
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanGrabTreasure(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以寻仙
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanExploreFairyland(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以进入竞技场
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanEnterPVPBattle(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
    /// <summary>
    /// 是否可以挑战天魔
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public bool CanFightBoss(IEnumerable<PACKAGE_TYPE> types)
    {
        return __CanPutInPackage(types);
    }
}
