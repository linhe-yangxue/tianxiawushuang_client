using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

enum eStartLevelUpResult
{
    STAR_LEVEL_UP_SUCCEED,
    STAR_LEVEL_LOW,
    STAR_MONEY_NOT_ENOUGH,
    STAR_GEM_NOT_ENOUGH,
    STAR_NOT_ALLOW,
    STAR_NOT_CONFIG,
}

public class RoleStarLevelUpWindow : tWindow
{
	public override void Init()
	{
		Net.gNetEventCenter.RegisterEvent("CS_RequestRoleStartlevelUp", new DefineFactory<CS_RequestRoleStartlevelUp>());
        EventCenter.Self.RegisterEvent("Button_role_evolution_button", new DefineFactory<Button_role_evolution_button>());
	}

    public override void OnOpen()
    {
        //DataCenter.OpenWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.SetData("ALL_ROLE_ATTRIBUTE_INFO_WINDOW", "SET_SEL_PAGE", "STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        //DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");

        //base.OnOpen();       
        DataCenter.OpenWindow("BAG_INFO_WINDOW");
        DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Stone_Window);               
        Refresh(null);
    }

    public override void Close()
    {
        DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_STATE", StoneStateFlags.None);
        DataCenter.CloseWindow("BAG_INFO_WINDOW");
        base.Close();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "STAR_LEVEL_UP":
                StarLevelUp((int)objVal);
                break;
        }
    }

    public override bool Refresh(object param)
    {
        //DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_STATE", StoneStateFlags.DisableButton);
        StarLevelData curData, needData;
        GetStarLevelData(out curData, out needData);       
        RefreshUI(curData, needData);
        ShowModel();
		return true;
    }

    private void StarLevelUp(int nowLevel)
    {
        StarLevelData needData = GetStarLevelNeedData(nowLevel - 1);
        if (needData != null)
        {
            ChangeRoleData(needData);
        }
        ChangeRoleModelIndex();
        Refresh(null);
    }

    private void ChangeRoleModelIndex()
    {
        RoleData mainRole = RoleLogicData.GetMainRole();
        int evolutionIndex = TableCommon.GetNumberFromActiveCongfig(mainRole.tid, "EVOLUTION_ID");

        if (evolutionIndex > 0)
        {
            mainRole.tid = evolutionIndex;
        }
    }

    private void RefreshUI(StarLevelData curData, StarLevelData needData)
    {
        SetRoleTitle(curData, needData);
        SetLevelUpButton(curData, needData);
        RefreshRoleBox(curData, needData);
        RefreshGemUI(curData, needData);
    }

    private bool Fit(StarLevelData curData, StarLevelData needData)
    {
        if (curData == null || needData == null)
        {
            return false;
        }
        return curData.mLevel >= needData.mLevel && curData.mGold >= needData.mGold && FitGemNumber(curData, needData);
    }

    private bool FitGemNumber(StarLevelData curData, StarLevelData needData)
    {
        if (curData == null || needData == null)
        {
            return false;
        }
        for (int i = 0; i < 5; ++i)
        {
            if (curData.mGems[i].mCount < needData.mGems[i].mCount)
            {
                return false;
            }
        }
        return true;
    }

    private void SetRoleTitle(StarLevelData curData, StarLevelData needData)
    {
        UILabel nowTitle = GetComponent<UILabel>("now_stage_sprite");
        UILabel nextTitle = GetComponent<UILabel>("next_stage_sprite");
        GameCommon.SetRoleTitle(nowTitle, curData.mStarLevel);
        if (needData == null)
        {
            SetText("text_label", "已到达最高阶级");
            nextTitle.text = "";
        }
        else
        {
            SetText("text_label", "将主角修炼到更高阶级");
            GameCommon.SetRoleTitle(nextTitle, curData.mStarLevel + 1);
        }
    }

    private void SetLevelUpButton(StarLevelData curData, StarLevelData needData)
    {
        GameObject btnObj = GameCommon.FindObject(mGameObjUI, "role_evolution_button");
        UIImageButton btn = btnObj.GetComponent<UIImageButton>();
        if (Fit(curData, needData))
        {
            btn.isEnabled = true;
            GameCommon.SetUIVisiable(btnObj, "btn_shade", false);
        }
        else
        {
            btn.isEnabled = false;
            GameCommon.SetUIVisiable(btnObj, "btn_shade", true);
        }
    }

    private void RefreshGemUI(StarLevelData curData, StarLevelData needData)
    {
        DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_GEM_ICONS", null);
        DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_STATE", StoneStateFlags.DisableButton);

        GameObject xingji = GameCommon.FindObject(mGameObjUI, "xingji_sprite");

        if (curData == null || needData == null)
        {
            for (int i = 0; i < 5; ++i)
            {
                string starName = "star_" + (i + 1).ToString();
                GameObject starObj = GameCommon.FindObject(xingji, starName);
                UISprite star = starObj.GetComponent<UISprite>();
                star.atlas = null;
            }
            return;
        }
        
        for (int i = 0; i < 5; ++i)
        {
            int gemID = curData.mGems[i].mType;
            string starName = "star_" + (i + 1).ToString();
            GameObject starObj = GameCommon.FindObject(xingji, starName);
            UISprite star = starObj.GetComponent<UISprite>();
            GameCommon.SetGemIcon(star, gemID);
            KeyValuePair<int, StoneStateFlags> pair;
            if (curData.mGems[i].mCount < needData.mGems[i].mCount)
            {
                GameCommon.SetUIVisiable(starObj, "star_shade", true);
                pair = new KeyValuePair<int, StoneStateFlags>(gemID - 1000, StoneStateFlags.DisableButton | StoneStateFlags.RedTextColor);
            }
            else 
            {
                GameCommon.SetUIVisiable(starObj, "star_shade", false);
				pair = new KeyValuePair<int, StoneStateFlags>(gemID - 1000, StoneStateFlags.DisableButton | StoneStateFlags.Checked);
            }

            DataCenter.SetData("BAG_INFO_WINDOW", "SET_STONE_STATE", pair);        
        }
    }

    private void RefreshRoleBox(StarLevelData curData, StarLevelData needData)
    {
        GameObject box = GameCommon.FindObject(mGameObjUI, "role_box_sprite");
        RefreshRoleBoxLevel(box, curData, needData);
        RefreshRoleBoxGold(box, curData, needData);
        RefreshRoleBoxGem(box, curData, needData);
    }

    private void RefreshRoleBoxLevel(GameObject box, StarLevelData curData, StarLevelData needData)
    {
        string str = "";
        if (curData != null && needData != null)
        {
            if (curData.mLevel < needData.mLevel)
            {
                str = "[FFFFFF]需要等级 [FF0000] " + needData.mLevel + " [FFFFFF]级";
            }
            else
            {
                str = "需要等级 " + needData.mLevel + " 级";
            }
        }
        GameCommon.SetUIText(box, "need_level_label", str);
    }

    private void RefreshRoleBoxGold(GameObject box, StarLevelData curData, StarLevelData needData)
    {
        string str = "";
        if (curData != null && needData != null)
        {
            if (curData.mGold < needData.mGold)
            {
                str = "[FF0000]" + needData.mGold;
            }
            else
            {
                str = needData.mGold.ToString();
            }
        }
        GameCommon.SetUIText(box, "goldcoin_label", str);
    }

    private void RefreshRoleBoxGem(GameObject box, StarLevelData curData, StarLevelData needData)
    {
        string str = "";

        if (curData != null && needData != null)
        {
            string strType = "小";
            string strNum = "一";
            switch (curData.mStarLevel)
            {
                case 1:
                    strType = "小";
                    strNum = "一";
                    break;
                case 2:
                    strType = "中";
                    strNum = "两";
                    break;
                case 3:
                    strType = "大";
                    strNum = "三";
                    break;
            }

            if (FitGemNumber(curData, needData))
            {
                str = "需要五行" + strType + "灵石各" + strNum + "枚";
            }
            else
            {
                str = "[FFFFFF]需要五行" + strType + "灵石各[FF0000] " + strNum + " [FFFFFF]枚";
            }
        }
        GameCommon.SetUIText(box, "need_label", str);
    }

    private void ShowModel()
    {
        ObjectManager.Self.ClearAll();
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        GameCommon.ShowCharactorModel(uiPoint, 1.2f);
    }

    private void ChangeRoleData(StarLevelData needData)
    {
        GameCommon.RoleChangeGold(-needData.mGold);
        GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
        for (int i = 0; i < 5; ++i)
        {
            GemData gemData = gemLogicData.GetGemDataByType(needData.mGems[i].mType);
            if (gemData != null)
            {
                gemData.mCount -= needData.mGems[i].mCount;
            }
        }
    }

    private void GetStarLevelData(out StarLevelData curData, out StarLevelData needData)
    {
        curData = GetStarLevelCurrentData();
        if (curData == null)
        {
            needData = null;
        }
        else
        {
            needData = GetStarLevelNeedData(curData.mStarLevel);
        }
    }

    private StarLevelData GetStarLevelCurrentData()
    {
        GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
        RoleData mainRole = RoleLogicData.GetMainRole();
        if (mainRole == null)
        {
            return null;
        }

        StarLevelData curData = new StarLevelData();  
        curData.mStarLevel = Mathf.Max(1, mainRole.starLevel);
        DataRecord record = DataCenter.mRoleStarLevel.GetRecord(curData.mStarLevel);

        curData.mLevel = mainRole.level;
        curData.mGold = RoleLogicData.Self.gold;

        for (int i = 0; i < 5; ++i)
        {
            string key = "GEM_ID_" + (i + 1).ToString();
            int iType = record.getData(key);
            curData.mGems[i] = new GemData();
            curData.mGems[i].mType = iType;
            curData.mGems[i].mCount = gemLogicData.GetGemDataByType(iType).mCount;
        }
        return curData;
    }

    private StarLevelData GetStarLevelNeedData(int curLevel)
    {      
        DataRecord record = DataCenter.mRoleStarLevel.GetRecord(curLevel);
        if (record == null || record.getData("EVO_ENABLE") == 0)
        {
            return null;
        }
        StarLevelData needData = new StarLevelData();
        needData.mStarLevel = curLevel + 1;
        needData.mLevel = record.getData("LEVEL_REQUIRED");
        needData.mGold = record.getData("COST_COIN");

        for (int i = 0; i < 5; ++i)
        {
            string key = "GEM_ID_" + (i + 1).ToString();
            int iType = record.getData(key);
            needData.mGems[i] = new GemData();
            needData.mGems[i].mType = iType;
            needData.mGems[i].mCount = record.getData("COST_NUM");
        }
        return needData;
    }

    private class StarLevelData
    {
        public int mStarLevel = 0;
        public int mLevel = 0;
        public int mGold = 0;
        public GemData[] mGems = new GemData[5];
    }
}

