using UnityEngine;
using System.Collections;
using Logic;




public class ResolveBGWindow : tWindow
{
    enum RESOLVE_PAGE_TYPE
    {
        PET_RESOLVE,
        EQUIP_REBIRTH,
        PET_REBIRTH,
        MAGIC_REBIRTH,
    }

    RESOLVE_PAGE_TYPE curPageType = RESOLVE_PAGE_TYPE.PET_RESOLVE;
    
    

    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_recover_title(Clone)_0", new DefineFactory<SwitchPageEvent>());
        EventCenter.Self.RegisterEvent("Button_recover_title(Clone)_1", new DefineFactory<SwitchPageEvent>());
        EventCenter.Self.RegisterEvent("Button_recover_title(Clone)_2", new DefineFactory<SwitchPageEvent>());
        EventCenter.Self.RegisterEvent("Button_recover_title(Clone)_3", new DefineFactory<SwitchPageEvent>());
        EventCenter.Self.RegisterEvent("Button_resolve_btn_help", new DefineFactory<Button_resolve_btn_help>());

        //EventCenter.Self.RegisterEvent("Button_recover_back_btn", new DefineFactory<CloseRecover>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        DataCenter.OpenBackWindow("RECOVER_WINDOW", "a_ui_huishou_logo", () => MainUIScript.Self.ShowMainBGUI(), 120);
        curPageType = RESOLVE_PAGE_TYPE.PET_RESOLVE;
        DataCenter.OpenWindow(UIWindowString.recover_petResolve);
        //GameCommon.ToggleTrue(GetSub("recover_title(Clone)_0"));
        var list = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "recover_title_buttons_group").controlList;
        list.ForEach(grid => grid.GetComponent<UIToggle>().value = false);

        if (param is int)
        {
            int index = (int)param;
            if (list.Count > index)
            {
                list[index].GetComponent<UIToggle>().value = true;
            }
        }
        else
        {
            list[0].GetComponent<UIToggle>().value = true;
        }
        //added by xuke 红点相关
        RecoverNewMarkManager.Self.CheckRecoverInfoAll_NewMark();
        RecoverNewMarkManager.Self.RefreshRecoverNewMark();
        //end
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("SHOW_WINDOW" == keyIndex)
        {
            curPageType = (RESOLVE_PAGE_TYPE)objVal;
			GameObject obj1 = GameCommon.FindObject (mGameObjUI, "recover_bg_01").gameObject;
			GameObject obj2 = GameCommon.FindObject (mGameObjUI, "recover_bg_02").gameObject;
			if((int)objVal == 0 || (int)objVal == 1)
			{
				obj1.SetActive (false);
				obj2.SetActive (true);
			}else
			{
				obj1.SetActive (true);
				obj2.SetActive (false);
			}
            ShowWindow();
        }
        else if ("REFRESH_RECOVER_NEWMARK" == keyIndex) 
        {
            RefreshRecoverNewMark();
        }
    }

    private void RefreshRecoverNewMark() 
    {
        UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "recover_title_buttons_group");
        if (_grid == null)
            return;
        if (_grid.MaxCount < 2)
            return;
        GameCommon.SetNewMarkVisible(_grid.controlList[0],RecoverNewMarkManager.Self.RecoverPetVisible);
        GameCommon.SetNewMarkVisible(_grid.controlList[1],RecoverNewMarkManager.Self.RecoverEquipVisible);
    }

    void ShowWindow()
    {
        CloseAllWindow();
        switch (curPageType)
        { 
            case RESOLVE_PAGE_TYPE.PET_RESOLVE:
                DataCenter.OpenWindow(UIWindowString.recover_petResolve);
                break;

            case RESOLVE_PAGE_TYPE.EQUIP_REBIRTH:
                DataCenter.OpenWindow(UIWindowString.recover_equipResolve);
                break;

            case RESOLVE_PAGE_TYPE.PET_REBIRTH:
                DataCenter.OpenWindow(UIWindowString.recover_petRebirth);
                break;

            case RESOLVE_PAGE_TYPE.MAGIC_REBIRTH:
                DataCenter.OpenWindow(UIWindowString.recover_magicRecast);
                break;
        }
    }

    void CloseAllWindow()
    {
        DataCenter.CloseWindow(UIWindowString.recover_petResolve);
        DataCenter.CloseWindow(UIWindowString.recover_equipResolve);
        DataCenter.CloseWindow(UIWindowString.recover_petRebirth);
        DataCenter.CloseWindow(UIWindowString.recover_magicRecast);
    }

    public override void Close()
    {
        base.Close();
        DataCenter.CloseBackWindow();
        CloseAllWindow();
    }


    class SwitchPageEvent : CEvent
    {
        public override bool _DoEvent()
        {
            string[] names = GetEventName().Split('_');
            DataCenter.SetData("RECOVER_WINDOW", "SHOW_WINDOW", int.Parse(names[names.Length - 1]));
            return true;
        }

    }

    //class CloseRecover : CEvent
    //{
    //    public override bool _DoEvent()
    //    {
    //        DataCenter.CloseWindow("RECOVER_WINDOW");
    //        return true;
    //    }

    //}
}


public class Button_RecoverBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("RECOVER_WINDOW", "OPEN", true);
		MainUIScript.Self.HideMainBGUI ();
        return true;
    }
}

//rule button
public class Button_resolve_btn_help : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RULE_TIPS_WINDOW", HELP_INDEX.HELP_RECOVER);
        return base._DoEvent();
    }
}
