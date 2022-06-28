using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;

public enum ALL_ROLE_ATTRIBUTE_PAGE_TYPE
{
	RoleInfo,
	RoleEvolution,
	RoleEquipCultivate,
	RoleEquipComposition,
}

public class AllRoleAttributeInfoUI : MonoBehaviour {

	//// Use this for initialization
	void Start () 
	{
        AllRoleAttributeInfoWindow win = new AllRoleAttributeInfoWindow();
        win.mGameObjUI = this.gameObject;
        DataCenter.Self.registerData("ALL_ROLE_ATTRIBUTE_INFO_WINDOW", win);
		DataCenter.Self.registerData("ExpansionInfoWindow", new ExpansionInfoWindow(transform.Find("ExpansionInfoWindow").gameObject));
        DataCenter.OpenWindow(MainUIScript.Self.mStrAllRoleAttInfoPageWindowName);
		DataCenter.SetData ("ALL_ROLE_ATTRIBUTE_INFO_WINDOW","SET_SEL_PAGE", MainUIScript.Self.mStrAllRoleAttInfoPageWindowName);

		DataCenter.OpenWindow ("BACK_GROUP_ROLE_WINDOW");
    }
    //
	void OnDestroy()
	{
		DataCenter.Remove("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
		DataCenter.Remove("ExpansionInfoWindow");
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");

		DataCenter.CloseWindow ("BACK_GROUP_ROLE_WINDOW");
	}
}

public class AllRoleAttributeInfoWindow : tWindow
{
	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ToggleCloseButCanClick (GetSub ("RoleEquipCultivateBtn"), UNLOCK_FUNCTION_TYPE.NONE, "Checkmark");
		GameCommon.ToggleCloseButCanClick (GetSub ("RoleEvolutionBtn"), UNLOCK_FUNCTION_TYPE.UPGRADE_ROLE, "Checkmark");
	}

	public override void Close()
	{
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");

		DataCenter.CloseWindow ("BACK_GROUP_ROLE_WINDOW");
        base.Close();
	}

    private void SetSelectPageBtn(string windowName)
    {
        switch (windowName)
        {
            case "ROLE_INFO_WINDOW":
                GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI, "RoleInfoBtn"));
                break;
            case "STAR_LEVEL_UP_WINDOW":
                GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI, "RoleEvolutionBtn"));
                break;
            case "ROLE_EQUIP_CULTIVATE_WINDOW":
                GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI, "RoleEquipCultivateBtn"));
                break;
            case "ROLE_EQUIP_COMPOSITION_WINDOW":
                GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI, "RoleEquipCompositionBtn"));
                break;
        }
    }

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

        if (keyIndex == "SET_SEL_PAGE")
        {
			InitButtonIsUnLockOrNot();
            SetSelectPageBtn((string)objVal);
        }
	}
}

public class RoleInfoSkinWindow : tWindow
{
	public RoleInfoSkinWindow()
	{
        mGameObjUI = GameCommon.FindUI("RoleInfoSkinWindow");
		
		Close();
	}
	public override  void Init()
	{
		
	}
}

public class RoleInfoUpdateWindow : tWindow
{
	public RoleInfoUpdateWindow()
	{
		mGameObjUI = GameCommon.FindUI("RoleInfoUpdateWindow");
		
		Close();
	}
	public override  void Init()
	{
		
	}
}