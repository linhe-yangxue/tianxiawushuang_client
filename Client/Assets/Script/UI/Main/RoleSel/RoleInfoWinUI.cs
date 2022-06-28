using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class RoleInfoChangeRoleWindow : tWindow
{
    public override void OnOpen()
    {
        Refresh(null);
    }

    public override bool Refresh(object param)
    {
        InitRole();
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "CHANGE_ROLE":
                OnChangeRole((int)objVal);
                break;
            case "SEND":
                OnSendFromMessageBox();
                break;
        }
    }

    private void OnChangeRole(int index)
    {
        RoleLogicData.Self.ChangeMainRole(index);       
        //GameCommon.RestoreCharacterConfigRecord ();
        //GameCommon.ChangeCharacterConfigRecord ();
        Refresh(null);
    }

    private void InitRole()
    {
		RoleLogicData roleLogicData = RoleLogicData.Self; // RoleLogicData.Self;
        UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "Grid").GetComponent<UIGridContainer>();
        int iCount = 0;
        for (int m = iCount; m < roleLogicData.mRoleList.Length; ++m)
        {
            if (roleLogicData.mRoleList[m] != null)
                iCount++;
        }
        grid.MaxCount = iCount + 1;

        for (int i = 0; i < grid.MaxCount - 1; ++i)
        {
            InitButton(i);

            if (roleLogicData.GetRole(i) == RoleLogicData.GetMainRole())
            {
                SetRoleUI(i, roleLogicData.GetRole(i), true, false, true);
            }
            else
            {
                SetRoleUI(i, roleLogicData.GetRole(i), true, false, false);
            }
        }

        InitButton(grid.MaxCount - 1);
        SetRoleUI(grid.MaxCount - 1, null, false, false, false);
    }

    private void InitButton(int slotIndex)
    {
        UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "Grid").GetComponent<UIGridContainer>();
        NiceData btnData = GameCommon.GetButtonData(grid.controlList[slotIndex], "Role");
        btnData.set("ROLE_INDEX", slotIndex);
    }

    private void SetRoleUI(int slotIndex, RoleData role, bool showName, bool isLock, bool isUse)
    {
        UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "Grid").GetComponent<UIGridContainer>();
        GameObject obj = grid.controlList[slotIndex];
        if(role != null)
        {
            SetRoleIcon(obj, role.tid);
        }        
        GameCommon.SetUIVisiable(obj, "lab_level", showName);
        GameCommon.SetUIVisiable(obj, "lab_name", showName);
        GameCommon.SetUIVisiable(obj, "lab_name1", showName);
        GameCommon.SetUIVisiable(obj, "diamond", isLock);
        GameCommon.SetUIVisiable(obj, "lock", isLock);
        GameCommon.SetUIVisiable(obj, "use", isUse);

        if (showName)
        {
            if (role == null)
            {
                GameCommon.SetUIText(obj, "lab_level", "Lv1");
            }
            else
            {
                GameCommon.SetUIText(obj, "lab_level", "Lv" + role.level.ToString());
                GameCommon.SetUIText(obj, "lab_name", TableCommon.GetStringFromActiveCongfig(role.tid, "NAME"));
                GameCommon.SetUIText(obj, "lab_name1", TableCommon.GetStringFromActiveCongfig(role.tid, "NAME"));
            }
        }

        if (isLock)
        {
            SetCost(obj, slotIndex);
        }
    }

    private void SetRoleIcon(GameObject obj, int iIndex)
    {
        string atlasName = TableCommon.GetStringFromRoleSkinConfig(iIndex, "ATLAS");
        string spriteName = TableCommon.GetStringFromRoleSkinConfig(iIndex, "SPRITE");
        GameCommon.SetIcon(obj, "Background", spriteName, atlasName);
    }

    private string GetUIObjName(int slotIndex)
    {
        return "Role0" + (slotIndex + 1).ToString();
    }

    private void SetCost(GameObject obj, int roleIndex)
    {
        DataRecord record = DataCenter.mRoleUIConfig.GetRecord(roleIndex);

        if (record == null)
            return;

        int itemType = record.get("COST_ITEM_TYPE");
        int itemCount = record.get("COST_ITEM_COUNT");
        GameObject costObj = GameCommon.FindObject(obj, "diamond");
        UISprite costIcon = costObj.GetComponent<UISprite>();
        GameCommon.SetItemSmallIcon(costIcon, itemType);
        GameCommon.SetUIText(costObj, "diamond_num", itemCount.ToString());
    }

    private void OnSendFromMessageBox()
    {
		int i = (int)(SHOP_PAGE_TYPE.CHARACTER);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
    }
}

