using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class ScrollWorldChapterName : tWindow
{
	Boss_GetDemonBossList_BossData _bossData;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_boss_appear_infos_btn", new DefineFactory<Button_boss_appear_infos_btn>());
	}

    public override void Open(object param)
    {
        base.Open(param);
		NetManager.RequestGetAppearBossList();
        SetChapterName();
//		SetBossTips(_bossData);
    }

    public override void onChange(string keyIndex, object objVal)
    {
         base.onChange(keyIndex, objVal);
         switch (keyIndex) 
         {
             case "SET_CHAPTER_NAME":
                 SetChapterName();
                 break;
		case "LEAVE_BOSS_DATA":
			SetBossTips((Boss_GetDemonBossList_BossData)objVal);
			break;
		case "APPEAR_BOSS_DATA":
			Boss_GetDemonBossList_BossData[] retBossList = (Boss_GetDemonBossList_BossData[])objVal;
			if (retBossList != null && retBossList.Length > 0)
			{
				for (int i = 0; i < retBossList.Length; i++)
				{
					if(retBossList[i].finderId == CommonParam.mUId)
					{
						SetBossTips (retBossList[i]);
//						DataCenter.SetData("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW", "LEAVE_BOSS_DATA", retBossList);
					}
				}
			}
			else 
				SetBossTips(_bossData);
			break;
         }
    }

    /// <summary>
    /// 设置章节名称
    /// </summary>
    /// <param name="kObjVal"></param>
    private void SetChapterName() 
    {
        UILabel _chapterNameLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "chapter_name_label");
        if (_chapterNameLbl != null) 
        {
            _chapterNameLbl.text = GetCurChapterName();
        }
    }

    private string GetCurChapterName() 
    {
        int _index = (ScrollWorldMapWindow.mDifficulty * 1000) + ScrollWorldMapWindow.mPage + 1;
        return TableCommon.GetStringFromStageFather(_index, "NAME");
    }
	void SetBossTips(Boss_GetDemonBossList_BossData bossData)
	{
		GameObject bossInfoObj = GameCommon.FindObject (mGameObjUI, "boss_appear_infos_btn");
        if (bossInfoObj == null)
            return;
        bossInfoObj.SetActive (false);
		if(bossData == null)
			return;
		DataRecord bossConfig = DataCenter.mBossConfig.GetRecord(bossData.tid);
		int starLevel = bossConfig.getData ("STAR_LEVEL");
		if(bossData.finderId == CommonParam.mUId)
		{
			bossInfoObj.SetActive (true);
			string text = GameCommon.SetStrQualityColor(bossData.quality)+ bossConfig.getData("NAME") + "[-]";
			SetText("boss_name_label", text);
			GameCommon.FindObject (mGameObjUI, "boss_name_label").GetComponent<UILabel>().color = GameCommon.GetNameColor(bossData.tid);
			SetUISprite("boss_icon", bossConfig.getData("HEAD_ATLAS_NAME"), bossConfig.getData("HEAD_SPRITE_NAME"));
			GameCommon.GetButtonData (bossInfoObj).set ("BOSS_INFOS_DATA", bossData);
			if(CommonParam.isPveBossFirst == 1)
			{
				DataCenter.OpenWindow ("BOSS_PK_TIPS_WINDOW", bossData);
				CommonParam.isPveBossFirst = 0;
			}
		}
	}
}

public class Button_boss_appear_infos_btn : CEvent
{
	public override bool _DoEvent()
	{
		Boss_GetDemonBossList_BossData _bossData = (Boss_GetDemonBossList_BossData)getObject ("BOSS_INFOS_DATA");
		DataCenter.OpenWindow ("BOSS_PK_TIPS_WINDOW", _bossData);
		return true;
	}
}