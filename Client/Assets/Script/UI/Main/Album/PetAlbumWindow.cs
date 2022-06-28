using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

class PetAlbumWindow : tWindow
{
    class CS_PetAlbum : GameServerMessage { }
    class SC_PetAlbum : RespMessage
    {
        public readonly int[] tidArr;
    }
    int[] tidArr = new int[0];

    public struct PetAlbumInfo
    {
        public readonly int tid;
        public readonly ElementType elementType;
        public readonly int level;
        public readonly string name;
        public readonly int firstShow;

        public PetAlbumInfo(int tid)
        {
            this.tid = tid;
            int elementIndex = (int)DataCenter.mActiveConfigTable.GetRecord(tid).getData("ELEMENT_INDEX");
            if (elementIndex < 0 || elementIndex > 4) elementIndex = 0;
            elementType = (ElementType)elementIndex;
            level = DataCenter.mActiveConfigTable.GetRecord(tid).getData("STAR_LEVEL");
            name = (string)DataCenter.mActiveConfigTable.GetRecord(tid).getData("NAME");
            firstShow = (int)DataCenter.mActiveConfigTable.GetRecord(tid).getData("FIRST_SHOW");
        }
    }

    public enum ElementType
    {
        Fire = 0,
        Water,
        Leaf,
        Light,
        Shadow
    }

