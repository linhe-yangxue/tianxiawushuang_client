using UnityEngine;
using System.Collections;

public class CommonTipWindow : tWindow {

	UILabel mDescriptionLabel;
	Transform mLabelBg;
	UISprite mLabelBgSprite;

	GameObject mParentObjBtn;

	Transform mWindowBg;
	UISprite mWindowBgSprite;
	BoxCollider mWindowBgBox;

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case "INIT_WINDOW":
			InitWindow(objVal);
			break;
		}
	}

	public void InitWindow(object objVal)
	{
		if(mGameObjUI != null)
		{
			mParentObjBtn = objVal as GameObject;
			mDescriptionLabel = mGameObjUI.transform.Find("tip/label").GetComponent<UILabel>();
			mLabelBg = mGameObjUI.transform.Find("tip");
			mLabelBgSprite = mLabelBg.GetComponent<UISprite>();

			mWindowBg = mGameObjUI.transform.Find("tip_background");
			mWindowBgSprite = mWindowBg.GetComponent<UISprite>();
			mWindowBgBox = mWindowBg.GetComponent<BoxCollider>();
			mGameObjUI.transform.parent = mParentObjBtn.transform;


			Refresh(null);
		}
	}

	public override bool Refresh (object param)
	{
		if(mGameObjUI != null)
		{			
			int iIndex = mParentObjBtn.GetComponent<UIButtonEvent>().mData.get ("INDEX");
			string strTable = mParentObjBtn.GetComponent<UIButtonEvent>().mData.get ("TABLE_NAME");
			SetText(iIndex, strTable);
			SetHight();
			SetPosition();
			SetWindowBgSpriteDepth();
		}

		return base.Refresh (param);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
	}

	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public void SetText(int iIndex, string strTable)
	{
		string strTitle = "";
		string strInfo = "";
		switch(strTable)
		{
		case "SKILL":
            SkillInfo skillInfo = SkillGlobal.GetInfo(iIndex);
			strTitle = skillInfo.title;//TableCommon.GetStringFromSkillConfig(iIndex, "TIP_TITLE");
			strInfo = skillInfo.describe;//TableCommon.GetStringFromSkillConfig(iIndex, "INFO");
			break;
		case "ATTACK_STATE":
			strTitle = TableCommon.GetStringFromAttackState(iIndex, "TIP_TITLE");
			strInfo = TableCommon.GetStringFromAttackState(iIndex, "INFO");
			break;
		case "AFFECT_BUFFER":
            BuffInfo buffInfo = BuffGlobal.GetInfo(iIndex);
			strTitle = buffInfo.title;//TableCommon.GetStringFromAffectBuffer(iIndex, "TIP_TITLE");
			strInfo = buffInfo.describe;//TableCommon.GetStringFromAffectBuffer(iIndex, "INFO");
			break;
		case "ActiveObject":
			strTitle = TableCommon.GetStringFromActiveCongfig(iIndex, "TIP_TITLE");
			strInfo = "\n[sub]"+ TableCommon.GetStringFromActiveCongfig(iIndex, "DESCRIBE");
			break;
			//新加Tips
		case "ITEM_TYPE_PET_FRAGMENT":
			strTitle = TableCommon.GetStringFromFragment (iIndex, "NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromFragment(iIndex, "DESCRIPTION");
			break ;
		case "ITEM_TYPE_PET":
			strTitle = TableCommon.GetStringFromActiveCongfig(iIndex, "TXT");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromActiveCongfig(iIndex, "DESCRIBE");
			break ;
		case "ITEM_TYPE_ROLE_EQUIP":
			strTitle = TableCommon.GetStringFromRoleEquipConfig(iIndex, "NAME");
			strInfo = "\n[sub]"+ TableCommon.GetStringFromRoleEquipConfig(iIndex, "DESCRIPTION");
			break ;
		case "ITEM_TYPE_ROLE_EQUIP_MATERIAL":
			strTitle = TableCommon.GetStringFromMaterialConfig (iIndex, "NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromMaterialConfig(iIndex, "DESCRIPTION");
			break ;
		case "ITEM_TYPE_ROLE_EQUIP_MATERIAL_FRAGMENT":
			strTitle = TableCommon.GetStringFromMaterialFragment (iIndex, "NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromMaterialFragment(iIndex, "DESCRIPTION");
			break ;
		case "ITEM_TYPE_STONE":
			strTitle = TableCommon.GetStringFromStoneTypeIconConfig (iIndex, "NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromStoneTypeIconConfig(iIndex, "DESCRIPTION");
			break ;
		case "ITEM_TYPE_TOOLITEM":
			strTitle = TableCommon.GetStringFromConsumeConfig (iIndex, "ITEM_NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromConsumeConfig(iIndex, "DESCRIBE");
			break ;
		case "ITEM_TYPE_ITEMICON":
			strTitle = TableCommon.GetStringFromItemIconConfig(iIndex, "NAME");
			strInfo ="\n[sub]"+ TableCommon.GetStringFromItemIconConfig(iIndex, "NAME");
			break ;
			
		}

		if(strTitle != "" || strInfo != "")
		{
			if(strTitle != "")
				strTitle += "[sup]";

			strInfo =  strTitle + strInfo;
			strInfo = strInfo.Replace("\\n", "\n");
			
			mDescriptionLabel.text = strInfo;
		}
	}

	public void SetHight()
	{
		mLabelBgSprite.height = mDescriptionLabel.height + 20;
	}

	public void SetPosition()
	{
		Transform btnBgTran = mParentObjBtn.transform.Find("background");

		float x = 0;
		float y = 0;
		float z = 0;

		Camera camera = GameCommon.FindUI("Camera").GetComponent<Camera>();
		Vector3 pos = camera.WorldToScreenPoint(btnBgTran.position);         //获取UI界面的屏幕坐标

		if(pos.x + mLabelBgSprite.width > Screen.width)
		{
			x = 0 - mLabelBgSprite.width;
		}

		if(mLabelBgSprite.height > pos.y)
		{
			y = 0 + mLabelBgSprite.height;
		}
		mGameObjUI.transform.localPosition = new Vector3(x, y, z);
		mWindowBg.position = camera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 0));
	}

	public void SetWindowBgSpriteDepth()
	{
		mWindowBgSprite.depth = mParentObjBtn.transform.Find("background").GetComponent<UISprite>().depth - 10;
	}
	
}