public class Button_role_evolution_button : CEvent
{
    public override bool _DoEvent()
    {
        tEvent request = Net.StartEvent("CS_RequestRoleStartlevelUp");
        request.DoEvent();

		return true;
    }
}

public class CS_RequestRoleStartlevelUp : tNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
        int result = respEvt["RESULT"];
        if (result == (int)eStartLevelUpResult.STAR_LEVEL_UP_SUCCEED)
        {
            //RoleLogicData roleData = RoleLogicData.Self;
			int nowStar = respEvt["STAR_LEVEL"];
			RoleLogicData.GetMainRole().starLevel = nowStar;
            
//            DataCenter.OpenMessageWindow("成功升阶至" + nowStar.ToString());
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_PET_UPGRADE_SUCCESS, nowStar.ToString ());
            DataCenter.SetData("STAR_LEVEL_UP_WINDOW", "STAR_LEVEL_UP", nowStar);
        }
        else
        {
            string info;
            switch ((eStartLevelUpResult)result)
            {
                case  eStartLevelUpResult.STAR_LEVEL_LOW:
                    info = "等级太低";
                    break;
                case  eStartLevelUpResult.STAR_MONEY_NOT_ENOUGH:
                    info = "金币不够";
                    break;
                case  eStartLevelUpResult.STAR_GEM_NOT_ENOUGH:
                    info = "灵石不够";
                    break;

                case  eStartLevelUpResult.STAR_NOT_ALLOW:
                    info = "星阶未开放";
                    break;
                case  eStartLevelUpResult.STAR_NOT_CONFIG:
                    info = "星阶未配置";
                    break;

                default:
                    info = "未知错误";
				break;
            }

            DataCenter.OpenMessageWindow(info);
        }
	}
}
