using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

/// <summary>
/// 属性加成
/// </summary>

class RammbockAttriNeedStarsData
{
    public int m_min = -1;
    public int m_max = -1;
    public int m_starNum = 0;
}
class RammbockAttriNeedStars
{
    public static List<RammbockAttriNeedStarsData> sm_stars = new List<RammbockAttriNeedStarsData>()
    {
        new RammbockAttriNeedStarsData() { m_min = 300101, m_max = 300120, m_starNum = 3 },
        new RammbockAttriNeedStarsData() { m_min = 300121, m_max = 300140, m_starNum = 6 },
        new RammbockAttriNeedStarsData() { m_min = 300141, m_max = 300160, m_starNum = 9 }
    };

    public static int GetNeedStarsNumber(int buffID)
    {
        int num = 0;
        for (int i = 0, count = sm_stars.Count; i < count; i++)
        {
            if (buffID >= sm_stars[i].m_min && buffID <= sm_stars[i].m_max)
            {
                num = sm_stars[i].m_starNum;
                break;
            }
        }
        return num;
    }
}

/// <summary>
/// 用于群魔乱舞每章节通关后属性加成选择界面
/// </summary>
public class RammbockAttriAddWindow : tWindow
{
	public int m_CurSelectBuffIndex = -1; 	//> 当前选中的BUFF index;
	public int m_NeedStarNum = 0;			//> 需要星星的数量
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_attribute_btn", new DefineFactoryLog<Button_Rammbock_attribute_btn>());
        EventCenter.Self.RegisterEvent("Button_addition_attribute_close_button", new DefineFactoryLog<Button_Rammbock_Attri_close_button>());
		EventCenter.Self.RegisterEvent ("Button_addition_attribute_continue_button",new DefineFactoryLog<Button_addition_attribute_continue_button>());

    }

    public override void Open(object param)
    {
        base.Open(param);

		IintInfo ();
        Refresh(param);

		UpdateBuffSelectState ();
    }

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch (keyIndex) 
		{
		case "UPDATE_BUFF_SELECT_STATE":
			UpdateBuffSelectState(true);
			break;
		}
	}


    public override bool Refresh(object param)
    {
        int[] chooseBuff = param as int[];

        __RefreshChooseBuffData(chooseBuff);
        __RefreshExistBuffData();

        return true;
    }

	private void UpdateBuffSelectState(bool kIsShow = false)
	{
		UIGridContainer grid = GetSub ("addition_group").GetComponent<UIGridContainer>();
		GameObject _selectFrameObj = GetSub ("select_buff_state");
		if (!kIsShow && _selectFrameObj != null) 
		{
			_selectFrameObj.SetActive(false);
			return;
		}
		
		GameObject _curBuffObj = grid.controlList [m_CurSelectBuffIndex];
		GameObject _curBuffBg = GameCommon.FindObject (_curBuffObj,"icon_bg_4");
		_selectFrameObj.SetActive(true);
		_selectFrameObj.transform.parent = _curBuffBg.transform;
		_selectFrameObj.transform.localPosition = Vector2.zero;
		_selectFrameObj.transform.parent = mGameObjUI.transform;
	}
	
	private void IintInfo()
	{
		m_CurSelectBuffIndex = -1;
		m_NeedStarNum = 0;
	}

    /// <summary>
    /// 刷新备选Buff数据
    /// </summary>
    /// <param name="chooseBuff"></param>
    private void __RefreshChooseBuffData(int[] chooseBuff)
    {
        for (int i = 0, count = chooseBuff.Length; i < count; i++)
        {
            int buffID = chooseBuff[i];
            GameObject itemParent = GetSub("group(Clone)_" + i.ToString());
            if (itemParent == null)
                continue;
            DataRecord affectConfig = DataCenter.mAffectBuffer.GetRecord(buffID);
            if (affectConfig == null)
            {
                GameCommon.SetUIVisiable(itemParent, "item_icon", false);
                GameCommon.SetUIVisiable(itemParent, "attribute_label01", false);
                GameCommon.SetUIText(itemParent, "star_number", "--");
                continue;
            }

            //Buff Icon
            GameCommon.SetUIVisiable(itemParent, "item_icon", true);
            GameCommon.SetIcon(itemParent, "item_icon", affectConfig.getData("SKILL_SPRITE_NAME"), affectConfig.getData("SKILL_ATLAS_NAME"));

            //Buff Name
            GameCommon.SetUIVisiable(itemParent, "attribute_label01", true);
            //string buffName = affectConfig.getObject("NAME").ToString() + "\n" + affectConfig.getObject("INFO").ToString();
			string buffName = affectConfig.getObject("NAME").ToString();
			string addInfo = affectConfig.getObject("INFO").ToString();
			string addValue = addInfo.Substring(addInfo.IndexOfAny(new char[]{'0','1','2','3','4','5','6','7','8','9'}));
            GameCommon.SetUIText(itemParent, "attribute_label01", buffName);
			GameCommon.SetUIText(itemParent,"addition_number","+" + addValue);
            //星数
            int needStarsNum = RammbockAttriNeedStars.GetNeedStarsNumber(buffID);
            GameCommon.SetUIText(itemParent, "star_number", needStarsNum.ToString());

            NiceData btnData = GameCommon.GetButtonData(itemParent, "attribute_btn");
            if (btnData != null)
            {
                btnData.set("BUFF_INDEX", i);
                btnData.set("NEED_STARS", needStarsNum);
            }
        }
    }
    /// <summary>
    /// 刷新现有Buff数据
    /// </summary>
    private void __RefreshExistBuffData()
    {
        GameObject parent = GetSub("attribute_label_info");
        if (parent == null)
            return;

        RammbockWindow win = DataCenter.Self.getData("RAMMBOCK_WINDOW") as RammbockWindow;

        //当前可用星
        GameCommon.SetUIText(parent, "star_number", win.m_climbingInfo.remainStars.ToString());

        //加成效果
        RammbockAttriBuffManager buffDataManager = win.m_buffDataManager;

		//加成效果
		int buffDataCount = 0;
		if (buffDataManager != null)
		{
			buffDataManager.Init();
			for (int i = 0, count = win.m_climbingInfo.buffList.Count; i < count; i++)
				buffDataManager.AddBuff(win.m_climbingInfo.buffList[i]);
		}

		List<RammbockAttriBuffData> _buffDataList = buffDataManager.__CombineSameBuffData();
		buffDataCount = _buffDataList.Count;
        //int buffDataCount = win.m_buffDataManager.BuffDataCount;
        for (int i = 0; i < 9; i++)
        {
            string buffName = "无加成";
            string buffValue = "";

			if (i < buffDataCount)
            {
//                RammbockAttriBuffData buffData = buffDataManager.GetBuffDataByIndex(i);
				RammbockAttriBuffData buffData = _buffDataList[i];
                buffName = buffData.Name;
                buffValue = buffData.ValueString;
            }
            GameObject subItem = GameCommon.FindObject(parent, "group(Clone)_" + i.ToString());
            GameCommon.SetUIVisiable(parent, "group(Clone)_" + i.ToString(), i < buffDataCount);
            GameCommon.SetUIText(subItem, "addition_effcet_label", buffName);
            GameCommon.SetUIText(subItem, "addition_effcet_number", buffValue);
        }
    }
}

