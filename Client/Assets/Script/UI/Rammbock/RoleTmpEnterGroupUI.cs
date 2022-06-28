using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class RoleTmpEnterGroupWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grabtreasure_enter_PVPBtn", new DefineFactoryLog<Button_grabtreasure_enter_PVPBtn>());
        EventCenter.Self.RegisterEvent("Button_rammbock_enter_PVPBtn", new DefineFactoryLog<Button_rammbock_enter_PVPBtn>());
        EventCenter.Self.RegisterEvent("Button_peak_pvp_enter_PVPBtn", new DefineFactoryLog<Button_peak_pvp_enter_PVPBtn>());
		EventCenter.Self.RegisterEvent("Button_peak_enter_MySteriousShop", new DefineFactory<Button_peak_enter_MySteriousShop>());
        EventCenter.Self.RegisterEvent("Button_fairyland_enter_PVEBtn", new DefineFactoryLog<Button_fairyland_enter_PVEBtn>());
    }
}

/// <summary>
/// 夺宝临时入口按钮
/// </summary>
class Button_grabtreasure_enter_PVPBtn : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.DoCoroutine(__DoAction());
        return true;
    }

    private IEnumerator __DoAction() 
    {
        yield return NetManager.StartWaitPackageData(PACKAGE_TYPE.MAGIC_FRAGMENT);

        DataCenter.OpenWindow("GRABTREASURE_WINDOW");
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.GRABTREASURE);        
    }
}

/// <summary>
/// 群魔乱舞临时入口按钮
/// </summary>
class Button_rammbock_enter_PVPBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RAMMBOCK_WINDOW");
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.RAMMBOCK);
//        EventCenter.Start("Button_trial_window_back_btn").DoEvent();

        return true;
    }
}

/// <summary>
/// 临时仙境入口按钮
/// </summary>
class Button_fairyland_enter_PVEBtn : CEvent
{
    public override bool _DoEvent()
    {
//        MainUIScript.Self.OpenMainUI();
        GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(""));

        return true;
    }
}


/// <summary>
/// 竞技场临时入口按钮
/// </summary>
class Button_peak_pvp_enter_PVPBtn : CEvent
{
    public override bool _DoEvent()
    {
        //GlobalModule.DoCoroutine(DoButton());
        DataCenter.OpenWindow(UIWindowString.arena_main_window);
        return true;
    }

    public static IEnumerator DoButton()
    {
        if (Time.time - ArenaBase.lastResfreshTime > 5f)
        {
            ArenaNetManager.ArenaChallengeListRequester arenaReq = new ArenaNetManager.ArenaChallengeListRequester();
            yield return arenaReq.Start();

            if (!arenaReq.success)
                yield break;
        }

        MainUIScript.Self.OpenMainUI();
        DataCenter.OpenWindow(UIWindowString.arena_main_window);
        DataCenter.SetData(UIWindowString.arena_main_window, "INIT_UI", null);
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.PVP);
    }
}

class Button_peak_enter_MySteriousShop:CEvent {
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
		DataCenter.OpenBackWindow("NEW_MYSTERIOUS_SHOP_WINDOW", "a_ui_heishilogo", () => MainUIScript.Self.ShowMainBGUI(), 184);
		MainUIScript.Self.HideMainBGUI ();
		return true;
	}
}