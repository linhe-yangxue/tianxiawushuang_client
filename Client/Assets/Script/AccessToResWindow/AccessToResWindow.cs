using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class AccessToResWindow : tWindow
{
    public int iCurTid = 0;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_access_close_Btn", new DefineFactoryLog<Button_access_close_Btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        //设置层级
        GameCommon.SetWindowZ(mGameObjUI, -1200);

        //data
        if (param is int)
        {
            iCurTid = (int)param;
        }

        //ui
        InitUI();
    }

    public void InitUI()
    {
        InitTopUI();

        //route
        UpdateGetWayUI();  
    }

    public void InitTopUI()
    {
        //ICON
        GameObject topIcon = GameCommon.FindObject(mGameObjUI, "icon");
        GameCommon.SetOnlyItemIcon(topIcon, iCurTid);

        //NAME
        GameCommon.SetUIText(mGameObjUI, "name", GameCommon.GetItemName(iCurTid));

        //NUM
        GameCommon.SetUIText(mGameObjUI, "own_num", PackageManager.GetItemLeftCount(iCurTid).ToString());
    }

    public void UpdateGetWayUI()
    {
        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grid");
        GameCommon.GetParth(grid, iCurTid);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "ACCESS_TO_RES_INITUI":
                {

                }
                break;

            default:
                break;
        }
    }

    public override void Close()
    {
        base.Close();

    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        return true;
    }
}

public class Button_access_close_Btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.access_to_res_window);
        return true;
    }
}

