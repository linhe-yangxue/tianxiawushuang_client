using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 历练界面
/// </summary>
public class TrialWindow : tWindow
{
    enum eStyle
    {
        MAP_BOSS = 1401,
        GRAB_TREASURE = 1403,
        RAMMBOCK = 1402,
        FAIRYLAND = 1404,
        PEAK_PVP = 1405
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        SetTrialDescriptionInfo();           
    }

    private void SetTrialDescriptionInfo() 
    {
        if (mGameObjUI == null)
            return;
        //1.竞技场描述
        GameObject peak_pvp = GetSub("peak_pvp_enter_PVPBtn");
        int openLevel = GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.PEAK_PVP);
        SetDesInfo(peak_pvp, openLevel, TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_PVP));
        //2.夺宝描述
        GameObject grab_treasure = GetSub("grabtreasure_enter_PVPBtn");
        openLevel = GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.GRAB_TREASURE);
        SetDesInfo(grab_treasure, openLevel, TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_GRAB_TREASURE));
        //3.封灵塔描述
        GameObject rammbock = GetSub("rammbock_enter_PVPBtn");
        openLevel = GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.RAMMBOCK);
        SetDesInfo(rammbock, openLevel, TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_RAMMBOCK));
        //4.寻仙描述
        GameObject fairyland = GetSub("fairyland_enter_PVEBtn");
        openLevel = GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.FAIRYLAND);
        SetDesInfo(fairyland, openLevel, TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_FAIRYLAND));
        //5.天魔描述
        GameObject map_boss = GetSub("map_boss_list");
        openLevel = GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.MAP_BOSS);
        SetDesInfo(map_boss, openLevel, TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_FEAST));
    }

    private void SetDesInfo(GameObject kObj,int openLevel, string kDescription,string kLblName = "description_label")
    {
        if (kObj == null)
            return;
        UILabel _label = GameCommon.FindComponent<UILabel>(kObj,kLblName);
        if (_label == null)
            return;
        _label.text = kDescription;

        UILabel _numlabel = GameCommon.FindObject(kObj, "number").GetComponent<UILabel>();
        if(_numlabel != null)
        {
            string temp = TableCommon.getStringFromStringList(STRING_INDEX.TRIAL_DESCRIPTION_NUMBERLABEL);
            string levelDes = string.Format(temp, openLevel);
            _numlabel.text = levelDes;
        }
    }

    public override void Open(object param)
    {
        base.Open(param);
        MainUIScript.Self.HideMainBGUI();

        //红点逻辑 begin
        TrialNewMarkManager.Self.CheckTrial_NewMarkAll();
        //end
        //by chenliang
        //begin

        //打开快捷入口
        DataCenter.OpenWindow("TRIAL_EASY_JUMP_WINDOW");

        //end
    }

    public override void Close()
    {
        //by chenliang
        //begin

        //关闭快捷入口
        DataCenter.CloseWindow("TRIAL_EASY_JUMP_WINDOW");

        //end
        MainUIScript.Self.ShowMainBGUI();
        base.Close();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "REFRESH_NEWMARK":
                RefreshNewMark();
                RefreshTitle();
                break;
            //by chenliang
            //begin

            case "REFRESH_EASY_JUMP":
                {
                    DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
                }break;

            //end
        }
    }

    private void RefreshNewMark() 
    {
        if (mGameObjUI == null)
            return;
        GameObject _gridObj = GameCommon.FindObject(mGameObjUI,"entrance_info_grid");
        if (_gridObj == null)
            return;
        GameObject _pvpBtnObj = GameCommon.FindObject(_gridObj, "peak_pvp_enter_PVPBtn");            //> 巅峰挑战
        GameObject _rammbockObj = GameCommon.FindObject(_gridObj, "rammbock_enter_PVPBtn");          //> 封灵塔
        GameObject _grabTreasureObj = GameCommon.FindObject(_gridObj, "grabtreasure_enter_PVPBtn");  //> 夺宝
        GameCommon.SetNewMarkVisible(_pvpBtnObj,PVPNewMarkManager.Self.PVPVisible);
        GameCommon.SetNewMarkVisible(_rammbockObj,RammbockNewMarkManager.Self.RammbockVisible);
        GameCommon.SetNewMarkVisible(_grabTreasureObj,GrabTreasureNewMarkManager.Self.GrabTreasureVisible);
    
        
    }

    /// <summary>
    /// 刷新标题
    /// </summary>
    private void RefreshTitle()
    {
        if (mGameObjUI == null)
            return;
        GameObject _gridObj = GameCommon.FindObject(mGameObjUI, "entrance_info_grid");
        if (_gridObj == null)
            return;
        GameObject _pvpBtnObj = GameCommon.FindObject(_gridObj, "peak_pvp_enter_PVPBtn");            //> 巅峰挑战 1405
        GameObject _grabTreasureObj = GameCommon.FindObject(_gridObj, "grabtreasure_enter_PVPBtn");  //> 夺宝 1403
        GameObject _rammbockObj = GameCommon.FindObject(_gridObj, "rammbock_enter_PVPBtn");          //> 封灵塔 1402
        GameObject _fairylandObj = GameCommon.FindObject(_gridObj,"fairyland_enter_PVEBtn");         //> 寻仙 1404
        GameObject _feastObj = GameCommon.FindObject(_gridObj,"map_boss_list");                      //> 天魔降临 1401

        UpdateSpecifyTile(_pvpBtnObj, GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.PEAK_PVP));
        UpdateSpecifyTile(_grabTreasureObj, GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.GRAB_TREASURE));
        UpdateSpecifyTile(_rammbockObj, GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.RAMMBOCK));
        UpdateSpecifyTile(_fairylandObj, GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.FAIRYLAND));
        UpdateSpecifyTile(_feastObj, GameCommon.GetFuncCanUseLevelByFuncID((int)eStyle.MAP_BOSS));
    }

    private void UpdateSpecifyTile(GameObject kObj, int kOPenLevel, string kTitleName = "title_sprite") 
    {
        if (kObj == null)
            return;
        UISprite _titleSprite = GameCommon.FindComponent<UISprite>(kObj, "title_sprite");
        if (_titleSprite == null)
            return;
        UILabel _titleLbl = GameCommon.FindComponent<UILabel>(_titleSprite.gameObject, "Label");
        if (_titleLbl == null)
            return;
        if (RoleLogicData.Self.character.level >= kOPenLevel)
        {
            _titleSprite.spriteName = "a_ui_title_1";
            _titleLbl.gradientTop = new Color(1f, 1f, 1f);
            _titleLbl.gradientBottom = new Color(1f, 249.0f/255.0f, 202.0f/255.0f);
        }
        else 
        {
            _titleSprite.spriteName = "a_ui_title_3";
            float _topColor = 220.0f / 255.0f;
            float _bottomColor = 168.0f / 255.0f;
            _titleLbl.gradientTop = new Color(_topColor, _topColor, _topColor);
            _titleLbl.gradientBottom = new Color(_bottomColor, _bottomColor, _bottomColor);
        }

        //show limitnumtips
        ShowLimitNumTips(kObj, "number", RoleLogicData.Self.character.level >= kOPenLevel ? false : true);
    }

    public void ShowLimitNumTips(GameObject obj, string name, bool isShow)
    {
        GameObject label = GameCommon.FindObject(obj, name);
        if (label)
        {
            label.SetActive(isShow);
        }
    }
}
