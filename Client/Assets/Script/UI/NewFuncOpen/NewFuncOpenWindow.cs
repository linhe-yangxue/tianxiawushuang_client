using UnityEngine;
using System.Collections;
using DataTable;
using System;
using System.Collections.Generic;
using Logic;

/// <summary>
/// 新功能开启弹窗
/// </summary>
public class NewFuncOpenWindow : tWindow 
{
    //private GET_PARTH_TYPE mCurEnterType = GET_PARTH_TYPE.NONE;
    private UIGridContainer _gridContainer = null;
    public static float mWaitTime = 0.8f;    //> 弹出窗口延迟时间
    public static bool mbNeedBreak = false;
    public override void Init() 
    {
        //RoleLogicData.mOnMainRoleLevelUp += ShowGoToNewFuncWin;
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        _gridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "func_grid");

        AddButtonAction("new_func_close_mask", () => { Close();});
    }

    /// <summary>
    /// 判断是否需要打开新功能开启窗口
    /// </summary>
    /// <param name="kFuncID"></param>
    public static void ShowGoToNewFuncWin(int kLevel)
    {
        // 当新手引导不处于活动状态中时，才打开升级窗口
        //if (!Guide.isActive)
        {
            DataCenter.OpenWindow("NEW_FUNC_OPEN_WINDOW", kLevel);
        }
    }
    //public static bool CheckWaitFlag = false;
    //private static bool NeedWaitShow() 
    //{
    //    //1.如果需要显示星相石碎片窗口则等待星相石碎片窗口关闭后再显示
    //    StageBattle _stageBattle = MainProcess.mStage;
    //    if (/*mDramaParam != null && */_stageBattle != null && _stageBattle.IsMainPVE() && _stageBattle.mStageProperty != null && !_stageBattle.mStageProperty.passed)
    //    {
    //        string astrologyTip = _stageBattle.mDramaParam.GetString("astrology_tip");

    //        if (!string.IsNullOrEmpty(astrologyTip))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public static void SetOpenFlagInfo() 
    {
        mbNeedBreak = true;
        GlobalModule.DoLater(() => 
        {
            mbNeedBreak = false;
        },mWaitTime);
    }

    public override void Open(object param)
    {
        base.Open(param);
        //GlobalModule.DoLater(() =>
        //{
        //    //Button_stage_info_clean_multi.stop = true;
        //    //Button_stage_info_clean_multi.forcedStop = true;
        //    //DataCenter.CloseWindow("SWEEP_LIST_WINDOW");
        //    //if (GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window") != null)
        //    //    MonoBehaviour.Destroy(GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window"));
        //}, 0.5f);
        if (param != null && param is int)
        {
            int _curLevel = (int)param;
            //1.设置等级,体力,精力信息
            SetLevelInfo(_curLevel);
            SetStaminaInfo();
            SetSpiritInfo();
            //2.设置功能描述
            SetFuncInfo();
        }

    }
    private void SetLevelInfo(int kLevel) 
    {
        UILabel _preLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "pre_level");
        UILabel _curLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "cur_level");

        if (_preLbl != null) 
        {
            _preLbl.text = (kLevel - 1).ToString();        
        }
        if (_curLbl != null)
        {
            _curLbl.text = kLevel.ToString();
        }
    }
    private void SetStaminaInfo() 
    {
        UILabel _preStaminaLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "pre_stamina");
        UILabel _curStaminaLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "cur_stamina");

        if (_preStaminaLbl != null) 
        {
            _preStaminaLbl.text = RoleLogicData.Self.preStamina.ToString();
        }
        if (_curStaminaLbl != null) 
        {
            _curStaminaLbl.text = RoleLogicData.Self.stamina.ToString();
        }
    }
    private void SetSpiritInfo() 
    {
        UILabel _preSpiritLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "pre_spirit");
        UILabel _curSpiritLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "cur_spirit");

        if (_preSpiritLbl != null)
        {
            _preSpiritLbl.text = RoleLogicData.Self.preSpirit.ToString();
        }
        if (_curSpiritLbl != null) 
        {
            _curSpiritLbl.text = RoleLogicData.Self.spirit.ToString();
        }
    }
    /// <summary>
    /// 获得当前等级需要展示的功能ID数组
    /// </summary>
    /// <returns></returns>
    private string[] GetCurLevelShowFunc() 
    {
        DataRecord _record = DataCenter.mCharacterLevelExpTable.GetRecord(RoleLogicData.Self.character.level);
        if (_record == null) 
        {
            DEBUG.LogError("等级" + RoleLogicData.Self.character.level + "没有记录");
            return null;        
        }
        string _funcIDInfo = _record.getData("LEVEL_NOTICE");
        return _funcIDInfo.Split('|');
    }
    /// <summary>
    /// 设置功能描述
    /// </summary>
    private void SetFuncInfo() 
    {
        string[] _funcIDArr = GetCurLevelShowFunc();
        if (_funcIDArr == null)
        {
            DEBUG.LogError("mCharacterLevelExpTable: LEVEL_NOTICE 为空 或者 对应等级没有记录");
            return;
        }
        if (_gridContainer == null)
            return;
        _gridContainer.MaxCount = _funcIDArr.Length;
        int _funcID = 0;
        for (int i = 0, count = _funcIDArr.Length; i < count; i++) 
        {
            int.TryParse(_funcIDArr[i], out _funcID);
            InitFuncItem(_gridContainer.controlList[i],_funcID,i);
        }
    }
    private float mFirstWaitTime = 0.3f;    //> 起始等待时间
    private float mStepWaitTime = 0.35f;         //> 当有多个功能开启时，需要依次动态添加相应图标的间隔时间

    /// <summary>
    /// 初始化功能描述
    /// </summary>
    /// <param name="kItem"></param>
    /// <param name="kFuncID"></param>
    private void InitFuncItem(GameObject kItem,int kFuncID,int kGridIndex) 
    {
        //1.设置功能图标
        UISprite _funcIcon = GameCommon.FindComponent<UISprite>(kItem, "func_sprite");
        if (_funcIcon != null) 
        {
            _funcIcon.atlas = GameCommon.LoadUIAtlas(TableCommon.GetStringFromFunctionConfig(kFuncID, "ICON_ATLAS_NAME"));
            _funcIcon.spriteName = TableCommon.GetStringFromFunctionConfig(kFuncID, "ICON_SPRITE_NAME");
        }
        //2.设置功能描述
        UILabel _funcLbl = GameCommon.FindComponent<UILabel>(kItem, "func_description");
        if (_funcLbl != null) 
        {
            _funcLbl.text = TableCommon.GetStringFromFunctionConfig(kFuncID, "DESC");
        }
        GameObject _openLogoObj = GameCommon.FindObject(kItem, "func_open_logo");
        GameObject _goToBtnObj = GameCommon.FindObject(kItem, "new_func_open_btn");
        GameObject _openLevelObj = GameCommon.FindObject(kItem, "open_level_label");
        if (_openLogoObj == null || _goToBtnObj == null || _openLevelObj == null) 
        {
            return;
        }

        //3.判断当前功能是否开启
        int _openLevel = GameCommon.GetFuncCanUseLevelByFuncID(kFuncID);
        if (RoleLogicData.Self.character.level > _openLevel)
        {
            _openLogoObj.SetActive(false);
            _goToBtnObj.SetActive(true);
            _openLevelObj.SetActive(false);

            GET_PARTH_TYPE _type = GetTypeByFuncID(kFuncID);
            //mCurEnterType = _type;
            AddButtonAction(_goToBtnObj, () =>
            {
                Action _action = null;
                if (GetPathHandlerDic.HandlerDic.TryGetValue(_type, out _action))
                {
                    if (CloseCurWin(_type))   //> 如果直接跳转
                        _action();
                }
                if (_action == null)
                    DEBUG.LogError("No this type in dic:(_type):" + _type.ToString());
            });
        }
        else if (RoleLogicData.Self.character.level == _openLevel)
        {
            GlobalModule.DoLater(() => { _openLogoObj.SetActive(true); }, mFirstWaitTime + kGridIndex * mStepWaitTime);
            _goToBtnObj.SetActive(true);
            _openLevelObj.SetActive(false);

            GET_PARTH_TYPE _type = GetTypeByFuncID(kFuncID);
            //mCurEnterType = _type;
            AddButtonAction(_goToBtnObj, () =>
            {
                Action _action = null;
                if (GetPathHandlerDic.HandlerDic.TryGetValue(_type, out _action))
                {
                    if (CloseCurWin(_type))   //> 如果直接跳转
                        _action();
                }
                if (_action == null)
                    DEBUG.LogError("No this type in dic:(_type):" + _type.ToString());
            });
        }
        else 
        {
            _openLogoObj.SetActive(false);
            _goToBtnObj.SetActive(false);
            _openLevelObj.SetActive(true);
            UILabel _openLevelLbl = _openLevelObj.GetComponent<UILabel>();
            if (_openLevelLbl != null) 
            {
                _openLevelLbl.text = "等级"+_openLevel+"开启";
            }
        }
    }

    //关闭当前窗口
    private bool CloseCurWin(GET_PARTH_TYPE kCurEnterType) 
    {
        bool isDirect = true;   //> 判断是否关闭窗口后直接调用跳转
        Close();
        //当前在主界面
        FUNC_ENTER_INDEX _enterType = (FUNC_ENTER_INDEX)((int)DataCenter.Self.getObject("FUNC_ENTER_INDEX"));
        if (FUNC_ENTER_INDEX.MAIL == _enterType || FUNC_ENTER_INDEX.TASK == _enterType) {
			DataCenter.CloseWindow ("TASK_WINDOW");
			MainUIScript.Self.ShowMainBGUI ();
			MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		}
        //在扫荡界面
        else if (FUNC_ENTER_INDEX.CLEAN_OUT == _enterType) {
			DataCenter.CloseWindow ("STAGE_INFO_WINDOW");
			DataCenter.CloseWindow ("PVE_ACCOUNT_CLEAN_WINDOW");
			DataCenter.CloseWindow ("BOSS_RAID_WINDOW");
            EventCenter.Start("Button_sweep_list_close_button").DoEvent();
			//DataCenter.CloseWindow("pve_account_clean_window");  
			Logic.EventCenter.Start ("Button_world_map_back").DoEvent ();
		} else if (FUNC_ENTER_INDEX.YIJIAN_GRABTREASURE == _enterType) {
			DataCenter.CloseWindow ("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW");
			DataCenter.CloseWindow ("GRABTREASURE_WINDOW");
			DataCenter.CloseWindow("TRIAL_WINDOW");
			DataCenter.CloseWindow ("GRABTREASURE_BACK_WINDOWk");
			DataCenter.CloseWindow("TRIAL_WINDOW_BACK");
            DataCenter.CloseWindow("GRABTREASURE_FIGHT_WINDOW");
		} else if (FUNC_ENTER_INDEX.Challenge_Five == _enterType) {
			DataCenter.CloseWindow("ARENA_LIST_WINDOW");
			DataCenter.CloseWindow("ARENA_MAIN_WINDOW");
			DataCenter.CloseWindow("TRIAL_WINDOW");
			DataCenter.CloseWindow ("ARENA_WINDOW_BACK");
			DataCenter.CloseWindow("TRIAL_WINDOW_BACK");
		}
        //在夺宝战斗界面/巅峰挑战战斗界面/冒险战斗场景界面
        else if (FUNC_ENTER_INDEX.PVP == _enterType || FUNC_ENTER_INDEX.GRABTREASURE == _enterType ) 
        {
            DataCenter.CloseWindow("PEAK_RESULT_WINDOW");
            DataCenter.CloseWindow("GRABTREASURE_BATTLE_RESULT_WIN_WINDOW");
            DataCenter.CloseWindow("BOSS_RAID_WINDOW");
           
            MainProcess.ClearBattle();
            MainUIScript.mLoadingFinishAction = GetPathHandlerDic.HandlerDic[kCurEnterType];
            MainProcess.LoadRoleSelScene();
            isDirect = false;
        }
        else if( FUNC_ENTER_INDEX.ADVENTURE == _enterType)
        {
            DataCenter.CloseWindow("PEAK_RESULT_WINDOW");
            DataCenter.CloseWindow("BOSS_RAID_WINDOW");
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            MainProcess.ClearBattle();
            MainUIScript.mLoadingFinishAction = GetPathHandlerDic.HandlerDic[kCurEnterType];
            MainProcess.LoadRoleSelScene();
            isDirect = false;
        }
        else if (FUNC_ENTER_INDEX.DAILYSTAGE == _enterType)
        {
            DataCenter.CloseWindow(UIWindowString.daily_stage_main_window);
            DataCenter.CloseWindow(UIWindowString.daily_stage_main_window_back);
            DataCenter.CloseWindow("PVE_ACCOUNT_WIN_WINDOW");

            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            MainProcess.ClearBattle();
            MainUIScript.mLoadingFinishAction = GetPathHandlerDic.HandlerDic[kCurEnterType];
            MainProcess.LoadRoleSelScene();
            isDirect = false;
        }
        else if (FUNC_ENTER_INDEX.GUILDBOSS == _enterType)
        {
            DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);
            DataCenter.CloseWindow(UIWindowString.union_pk_back_window);
            DataCenter.CloseWindow("UNION_PK_WIN_WINDOW");

            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            MainProcess.ClearBattle();
            MainUIScript.mLoadingFinishAction = GetPathHandlerDic.HandlerDic[kCurEnterType];
            MainProcess.LoadRoleSelScene();
            isDirect = false;
        }
        else if (FUNC_ENTER_INDEX.PACKAGE == _enterType) 
        {
            DataCenter.CloseWindow("PACKAGE_CONSUME_WINDOW");
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        }
        //重置
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.NONE);
        return isDirect;
    }

    //根据FunctionID获得对应的GET_PATH_TYPE
    private GET_PARTH_TYPE GetTypeByFuncID(int kFuncID) 
    {
        foreach (DataRecord kRecord in DataCenter.mGainFunctionConfig.Records()) 
        {
            if (kFuncID == kRecord.getData("Function_ID")) 
            {
                return (GET_PARTH_TYPE)kRecord.getObject("Type");
            }
        }
        DEBUG.LogError("Function_ID没有对应的TYPE. Function_ID是: " + kFuncID.ToString());
        return GET_PARTH_TYPE.NONE;
    }
}

