using UnityEngine;
using Logic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PACKAGE_TYPE
{
    PET = 1,            // 宠物
    PET_FRAGMENT,       // 宠物碎片
    EQUIP,              // 装备
    EQUIP_FRAGMENT,     // 装备碎片
    MAGIC,              // 法器
    MAGIC_FRAGMENT,     // 法器碎片    
    CONSUME_ITEM,       // 消耗品    
    MAX,
    GEM,                // 宝石(待定)
    MATERIAL,           // 材料(待定)
    MATERIAL_FRAGMENT,  // 材料碎片(待定)
}

public class PackageWindow : tWindow {

	public PACKAGE_TYPE mCurPackageType = PACKAGE_TYPE.PET;

	public Transform mAllBtnTrans;

	public override void Init ()
	{
		base.Init();
		EventCenter.Self.RegisterEvent("Button_package_btn", new DefineFactory<ButtonPackageBtn>());
		EventCenter.Self.RegisterEvent("Button_package_window_info_back", new DefineFactory<ButtonPackageWindowBackBtn>());

        for (int i = (int)PACKAGE_TYPE.PET; i < (int)PACKAGE_TYPE.MAX; i++)
        {
            EventCenter.Self.RegisterEvent("Button_package_btn_" + i.ToString(), new DefineFactory<PackageTypeEvent>());
        }
	}

	public void InitVariable()
	{
		mCurPackageType = PACKAGE_TYPE.PET;

		mAllBtnTrans = mGameObjUI.transform.Find("package_bg/all_button");
		if(mAllBtnTrans != null)
		{
			GameCommon.ToggleTrue(mAllBtnTrans.Find("package_btn_" + ((int)mCurPackageType).ToString()).gameObject);
		}
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SHOW_WINDOW")
		{
			mCurPackageType = (PACKAGE_TYPE)objVal;
			ShowItemGird();
		}
		else if(keyIndex == "")
		{

		}
	}

	public override void OnOpen ()
	{
		base.OnOpen ();

		MainUIScript.Self.DestroyAllMainWindow();

		DataCenter.OpenWindow("BACK_PACKAGE_WINDOW");

		InitVariable();

		Refresh (null);
	}

	public override bool Refresh (object param)
	{
		ShowItemGird();

		return base.Refresh (param);
	}

	public override void Close()
	{
		base.Close();

		DataCenter.CloseWindow("BACK_PACKAGE_WINDOW");
	}

	public void ShowItemGird()
	{
		RefreshItemGrid((int)mCurPackageType);
	}

	public void CloseAllWindow()
	{
		DataCenter.CloseWindow("ROLE_EQUIP_PACKAGE_WINDOW");
		DataCenter.CloseWindow("PET_EQUIP_PACKAGE_WINDOW");
		DataCenter.CloseWindow("PET_FRAGMENT_PACKAGE_WINDOW");
		DataCenter.CloseWindow("GEM_PACKAGE_WINDOW");
		DataCenter.CloseWindow("CONSUME_ITEM_PACKAGE_WINDOW");
        DataCenter.CloseWindow("MATERIAL_PACKAGE_WINDOW");
        DataCenter.CloseWindow("MATERIAL_FRAGMENT_PACKAGE_WINDOW");
	}

	public void RefreshItemGrid(int packageType)
	{
		CloseAllWindow();

		switch((PACKAGE_TYPE)packageType)
		{
		case PACKAGE_TYPE.EQUIP:
			DataCenter.OpenWindow("ROLE_EQUIP_PACKAGE_WINDOW");
			break;
        //case PACKAGE_TYPE.PET_EUQIP:
        //    DataCenter.OpenWindow("PET_EQUIP_PACKAGE_WINDOW");
        //    break;
		case PACKAGE_TYPE.PET_FRAGMENT:
			DataCenter.OpenWindow("PET_FRAGMENT_PACKAGE_WINDOW");
			break;
        case PACKAGE_TYPE.MATERIAL_FRAGMENT:
            DataCenter.OpenWindow("MATERIAL_FRAGMENT_PACKAGE_WINDOW");
            break;
		case PACKAGE_TYPE.GEM:
			DataCenter.OpenWindow("GEM_PACKAGE_WINDOW");
			break;
		case PACKAGE_TYPE.CONSUME_ITEM:
			DataCenter.OpenWindow("CONSUME_ITEM_PACKAGE_WINDOW");
            break;
        case PACKAGE_TYPE.MATERIAL:
            DataCenter.OpenWindow("MATERIAL_PACKAGE_WINDOW");
			break;
		}
	}

	///----------------------------------------------------------------------------------------------
	// role equip




	///----------------------------------------------------------------------------------------------
	// pet equip
	public void RefreshPetEquip()
	{
		
	}
	
	public void RefreshFragment()
	{
		
	}
	
	public void RefreshGem()
	{
		
	}
	
	public void RefreshConsumeItem()
	{
		
	}
}


public class ButtonPackageBtn : CEvent{

	public override bool _DoEvent ()
	{
        MainUIScript.Self.OpenMainUI();
        DataCenter.OpenWindow("PACKAGE_WINDOW");
        return true;
		if (CommonParam.bIsNetworkGame)
		{
            tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            gemQuest.DoEvent();

            tEvent petFragmentQuest = Net.StartEvent("CS_RequestFragmentData");
            petFragmentQuest.DoEvent();

            tEvent consumeQuest = Net.StartEvent("CS_RequestConsumData");
            consumeQuest.DoEvent();

            tEvent materialQuest = Net.StartEvent("CS_RequestMaterial");
            materialQuest.DoEvent();

            tEvent materialFragmentQuest = Net.StartEvent("CS_RequestMaterialFragment");
            materialFragmentQuest.DoEvent();

            CS_RequestRoleEquip quest = Net.StartEvent("CS_RequestRoleEquip") as CS_RequestRoleEquip;
			quest.mAction = () =>
			{
                MainUIScript.Self.OpenMainUI();
                DataCenter.OpenWindow("PACKAGE_WINDOW");
			};
			SetBackAction(quest);
			quest.DoEvent();
			
		}
		else
		{
			MainUIScript.Self.OpenMainUI();
			DataCenter.OpenWindow("PACKAGE_WINDOW");
		}
		return base._DoEvent ();
	}

	public void SetBackAction(CS_RequestRoleEquip request)
	{
		if(request != null)
		{
			if(get("ACTION") == "MAIL_ACTION")
				request.mBackAction = () => {MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.MailWindow);};
		}
	}
}

public class ButtonPackageWindowBackBtn : CEvent{
	
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("PACKAGE_WINDOW");
		if(MainUIScript.Self.mWindowBackAction != null)
		{
			MainUIScript.Self.mWindowBackAction();
			MainUIScript.Self.mWindowBackAction = null;
		}
		else
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return base._DoEvent ();
	}
}

public class PackageTypeEvent : CEvent
{
	public override bool _DoEvent()
	{
		string[] names = GetEventName().Split('_');
		DataCenter.SetData("PACKAGE_WINDOW", "SHOW_WINDOW", int.Parse(names[names.Length - 1]));
		return true;
	}
}
