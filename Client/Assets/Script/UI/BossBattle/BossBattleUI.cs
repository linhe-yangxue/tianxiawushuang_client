using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;
using UnityEngine;

public class BossBattleWindow : tWindow
{
	public override void Open(object param)
	{
		DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
		DataCenter.OpenWindow("BATTLE_PLAYER_EXP_WINDOW");
		DataCenter.OpenWindow("BATTLE_BOSS_HP_WINDOW");
		//DataCenter.OpenWindow("PET_ATTACK_BOSS_WINDOW");
		DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
		DataCenter.OpenWindow("BOSS_ATTACK_INFO_WINDOW");
		DataCenter.OpenWindow("PVE_TOP_RIGHT_WINDOW"); 
        //DataCenter.OpenWindow("BATTLE_AUTO_FIGHT_BUTTON");
		
		//DataCenter.OpenWindow("MASK_OPERATE_WINDOW");
		
        //by chenliang
        //begin

//		MainProcess.mStage.Start();
//---------------
        //移到加载结束后调用

        //end
	}
}
//-------------------------------------------------------------------------

//-------------------------------------------------------------------------
public class BossAttackInfoWindow : tWindow
{
	static int msMaxSortCount = 5;
	
	public override void OnOpen()
	{
		GameCommon.SetUIButtonEnabled(mGameObjUI, "set_button", true);
		GameCommon.SetUIText(mGameObjUI, "gold_num", "0");
	}
	
	public override bool Refresh(object param)
	{        
		NiceTable damageList = param as NiceTable;
		if (damageList==null)
			return false;
		
		BossBattle battle = MainProcess.mStage as BossBattle;
		if (battle==null)
			return false;
		
		DataRecord[] sortInfo = new DataRecord[msMaxSortCount];
		
		DataRecord selfDamage = damageList.GetRecord(RoleLogicData.Self.mDBID);
		
		int count = 0;
		bool bHaveSelf = false;
		for (int i = 0; i < msMaxSortCount; ++i)
		{
			if (damageList.GetRecordCount()<=0)
				break;
			
			int nDamage = -1;
			DataRecord maxDamage = null;
			foreach (KeyValuePair<int, DataRecord> kv in damageList.GetAllRecord())
			{
				DataRecord r = kv.Value;
				if (maxDamage==null || r.get("DAMAGE")>nDamage)
				{
					nDamage = r.get("DAMAGE");
					maxDamage = r;
				}
			}
			if ((int)maxDamage.get(0) == RoleLogicData.Self.mDBID)
				bHaveSelf = true;
			damageList.DeleteRecord((int)maxDamage.get(0));
			sortInfo[i] = maxDamage;
			++count;
		}
		
		if (selfDamage == null)
			bHaveSelf = true;
		
		GameObject gridObj = GetSub("grid");
		if (gridObj!=null)
		{
			UIGridContainer grid = gridObj.GetComponent<UIGridContainer>();
			if (grid!=null)
			{
				int bossMaxHp = battle.mBoss.GetMaxHp();
				if (!bHaveSelf)
					grid.MaxCount = count+1;
				else
					grid.MaxCount = count;
				for (int i=0; i<count; ++i)
				{
					GameObject item = grid.controlList[i];
					string nameInfo = (i+1).ToString()+"【"+sortInfo[i].get("ATTACKER_NAME")+"】";
					GameCommon.SetUIText(item, "attacker_name", nameInfo);
					int damage = sortInfo[i].get("DAMAGE");
					int rate = (int)((float)damage/bossMaxHp * 100);
					GameCommon.SetUIText(item, "damage_info", damage.ToString()+"("+rate.ToString()+"%)");
				}
				if (!bHaveSelf)
				{
					GameObject item = grid.controlList[count];
					string nameInfo = "【"+RoleLogicData.Self.name+"】";
					GameCommon.SetUIText(item, "attacker_name", nameInfo);
					int damage = selfDamage.get("DAMAGE");
					int rate = (int)((float)damage / bossMaxHp * 100);
					GameCommon.SetUIText(item, "damage_info", damage.ToString() + "(" + rate.ToString() + "%)");
				}
			}
		}
		return true;
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "ENABLE_SETTING_BTN":
			bool bIsEnabled = (bool)objVal;
			GameCommon.SetUIButtonEnabled(mGameObjUI, "set_button", bIsEnabled);
			
			if (!bIsEnabled)
				DataCenter.CloseWindow("BATTLE_SETTINGS_WINDOW");
			
			break;
		}
	}
}