/// <summary>
/// 属性加成选择按钮
/// </summary>
class Button_Rammbock_attribute_btn : CEvent
{
    public override bool _DoEvent()
    {
        int needStars = (int)getObject("NEED_STARS");
        RammbockWindow win = DataCenter.Self.getData("RAMMBOCK_WINDOW") as RammbockWindow;
		RammbockAttriAddWindow attrAddWin = DataCenter.Self.getData ("RAMMBOCK_ATTRI_ADD_WINDOW") as RammbockAttriAddWindow;


		int buffIndex = (int)getObject("BUFF_INDEX");
		attrAddWin.m_NeedStarNum = needStars;
		attrAddWin.m_CurSelectBuffIndex = buffIndex;
		
		if(win.m_climbingInfo.remainStars < needStars)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_RAMMBOCK_STARS_NO_ENGOUGH, true);

            return true;
        }

		DataCenter.SetData ("RAMMBOCK_ATTRI_ADD_WINDOW","UPDATE_BUFF_SELECT_STATE",true);

//        RammbockNetManager.RequestClimbTowerChooseBuff(buffIndex + 1);

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_Rammbock_Attri_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_ATTRI_ADD_WINDOW");

        return true;
    }
}

/// <summary>
/// 继续挑战按钮
/// </summary>
class Button_addition_attribute_continue_button : CEvent
{
	public override bool _DoEvent()
	{
		RammbockWindow win = DataCenter.Self.getData("RAMMBOCK_WINDOW") as RammbockWindow;
		RammbockAttriAddWindow attrAddWin = DataCenter.Self.getData ("RAMMBOCK_ATTRI_ADD_WINDOW") as RammbockAttriAddWindow;
		if (attrAddWin.m_CurSelectBuffIndex == -1) 
		{
			DataCenter.OpenMessageWindow("请选择一个BUFF!");
			return true;
		}
		if (win.m_climbingInfo.remainStars < attrAddWin.m_NeedStarNum) 
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_RAMMBOCK_STARS_NO_ENGOUGH, true);
			return true;
		}

		RammbockNetManager.RequestClimbTowerChooseBuff(attrAddWin.m_CurSelectBuffIndex + 1);
		return true;
	}
}