    string[] toggleNameArr = new string[5] { "fire", "water", "leaf", "shadow", "light" };
    Dictionary<ElementType, List<PetAlbumInfo>> petDict;
	
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_pet_album_camp_info_Btn", new DefineFactoryLog<Button_pet_album_camp_info_Btn>());
	}

	protected override void OpenInit()
    {
        base.OpenInit();
        Func<string, Tuple<ElementType, string>> getElementTuple = name =>
		{
			switch (name) {
			case "fire":
				return new Tuple<ElementType, string> (ElementType.Fire, "赤炎进度");
			case "water":
				return new Tuple<ElementType, string> (ElementType.Water, "碧渊进度");
			case "leaf":
				return new Tuple<ElementType, string> (ElementType.Leaf, "森罗进度");
			case "shadow":
				return new Tuple<ElementType, string> (ElementType.Shadow, "玄冥进度");
			case "light":
				return new Tuple<ElementType, string> (ElementType.Light, "乾阳进度");
			default:
				return new Tuple<ElementType, string> (ElementType.Fire, "赤炎进度");
			}
		};

        toggleNameArr.Foreach(toggle => AddButtonAction(toggle, () =>
        {
            var tuple = getElementTuple(toggle);
			SetToggleArrIndex(tuple.field1);
			RefreshContainer(tuple.field1, tuple.field2);
        }));

    }

	public void SetToggleArrIndex(ElementType type)
	{
		DataCenter.Set("PET_ALBUM_TOGGLE_ARR_INDEX", (int)type);
	}


    public override void OnOpen()
    {
        base.OnOpen();
        DataCenter.OpenBackWindow(UIWindowString.petAlbum, "a_ui_fltj_logo", () => MainUIScript.Self.ShowMainBGUI());

		//init type
		SetToggleArrIndex (ElementType.Fire);

        GetUIToggle(toggleNameArr[0]).value = true;
        SetDict();


        HttpModule.CallBack requestSuccess = text =>
        {
            SC_PetAlbum item = JCode.Decode<SC_PetAlbum>(text);
            tidArr = item.tidArr;
            RefreshContainer(ElementType.Fire, "赤炎进度");
        };
        HttpModule.Instace.SendGameServerMessageT(new CS_PetAlbum(), requestSuccess, NetManager.RequestFail);


    }

    void SetDict()
    {
        petDict = new Dictionary<ElementType, List<PetAlbumInfo>>() {
            {ElementType.Fire,new List<PetAlbumInfo>()},
            {ElementType.Water,new List<PetAlbumInfo>()},
            {ElementType.Leaf,new List<PetAlbumInfo>()},
            {ElementType.Shadow,new List<PetAlbumInfo>()},
            {ElementType.Light,new List<PetAlbumInfo>()},                    
        };

        DataCenter.mActiveConfigTable.GetAllRecord().Values.Foreach(record =>
        {
            if (record.getData("STAR_LEVEL") > 1
                && record.getData("CLASS") != "CHAR"
                && record.getData("IS_TUJIAN_SHOW") == 1)
            {
                int tid = record.getData("INDEX");
                var petAlbumInfo = new PetAlbumInfo(tid);
                petDict[petAlbumInfo.elementType].Add(petAlbumInfo);
            }
        });
    }

    void RefreshContainer(ElementType elementType, string elementText)
    {
        int lineCount = 0;
        float startPos = 270;
        var list = petDict[elementType];
        var dict = new Dictionary<int, List<PetAlbumInfo>>();
        list.ForEach(info =>
        {
            if (!dict.ContainsKey(info.level)) dict.Add(info.level, new List<PetAlbumInfo>());
            dict[info.level].Add(info);
        });
        /*目前版本没有红符灵（level6），故i的值从3开始（3+2=5）*/
        int petTotal = 0;
        for (int i = 3; i >= 0; i--)
        {
            int level = i + 2;
            var lvContainer = GetCurUIGameObject("level" + level);
            var grid = lvContainer.transform.FindChild("grid").GetComponent<UIGridContainer>();
            grid.MaxCount = 0;
            if (dict.ContainsKey(level))
            {
                grid.MaxCount = dict[level].Count;
                lvContainer.transform.FindChild("bg").GetComponent<UISprite>().height = 125 + ((grid.MaxCount - 1) / 9) * 100;
                petTotal += grid.MaxCount;
                for (int j = 0; j < dict[level].Count; j++)
                {
                    int tid = dict[level][j].tid;
                    var icon = GameCommon.FindObject(grid.controlList[j], "itemIcon");
                    UISprite sprite = icon.GetComponent<UISprite>();
                    sprite.color = tidArr.Contains(tid) ? new Color(1, 1, 1) : new Color(.5f, .5f, .5f);
                    GameCommon.SetItemIcon(sprite, PackageManager.GetItemTypeByTableID(tid), tid);
                    AddButtonAction(icon, () => DataCenter.OpenWindow(UIWindowString.petDetail, tid));
                    UILabel nameLabel = GameCommon.FindObject(grid.controlList[j], "name").GetComponent<UILabel>();
                    GameObject adviceIcon = GameCommon.FindObject(grid.controlList[j], "advice_icon");
                    adviceIcon.SetActive(false);
                    if (dict[level][j].name != null)
                    {
                        nameLabel.text = dict[level][j].name;
                        nameLabel.color = GameCommon.GetNameColor(dict[level][j].tid);
                    }
                    else nameLabel.gameObject.SetActive(false);
                    if (dict[level][j].firstShow != null)
                    {
                        if (dict[level][j].firstShow == 1) adviceIcon.SetActive(true);
                    }
                }
            }
            var newPos = startPos - 150 - lineCount * 100;
            lvContainer.transform.localPosition = new Vector3(0, newPos, 0);
            startPos = newPos;
            //int petTotal=DataCenter.mActiveConfigTable.GetAllRecord().Values.Where(record => record.getData("ELEMENT_INDEX")==(int)elementType).ToArray().Length;

            int petNum = 0;
            tidArr.Foreach(tid =>
            {
                if ((int)DataCenter.mActiveConfigTable.GetRecord(tid).getData("IS_TUJIAN_SHOW") == 1)
                {
                    int elementIndex = (int)DataCenter.mActiveConfigTable.GetRecord(tid).getData("ELEMENT_INDEX");
                    if (elementIndex < 0 || elementIndex > 4) elementIndex = 0;
                    var curElementType = (ElementType)elementIndex;
                    if (curElementType == elementType) petNum++;
                }
            });

            GetUILabel("petNum").text = ((petNum.ToString()).SetTextColor(LabelColor.Green));
            GetUILabel("petTotal").text = petTotal.ToString();
            GetUILabel("elementSchedule").text = elementText;
            lineCount = (grid.MaxCount - 1) / 9;
        }

        //刷新滚动面板位置
        UIScrollView _scrollView = GetSub("ScrollView").GetComponent<UIScrollView>();
        _scrollView.ResetPosition();
    }

    public override void OnClose()
    {
        base.OnClose();
        DataCenter.CloseWindow(UIWindowString.common_back);
        //MainUIScript.Self.ShowMainBGUI ();
    }
}

public class Button_ToAlbum : Logic.CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow(UIWindowString.petAlbum);
        MainUIScript.Self.HideMainBGUI();
        //added by xuke 红点相关
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_ATLAS, false);
        //end
        return true;
    }
}

public class Button_pet_album_camp_info_Btn:CEvent
{
	public override bool _DoEvent()
	{
		int xx = DataCenter.Get ("PET_ALBUM_TOGGLE_ARR_INDEX");
		DataCenter.OpenWindow(UIWindowString.petAlbumCampInfo, xx);
		return true;
	}
}



