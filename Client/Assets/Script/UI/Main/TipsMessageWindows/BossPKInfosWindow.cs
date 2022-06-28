using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class BossPKInfosWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_boss_pk_now_btn", new DefineFactory<Button_boss_pk_now_btn>());
		EventCenter.Self.RegisterEvent ("Button_boss_pk_later_btn", new DefineFactory<Button_boss_pk_later_btn>());
	}

	public override void Open(object param)
	{
		base.Open(param);
		ShowUI((Boss_GetDemonBossList_BossData)param);
	}
	private void ShowUI(Boss_GetDemonBossList_BossData _bossData)
	{
		int bossID = _bossData.tid;
		DataRecord bossConfig = DataCenter.mBossConfig.GetRecord(bossID);
		UILabel textLabel = GameCommon.FindObject (mGameObjUI, "text").GetComponent<UILabel>();
		if (textLabel != null)
		{
			string showText = "";
			string _name = bossConfig.getData("NAME");
			int starLevel = bossConfig.getData ("STAR_LEVEL");
			string strName = GameCommon.SetStrQualityColor(_bossData.quality) + _name ;
			string strLevel =  _bossData.bossLevel + "级";
			showText = TableCommon.getStringFromStringList (STRING_INDEX.TRIAL_ISPK_DESCRIPTION_FEAST);
			showText = string.Format (showText, strName, strLevel, "[-]");	
			textLabel.text = showText;
		}	

		NiceData buttonData = GameCommon.GetButtonData(mGameObjUI, "boss_pk_now_btn");
		if (buttonData != null)
			buttonData.set("BOSS_DATA", _bossData);

		ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "BossModelPosition");
		if (birthUI != null)
		{
			birthUI.mBirthConfigIndex = bossID;
			birthUI.mObjectType = (int)OBJECT_TYPE.BIG_BOSS;
			birthUI.Init();
			if (birthUI.mActiveObject != null)
			{
				birthUI.mActiveObject.SetScale(100f);
				birthUI.mActiveObject.PlayAnim("idle");
				
				GameObject tmpObj = null;
				if(birthUI.mActiveObject.mMainObject != null)
				{
					tmpObj = birthUI.mActiveObject.mMainObject;
					if(tmpObj != null && tmpObj.transform.parent != null)
						tmpObj = tmpObj.transform.parent.gameObject;
					if(tmpObj != null && tmpObj.transform.parent != null)
						tmpObj = tmpObj.transform.parent.gameObject;
				}
				if (tmpObj != null && tmpObj.name == "3d_scene")
				{
					Vector3 tmpPos = tmpObj.transform.localPosition;
					tmpPos.x = -100000.0f;
					tmpObj.transform.localPosition = tmpPos;
				}
				
				BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
				if (modelScript != null)
					modelScript.mActiveObject = birthUI.mActiveObject;
			}
		}
	}
	public override void OnClose ()
	{
		base.OnClose ();
		GameObject.Destroy (mGameObjUI);
		//		card = GameCommon.FindObject(mGameObjUI, "breakAndFateCard");
		//		foreach(Transform child in card.transform)
		//		{
		//			GameObject.Destroy(child.gameObject);
		//		}
	}

}
public class Button_boss_pk_now_btn : CEvent
{
	public override bool _DoEvent()
	{
		Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
		
		if (bossData != null)
		{
			DataCenter.CloseWindow("BOSS_PK_TIPS_WINDOW");
			DataRecord config = DataCenter.mBossConfig.GetRecord(bossData.tid);
			DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "BOSS_DATA", bossData);
			DataCenter.OpenWindow("BOSS_STAGE_INFO_WINDOW", (int)config.get("SCENE_ID"));
		}
		else
		{
			Log("ERROR: No set boss data to start button [BOSS_DATA]");
		}

		return true;
	}
}
public class Button_boss_pk_later_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("BOSS_PK_TIPS_WINDOW");
		return true;
	}
}