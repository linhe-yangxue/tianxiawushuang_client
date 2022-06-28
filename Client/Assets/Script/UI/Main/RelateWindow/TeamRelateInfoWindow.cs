using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Logic;


public class TeamRelateInfoWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_karma_effect_Button", new DefineFactory<Button_karma_effect_Button>());
        EventCenter.Self.RegisterEvent("Button_karma_btn", new DefineFactory<Button_karma_btn>());
        EventCenter.Self.RegisterEvent("Button_add_karma_btn", new DefineFactory<Button_add_karma_btn>());
        EventCenter.Self.RegisterEvent("Button_pet_change_Button", new DefineFactory<Button_pet_change_Button>());
        EventCenter.Self.RegisterEvent("Button_pet_demount_Button", new DefineFactory<Button_pet_demount_Button>());          
    }

    public override void Open(object param)
    {
        base.Open(param);
        AppendDelegate<int, HashSet<int>, HashSet<int>>(Relationship.onRelationChanged, OnRelationChanged);
        Refresh(param);
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("TEAM_RELATE_EFFECT_WINDOW");
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "EFFECT":
                ShowEffect();
                break;

            case "FOCUS":
                FocusRelPet((int)objVal);
                break;
            case "REFRESH_RELATION_NEWMARK":
                ChcekRelatePos_NewMark();
                break;
        }
    }

    public override bool Refresh(object param)
    {
        UIGridContainer teamGrid = GetComponent<UIGridContainer>("role_team_info_grid");

        for (int i = 0; i <= 3; ++i)
        {
            SetTeamPetInfo(teamGrid.controlList[i], i);
        }

        UIGridContainer relGrid = GetComponent<UIGridContainer>("karma_info_grid");

        for (int i = 11; i <= 16; ++i)
        {
            SetRelatePetInfo(relGrid.controlList[i - 11], i);
        }

        SetTotal();
        ShowEffect();
        //added by xuke
        ChcekRelatePos_NewMark();
        //end
        return true;
    }

    private int GetCurCanActiveRelPos()
    {
        int _relIndex = (int)TeamManager.PET_RELATE_ACTIVE_INDEX;
        if (_relIndex < (int)TEAM_POS.RELATE_1 || _relIndex > (int)TEAM_POS.RELATE_6)
            return -1;
        return _relIndex - (int)TEAM_POS.RELATE_1;
    }
    //added by xuke 缘分站位红点提示
    private void ChcekRelatePos_NewMark() 
    {
        UIGridContainer relGrid = GetComponent<UIGridContainer>("karma_info_grid");
        if (relGrid == null)
            return;
        for (int i = 0, count = relGrid.MaxCount; i < count; i++) 
        {
            //初始化
            GameCommon.SetNewMarkVisible(relGrid.controlList[i],false);
        }
        int _curActivePosIndex = GetCurCanActiveRelPos();
        if (_curActivePosIndex == -1)
            return;
        if (_curActivePosIndex < relGrid.MaxCount) 
        {
            GameCommon.SetNewMarkVisible(relGrid.controlList[_curActivePosIndex],true);
        }
    }
    //end

    private void ShowEffect()
    {
        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_RELATE_EFFECT_WINDOW");
        GameCommon.SetUIButtonEnabled(mGameObjUI, "pet_change_Button", false);
        GameCommon.SetUIButtonEnabled(mGameObjUI, "pet_demount_Button", false);
        UIGridContainer relGrid = GetComponent<UIGridContainer>("karma_info_grid");

        for (int i = 11; i <= 16; ++i)
        {
            GameCommon.SetUIVisiable(relGrid.controlList[i - 11], "Checkmark", false);
        }
    }

    private void FocusRelPet(int teamPos)
    {
        TeamManager.mCurTeamPos = (TEAM_POS)teamPos;
        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
        UIGridContainer relGrid = GetComponent<UIGridContainer>("karma_info_grid");

        for (int i = 11; i <= 16; ++i)
        {
            GameCommon.SetUIVisiable(relGrid.controlList[i - 11], "Checkmark", i == (int)TeamManager.mCurTeamPos);
        }

        GameCommon.SetUIButtonEnabled(mGameObjUI, "pet_change_Button", true);
        GameCommon.SetUIButtonEnabled(mGameObjUI, "pet_demount_Button", true);
    }

    private void SetTeamPetInfo(GameObject target, int teamPos)
    {
        ActiveData activeData = TeamManager.GetActiveDataByTeamPos(teamPos);

        if (activeData == null)
        {
            GameCommon.SetUIVisiable(target, "name_label", false);
            GameCommon.SetUIVisiable(target, "icon_sprite", false);
            GameCommon.SetUIVisiable(target, "karma_number_label", false);
        }
        else
        {
            GameCommon.SetUIVisiable(target, "name_label", true);
            GameCommon.SetUIVisiable(target, "icon_sprite", true);
            GameCommon.SetUIVisiable(target, "karma_number_label", true);

            SetPetIconAndName(target, activeData.tid);
            int relCount = Relationship.GetCachedActiveRelateCount(teamPos);//((IEnumerable<Relationship>)Relationship.AllRelationships(teamPos)).Count<Relationship>(x => x.active);
            GameCommon.SetUIText(target, "karma_number_label", relCount.ToString());
        }
    }

    private void SetRelatePetInfo(GameObject target, int teamPos)
    {   
        TeamPosData teamPosData = TeamManager.GetTeamPosData(teamPos);
        bool isOpen = teamPosData.openLevel <= RoleLogicData.GetMainRole().level;
        ActiveData activeData = TeamManager.GetActiveDataByTeamPos(teamPos);

        if (!isOpen)
        {
            GameCommon.SetUIVisiable(target, "add_karma_btn", false);
            GameCommon.SetUIVisiable(target, "Background", true);
            GameCommon.SetUIVisiable(target, "level_limit_label", true);
            GameCommon.SetUIVisiable(target, "karma_btn", false);
            GameCommon.SetUIVisiable(target, "name_label", false);

            GameCommon.SetUIText(target, "level_limit_label", teamPosData.openLevel + "级开放");
        }
        else if (activeData == null)
        {
            GameCommon.SetUIVisiable(target, "add_karma_btn", true);
            GameCommon.SetUIVisiable(target, "Background", true);
            GameCommon.SetUIVisiable(target, "level_limit_label", false);
            GameCommon.SetUIVisiable(target, "karma_btn", false);
            GameCommon.SetUIVisiable(target, "name_label", false);

            InitAddBtn(target, teamPos);
        }
        else
        {
            GameCommon.SetUIVisiable(target, "add_karma_btn", false);
            GameCommon.SetUIVisiable(target, "Background", false);
            GameCommon.SetUIVisiable(target, "level_limit_label", false);
            GameCommon.SetUIVisiable(target, "karma_btn", true);
            GameCommon.SetUIVisiable(target, "name_label", true);

            SetPetIconAndName(target, activeData.tid);
            InitHeadBtn(target, teamPos);
        }
    }

    private void SetTotal()
    {
        int totalRel = 0;

        for (int i = 0; i <= 3; ++i)
        {
            totalRel += Relationship.GetCachedActiveRelateCount(i);
            //var rels = Relationship.AllRelationships(i);

            //if (rels != null)
            //{
            //    totalRel += ((IEnumerable<Relationship>)rels).Count<Relationship>(x => x.active);
            //}
        }

        SetText("cur_karma_label", "[ffffff]当前缘分总数[00ff00]" + totalRel + "[ffffff]个");
    }

    private void OnRelationChanged(int teamPos, HashSet<int> added, HashSet<int> removed)
    {
        if (teamPos <= 3)
        {
            UIGridContainer teamGrid = GetComponent<UIGridContainer>("role_team_info_grid");
            SetTeamPetInfo(teamGrid.controlList[teamPos], teamPos);
            SetTotal();
        }
        else 
        {
            UIGridContainer relGrid = GetComponent<UIGridContainer>("karma_info_grid");
            SetRelatePetInfo(relGrid.controlList[teamPos - 11], teamPos);

            if (TeamManager.GetActiveDataByTeamPos(teamPos) != null)
            {
                FocusRelPet(teamPos);
            }
            else 
            {
                ShowEffect();
            }
        }
    }
    
    private void SetPetIconAndName(GameObject target, int tid)
    {
        GameCommon.SetPetIconWithElementAndStar(target, "icon_sprite", "", "", tid);

        string name = TableCommon.GetStringFromActiveCongfig(tid, "NAME");
        GameCommon.SetUIText(target, "name_label", name);
    }

    private void InitHeadBtn(GameObject target, int teamPos)
    {
        var btnData = GameCommon.GetButtonData(target, "karma_btn");
        btnData.set("TEAM_POS", teamPos);
    }

    private void InitAddBtn(GameObject target, int teamPos)
    {
        var btnData = GameCommon.GetButtonData(target, "add_karma_btn");
        btnData.set("TEAM_POS", teamPos);
    }
}



