using UnityEngine;
using System.Collections;
using Logic;
using System.Diagnostics;
using System;

//---------------------------------------------------------
public enum TEAM_PAGE_TYPE
{
    TEAM = 1,
    PET_PACKAGE,
    PET_FRAGMENT_PACKAGE,
}

public class TeamWindow : tWindow
{

    private TEAM_PAGE_TYPE mCurTeamPageType = TEAM_PAGE_TYPE.TEAM;
    //by chenliang
    //begin

    private bool mIsOpened = false;         //是否打开了窗口

    //end

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_team_button_1", new DefineFactory<TeamPageEvent>());
        EventCenter.Self.RegisterEvent("Button_team_button_2", new DefineFactory<TeamPageEvent>());
        EventCenter.Self.RegisterEvent("Button_team_button_3", new DefineFactory<TeamPageEvent>());

        EventCenter.Self.RegisterEvent("Button_team_window_back_btn", new DefineFactory<Button_TeamWindowBack>());
    }

    public override void Open(object param)
    {
        //by chenliang
        //begin

//         if (param.GetType() == typeof(bool))
//         {
//             Stopwatch stopwatch = new Stopwatch();
//             stopwatch.Reset();
//             stopwatch.Start();
//             //DEBUG.Log("team window total start Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
//             base.Open(param);
// 
//             tWindow window;
// 
//             window = DataCenter.GetData("TEAM_INFO_WINDOW") as tWindow;
//             if (null == window.mGameObjUI)
//             {
//                 DataCenter.SetData("TEAM_INFO_WINDOW", "OPEN", false);
//             }
// 
//             window = DataCenter.GetData("TEAM_PET_PACKAGE_WINDOW") as tWindow;
//             if (null == window.mGameObjUI)
//             {
//                 DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "OPEN", false);
//             }
// 
//             window = DataCenter.GetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW") as tWindow;
//             if (null == window.mGameObjUI)
//             {
//                 DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW", "OPEN", false);
//             }
// 
//             mGameObjUI.SetActive(false);
//         }
//         else if (param.GetType() == typeof(TEAM_PAGE_TYPE))
//------------------
        //不进行预加载

        if (mGameObjUI == null)
        {
            mIsPreloadComplete = false;
        }

        base.Open(param);
        mIsOpened = true;

        //异步打开窗口
        GlobalModule.DoCoroutine(__AsyncOpenPreload(param));
        return;

        tWindow window;

        window = DataCenter.GetData("TEAM_INFO_WINDOW") as tWindow;
        if (null == window.mGameObjUI)
        {
            DataCenter.SetData("TEAM_INFO_WINDOW", "OPEN", false);
        }

        window = DataCenter.GetData("TEAM_PET_PACKAGE_WINDOW") as tWindow;
        if (null == window.mGameObjUI)
        {
            DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "OPEN", false);
        }

        window = DataCenter.GetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW") as tWindow;
        if (null == window.mGameObjUI)
        {
            DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW", "OPEN", false);
        }

        if (param.GetType() == typeof(TEAM_PAGE_TYPE))

        //end
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            //DEBUG.Log("team window total start Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            base.Open(param);

            mCurTeamPageType = (TEAM_PAGE_TYPE)param;
            //DEBUG.Log("team window start1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamLeftUI();
            //DEBUG.Log("team window end1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            //DEBUG.Log("team window start2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamRightUI();
            //DEBUG.Log("team window end2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            DataCenter.OpenWindow("BACK_GROUP_TEAM_WINDOW");
            //DEBUG.Log("team window total end Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
        }

        //added by xuke 红点逻辑
        TeamNewMarkManager.Self.CheckTeamNewMark();
        TeamNewMarkManager.Self.RefreshTeam_AllNewMark();
        //end
    }
    //by chenliang
    //begin

    private static Action msWindowInstanceComplete = null;      //窗口实例化完成
    public static void AddWinInstanceComplete(Action callback)
    {
        msWindowInstanceComplete += callback;
    }
    public static void RemoveWindowInstanceComoplete(Action callback)
    {
        msWindowInstanceComplete -= callback;
    }
    private bool mIsPreloadComplete = false;        //是否预加载成功
    private TEAM_PAGE_TYPE mRequestForPageType;     //请求加载的页签类型
    private bool mHasRequestedUpdateUI = false;     //是否请求了刷新界面（非预加载）

    /// <summary>
    /// 异步打开窗口
    /// </summary>
    /// <returns></returns>
    private IEnumerator __AsyncOpen(object param)
    {
        //最先打开返回按钮
        DataCenter.OpenWindow("BACK_GROUP_TEAM_WINDOW");

        tWindow window;

        TeamInfoWindow tmpInfoWin = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
        if (null == tmpInfoWin.mGameObjUI)
        {
            if (!mIsOpened)
                yield break;
            else
                yield return null;
            bool tmpContinue = false;
            tmpInfoWin.CanClose = false;
            if (tmpInfoWin != null)
                tmpInfoWin.OpenCompleteCallback = () =>
                {
                    tmpContinue = true;
                };
            DataCenter.SetData("TEAM_INFO_WINDOW", "OPEN", false);
            while (!tmpContinue)
                yield return null;
        }

        window = DataCenter.GetData("TEAM_PET_PACKAGE_WINDOW") as tWindow;
        if (null == window.mGameObjUI)
        {
            if (!mIsOpened)
                yield break;
            else
                yield return null;
            DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "OPEN", false);
        }

        window = DataCenter.GetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW") as tWindow;
        if (null == window.mGameObjUI)
        {
            if (!mIsOpened)
                yield break;
            else
                yield return null;
            DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW", "OPEN", false);
        }

        if (param.GetType() == typeof(TEAM_PAGE_TYPE))
        {
            mCurTeamPageType = (TEAM_PAGE_TYPE)param;
            //DEBUG.Log("team window start1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamLeftUI();
            //DEBUG.Log("team window end1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            //DEBUG.Log("team window start2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamRightUI();
            //DEBUG.Log("team window end2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
        }
        tmpInfoWin.CanClose = true;

        TeamNewMarkManager.Self.CheckTeamNewMark();
        TeamNewMarkManager.Self.RefreshTeam_AllNewMark();
    }
    /// <summary>
    /// 异步打开窗口
    /// </summary>
    /// <returns></returns>
    private IEnumerator __AsyncOpenPreload(object param)
    {
        bool tmpIsForPreload = false;
        if (param.GetType() == typeof(bool))
        {
            tmpIsForPreload = true;
            mIsPreloadComplete = false;

            mGameObjUI.SetActive(false);

            tWindow window;

            TeamInfoWindow tmpInfoWin = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
            if (null == tmpInfoWin.mGameObjUI)
            {
                if (!mIsOpened)
                    yield break;
                else
                    yield return null;
                bool tmpContinue = false;
                if (tmpInfoWin != null)
                    tmpInfoWin.OpenCompleteCallback = () =>
                    {
                        tmpContinue = true;
                    };
                DataCenter.SetData("TEAM_INFO_WINDOW", "OPEN", false);
                tmpInfoWin.mGameObjUI.SetActive(false);
                while (!tmpContinue)
                    yield return null;
            }

            window = DataCenter.GetData("TEAM_PET_PACKAGE_WINDOW") as tWindow;
            if (null == window.mGameObjUI)
            {
                if (!mIsOpened)
                    yield break;
                else
                    yield return null;
                DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "OPEN", false);
                window.mGameObjUI.SetActive(false);
            }

            window = DataCenter.GetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW") as tWindow;
            if (null == window.mGameObjUI)
            {
                if (!mIsOpened)
                    yield break;
                else
                    yield return null;
                DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW", "OPEN", false);
                window.mGameObjUI.SetActive(false);
            }

            yield return null;
            if (msWindowInstanceComplete != null)
                msWindowInstanceComplete();

            mIsPreloadComplete = true;
        }
        else if (param.GetType() == typeof(TEAM_PAGE_TYPE))
        {
            mRequestForPageType = (TEAM_PAGE_TYPE)param;
            mHasRequestedUpdateUI = true;
            if (!mIsPreloadComplete)
                yield break;

            mCurTeamPageType = (TEAM_PAGE_TYPE)param;
            //DEBUG.Log("team window start1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamLeftUI();
            //DEBUG.Log("team window end1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            //DEBUG.Log("team window start2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            UpdateTeamRightUI();
            //DEBUG.Log("team window end2 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            DataCenter.OpenWindow("BACK_GROUP_TEAM_WINDOW");
        }

        //added by xuke 反馈文字
        if (ChangeTipManager.Self.mGridContainer == null)
        {
            ChangeTipManager.Self.mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "change_tip_grid");
        }
        //end

        TeamNewMarkManager.Self.CheckTeamNewMark();
        TeamNewMarkManager.Self.RefreshTeam_AllNewMark();

        //判断是否需要打开窗口
        if (tmpIsForPreload && mHasRequestedUpdateUI && mIsOpened)
            Open(mRequestForPageType);
    }

    //end

    public override void OnOpen()
    {
        base.OnOpen();


    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("SHOW_WINDOW" == keyIndex)
        {
            if ((int)mCurTeamPageType == (int)objVal)
                return;

            mCurTeamPageType = (TEAM_PAGE_TYPE)objVal;
            GlobalModule.DoOnNextUpdate(ShowWindow);            
        }
        else if ("REFRESH_NEW_MARK" == keyIndex) 
        {
            RefreshTeamNewMark();
        }
    }

    private void RefreshTeamNewMark() 
    {
        UIGridContainer _petIconGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_icon_info_buttons_grid");
        if (_petIconGrid == null)
            return;
       
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++) 
        {
            //1.刷新所有符灵头像的红点
            PetNewMarkData _petNewMarkData = TeamNewMarkManager.Self.GetPetNewMarkData(i);
            if (_petNewMarkData != null && _petIconGrid.controlList.Count > i)
            {
                GameCommon.SetNewMarkVisible(_petIconGrid.controlList[i], _petNewMarkData.PetIconVisible);
            }

            //2.刷新所有装备的红点
            UIGridContainer _teamInfoGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_info_grid");
            if (_teamInfoGrid == null)
                return;
            if (_teamInfoGrid.controlList.Count <= i)
                continue;
            UIGridContainer _equipContainer = GameCommon.FindComponent<UIGridContainer>(_teamInfoGrid.controlList[i], "equip_info_grid");
            for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; j++)
            {
                EquipNewMarkData _equipNewMarkData = TeamNewMarkManager.Self.GetEquipNewMarkData(i,j,ITEM_TYPE.EQUIP);
                if(_equipNewMarkData == null)
                    continue;
                GameCommon.SetNewMarkVisible(_equipContainer.controlList[j], _equipNewMarkData.EquipVisible);
            }
            UIGridContainer _magicContainer = GameCommon.FindComponent<UIGridContainer>(_teamInfoGrid.controlList[i], "magic_info_grid");
            for (int k = (int)MAGIC_TYPE.MAGIC1; k < (int)MAGIC_TYPE.MAX; k++) 
            {
                EquipNewMarkData _euqipNewMarkData = TeamNewMarkManager.Self.GetEquipNewMarkData(i,k,ITEM_TYPE.MAGIC);
                if(_euqipNewMarkData == null)
                    continue;
                GameCommon.SetNewMarkVisible(_magicContainer.controlList[k], _euqipNewMarkData.EquipVisible);
            }
        }    
        //3.刷新标签页的红点
        GameObject _teamBtnObj_1 = GameCommon.FindObject(mGameObjUI, "team_button_1");
        GameCommon.SetNewMarkVisible(_teamBtnObj_1, TeamNewMarkManager.Self.mTab1NewMarkData.TabVisible);

        GameObject _teamBtnObj_2 = GameCommon.FindObject(mGameObjUI, "team_button_2");
        GameCommon.SetNewMarkVisible(_teamBtnObj_2, TeamNewMarkManager.Self.mTab2NewMarkData.TabVisible);

        GameObject _teamBtnObj_3 = GameCommon.FindObject(mGameObjUI, "team_button_3");
        GameCommon.SetNewMarkVisible(_teamBtnObj_3, TeamNewMarkManager.Self.mTab3NewMarkData.TabVisible);

        //4.刷新缘分红点
        GameObject _relatePosBtnObj = GameCommon.FindObject(mGameObjUI, "karma_button");
        GameCommon.SetNewMarkVisible(_relatePosBtnObj,TeamNewMarkManager.Self.mTab1NewMarkData.RelatePosVisible);
    }

    public void UpdateTeamLeftUI()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        DEBUG.Log("UpdateTeamLeftUI start1 Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
        GameCommon.ToggleTrue(GetSub("team_button_" + ((int)mCurTeamPageType).ToString()));
        GlobalModule.DoOnNextUpdate(ShowWindow);
        //ShowWindow();
        DEBUG.Log("UpdateTeamLeftUI end Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
    }

    public void UpdateTeamRightUI()
    {
        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
        //DataCenter.OpenWindow("EQUIP_INFO_WINDOW");
    }

    public void ShowWindow()
    {
        CloseAllWindow();
        switch (mCurTeamPageType)
        {
            case TEAM_PAGE_TYPE.TEAM:
                DataCenter.OpenWindow("TEAM_INFO_WINDOW");
                GetUIToggle("team_button_1").value=true;
			GameCommon.SetUIVisiable(mGameObjUI ,"Label_no_pet_fragment_tips",false);
                break;
            case TEAM_PAGE_TYPE.PET_PACKAGE:
                DataCenter.OpenWindow("TEAM_PET_PACKAGE_WINDOW");
                GetUIToggle("team_button_2").value=true;
                break;
            case TEAM_PAGE_TYPE.PET_FRAGMENT_PACKAGE:
                DataCenter.OpenWindow("TEAM_PET_FRAGMENT_PACKAGE_WINDOW");
                GetUIToggle("team_button_3").value=true;
                break;
        }
    }

    public void CloseAllWindow()
    {
        CloseAllLeftWindow();
        CloseAllRightWindow();
    }

    public void CloseAllLeftWindow()
    {
        DataCenter.CloseWindow("TEAM_INFO_WINDOW");
        DataCenter.CloseWindow("TEAM_PET_PACKAGE_WINDOW");
        DataCenter.CloseWindow("TEAM_PET_FRAGMENT_PACKAGE_WINDOW");
    }

    public void CloseAllRightWindow()
    {
        TeamInfoWindow.CloseAllWindow();
    }

    public override void Close()
    {
        base.Close();
        DataCenter.CloseWindow("BACK_GROUP_TEAM_WINDOW");
        DataCenter.CloseWindow("TEAM_INFO_WINDOW");
        TeamInfoWindow.CloseAllWindow();
        //by chenliang
        //begin

        mIsOpened = false;
        mHasRequestedUpdateUI = false;

        //end
    }
}

public class TeamPageEvent : CEvent
{
    public override bool _DoEvent()
    {
        string[] names = GetEventName().Split('_');
        DataCenter.SetData("TEAM_WINDOW", "SHOW_WINDOW", int.Parse(names[names.Length - 1]));
        return true;
    }
}

//----------------------------------------------------------------------------------
// team window back
public class Button_TeamWindowBack : CEvent
{
    public override bool _DoEvent()
    {
        if (MainUIScript.Self.mWindowBackAction != null)
        {
            MainUIScript.Self.mWindowBackAction();
            MainUIScript.Self.mWindowBackAction = null;
        }
        else
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        }

        DataCenter.CloseWindow("TEAM_WINDOW");
		TeamInfoWindow.saveTeamAttribute.Clear ();
        return true;
    }
}