public class Button_ClickRoleButton : CEvent
{
    public override bool _DoEvent()
    {
        int roleIndex = get("ROLE_INDEX");

        RoleLogicData roleLogicData = RoleLogicData.Self;
        int iSelModelIndex = RoleLogicData.GetMainRole().tid;
        RoleData roleData = roleLogicData.GetRole(roleIndex);
        if (roleData != null)
        {
            int iModelIndex = roleData.tid;
            if (GameCommon.GetCharacterTypeByModelIndex(iModelIndex) != GameCommon.GetCharacterTypeByModelIndex(iSelModelIndex))
            {
                ChangeRole(roleIndex);
            }
        }

        return true;
    }

    private void ChangeRole(int roleIndex)
    {
        CS_RequestChangeMainChar evt = Net.StartEvent("CS_RequestChangeMainChar") as CS_RequestChangeMainChar;
        evt.set("CHAR_ID", RoleLogicData.Self.GetRole(roleIndex).mIndex);
        evt.set("INDEX", roleIndex);
        evt.set("SKIN", GetShopIndex(roleIndex));
        evt.mAction = () =>
            {
                int index = evt.get("INDEX");
                DataCenter.SetData("ROLE_INFO_CHANGE_ROLE_WINDOW", "CHANGE_ROLE", index);
                DataCenter.SetData("ROLE_INFO_WINDOW", "REFRESH", null);
            };
        evt.DoEvent();
    }

    private int GetShopIndex(int roleIndex)
    {
        RoleData roleData = RoleLogicData.Self.GetRole(roleIndex);
        if(roleData != null)
        {            
            foreach (KeyValuePair<int, DataRecord> iter in DataCenter.mShopSlotBase.GetAllRecord())
            {
                if (iter.Value.get("ROLE_MODEL_INDEX") == roleData.tid)
                    return iter.Key;
            }
        }
        return -1;        
    }

    private void BuyRole(RoleLogicData roleLogicData, int slotIndex)
    {
        DataRecord record = DataCenter.mRoleUIConfig.GetRecord(slotIndex);
        if (record == null)
            return;

        int itemType = record.get("COST_ITEM_TYPE");
        int itemCount = record.get("COST_ITEM_COUNT");

        if (itemType == (int)ITEM_TYPE.GOLD)
        {
            if (roleLogicData.gold < itemCount)
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_NO_ENOUGH_GOLD, "", "ROLE_INFO_CHANGE_ROLE_WINDOW");
            }
            else
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_BUY_ROLE, "", "ROLE_INFO_CHANGE_ROLE_WINDOW");
            }
        }
        else if(itemType == (int)ITEM_TYPE.YUANBAO)
        {
            if (roleLogicData.diamond < itemCount)
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_NO_ENOUGH_DIAMOND, "", "ROLE_INFO_CHANGE_ROLE_WINDOW");
            }
            else
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_BUY_ROLE, "", "ROLE_INFO_CHANGE_ROLE_WINDOW");
            }
        }
        else
        {
            return;
        }
    }
}

public class Button_ChangeRole : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RoleInfoChangeRoleWindow", "OPEN", true);
		return true;
    }
}

public class Button_RoleSkinExit : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RoleInfoChangeRoleWindow", "CLOSE", true);
        return true;
    }
}