public class Button_karma_effect_Button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("TEAM_RELATE_INFO_WINDOW", "EFFECT", null);
        return true;
    }
}


public class Button_add_karma_btn : CEvent
{
    public override bool _DoEvent()
    {
        int teamPos = get("TEAM_POS");
        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();

        openObj.mBagShowType = BAG_SHOW_TYPE.USE;

        openObj.mFilterCondition = (itemData) =>
        {
            return !TeamManager.IsPetInTeamByTid(itemData.tid);
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            return GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByTeamPosType);
        };

        openObj.mSelectAction = (upItemData) =>
        {
            if (null == upItemData)
                return;

            int iUpItemId = upItemData.itemId;
            int iUpTid = upItemData.tid;

            TeamManager.RequestChangeTeamPos((int)teamPos, iUpItemId, iUpTid, -1, -1);
        };

        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
        return true;
    }
}


public class Button_karma_btn : CEvent
{
    public override bool _DoEvent()
    {
        int teamPos = get("TEAM_POS");
        DataCenter.SetData("TEAM_RELATE_INFO_WINDOW", "FOCUS", teamPos);
        return true;
    }
}


public class Button_pet_change_Button : CEvent
{
    public override bool _DoEvent()
    {
        int teamPos = (int)TeamManager.mCurTeamPos;
        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();

        openObj.mBagShowType = BAG_SHOW_TYPE.USE;

        openObj.mFilterCondition = (itemData) =>
        {
            ActiveData activeData = TeamManager.GetActiveDataByTeamPos(teamPos);
            return !TeamManager.IsPetInTeamByTid(itemData.tid) /*指定tid的宠物，只要有一个在队伍中，便都不能显示*/
                || (itemData.tid == activeData.tid && itemData.itemId != activeData.itemId);/*当前阵位的宠物对应的tid的宠物，是可以显示的，除了自身*/
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER", true);
            List<PetData> mlist = new List<PetData>();
            foreach (PetData temp in tempList)
            {
                if (temp.tid == 30293 || temp.tid == 30299 || temp.tid == 30305)
                {
                    mlist.Add(temp);
                }
            }
            foreach (PetData temp in mlist)
            {
                tempList.Remove(temp);
            }
            GameCommon.SortList<PetData>(mlist, GameCommon.SortPetDataByTeamPosType);
            GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByTeamPosType);
            foreach (PetData temp in mlist)
            {
                tempList.Add(temp);
            }
            return tempList;
        };

        openObj.mSelectAction = (upItemData) =>
        {
            if (null == upItemData)
                return;

            int iUpItemId = upItemData.itemId;
            int iUpTid = upItemData.tid;

            PetData downPetData = TeamManager.GetPetDataByTeamPos(teamPos);

            TeamManager.RequestChangeTeamPos(teamPos, iUpItemId, iUpTid,
                downPetData != null ? downPetData.itemId : -1, downPetData != null ? downPetData.tid : -1);
        };

        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
        return true;
    }
}


public class Button_pet_demount_Button : CEvent
{
    public override bool _DoEvent()
    {
        if ((int)TeamManager.mCurTeamPos >= (int)TEAM_POS.RELATE_1 && (int)TeamManager.mCurTeamPos <= (int)TEAM_POS.RELATE_6)
        {
            var downPetData = TeamPosInfoWindow.mCurActiveData;

            if (downPetData != null)
            {
                TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, -1, -1, downPetData.itemId, downPetData.tid);
            }
        }

        return true;
    }
}