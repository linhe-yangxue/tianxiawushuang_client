using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class PetAlbumCampInfoWindow : tWindow
{
    public override void Init()
    {
		EventCenter.Self.RegisterEvent("Button_pet_album_camp_info_close_Btn", new DefineFactoryLog<Button_pet_album_camp_info_close_Btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);
		InitUI (param);
    }

	public void InitUI(object param)
	{
		if (param is int)
        {
            int index = (int)param;
            if (index == 3)
            {
                index = 4; 
            }
            else if (index == 4) 
            { 
                index = 3; 
            }

			//desc
			string desc = TableCommon.GetStringFromConfig(index, "DESCRIBE", DataCenter.mKingdomDescribe);
			SetText("desc", desc);

			//Board
			string excellentPet = TableCommon.GetStringFromConfig(index, "EXCELLENT_PET", DataCenter.mKingdomDescribe);
			List<string> tidList = GetPetList(excellentPet, '|');

			//resetpos
			GameCommon.ResetScrollViewPosiTion(mGameObjUI, "role_tips_scroll_view");
			UIGridContainer grid = GetUIGridContainer("grid_pet");
			grid.MaxCount = tidList.Count;
			var gridList = grid.controlList;
			for(int i = 0; i < tidList.Count; i++)
			{
				RefreshBoard(gridList[i], tidList[i]);
			}
		}
	}

	public List<string> GetPetList(string petStr, char petChar)
	{
		List<string> tidList = new List<string>();
		string[] strArr = petStr.Split(petChar);
		for(int i = 0; i < strArr.Length; i++)
		{
			string temp = strArr[i];
			if(temp != "")
			{
				tidList.Add(temp);
			}
		}
		return tidList;
	}

    public string GetStringByStarLevel(string tid)
    {
        string ret = "";
        if(tid != "")
        {
            int tempTid = int.Parse(tid);
            int starLevel = GameCommon.GetItemStarLevel(tempTid);
            switch (starLevel)
            {
                case 2:
                    ret = "绿色最强队";
                    break;
                case 3:
                    ret = "蓝色最强队";
                    break;
                case 4:
                    ret = "紫色最强队";
                    break;
                case 5:
                    ret = "橙色最强队";
                    break;
            }
        }
        return ret;
    }

	public void RefreshBoard(GameObject board, string iconStr)
	{
		if(board != null)
		{
            //data
            List<string> strList = GetPetList(iconStr, '#');

            //quality label
            if (strList.Count > 0)
            {
                GameCommon.SetUIText(board, "quality_label", GetStringByStarLevel(strList[0]));
            }

            //grid icon
			UIGridContainer gridIcon = GameCommon.FindObject(board, "grid_icon").GetComponent<UIGridContainer>();
			gridIcon.MaxCount = strList.Count;
			var iconList = gridIcon.controlList;
			for(int i = 0; i < strList.Count; i++)
			{
                GameObject boardIcon = iconList[i];
                if (boardIcon != null && strList[i] != "")
				{
					int tid = int.Parse(strList[i]);
                    GameCommon.SetOnlyItemIcon(boardIcon, "icon", tid);
                    GameCommon.SetUIText(boardIcon, "num", "");
                    GameCommon.SetUIText(boardIcon, "name", GameCommon.GetItemName(tid));
                    GameCommon.FindObject(boardIcon, "name").GetComponent<UILabel>().color = GameCommon.GetNameColor(tid);
				}
			}
		}
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "INITUI":
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

public class Button_pet_album_camp_info_close_Btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.petAlbumCampInfo);
        return true;
    }
}

