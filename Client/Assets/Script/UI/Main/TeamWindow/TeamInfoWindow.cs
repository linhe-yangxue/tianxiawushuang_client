using UnityEngine;
using System.Collections;
using Logic;
using System;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;

public enum ADD_ATTRITUBE_VALUE_TYPE
{
	NONE,
	ATTACK,             // 攻击
	HP,                 // 生命
	PHYSICAL_DEFENCE,   // 物防
	MAGIC_DEFENCE,      // 法防
	MAX,
}

public class TeamInfoWindow : tWindow
{
    public UIGridContainer mTeamIconGrid;
    public UIGridContainer mTeamInfoGrid;
    public GameObject[] mCardList;
    private float mfCardScale = 1.0f;
    public GameObject mMastButtonUI;
    private PopupTip mPopupTip;

	public static Dictionary<ADD_ATTRITUBE_VALUE_TYPE , int> saveTeamAttribute = new Dictionary<ADD_ATTRITUBE_VALUE_TYPE, int>();
    //by chenliang
    //begin

    private Action mOpenCompleteCallback;       //窗口打开时回调函数
    public Action OpenCompleteCallback
    {
        set { mOpenCompleteCallback = value; }
        get { return mOpenCompleteCallback; }
    }

    private bool mCanClose = true;          //是否可以关闭窗口
    public bool CanClose
    {
        set { mCanClose = value; }
        get { return mCanClose; }
    }

    //end

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_team_pos_head_btn", new DefineFactory<TeamPosHeadBtn>());
        EventCenter.Self.RegisterEvent("Button_team_add_pos_btn", new DefineFactory<TeamPosAddBtn>());
        EventCenter.Self.RegisterEvent("Button_team_lock_pos_btn", new DefineFactory<TeamLockPosBtn>());

        EventCenter.Self.RegisterEvent("Button_equip_btn", new DefineFactory<TeamSlotEquipInfoBtn>());
        EventCenter.Self.RegisterEvent("Button_add_equip_btn", new DefineFactory<TeamSlotEquipAddBtn>());
        EventCenter.Self.RegisterEvent("Button_magic_btn", new DefineFactory<TeamSlotMagicInfoBtn>());
        EventCenter.Self.RegisterEvent("Button_add_magic_btn", new DefineFactory<TeamSlotMagicAddBtn>());

        EventCenter.Self.RegisterEvent("Button_strengthen_master_btn", new DefineFactory<Button_StrengthenMasterBtn>());
        EventCenter.Self.RegisterEvent("Button_key_to_strengthen_btn", new DefineFactory<Button_KeyToStrengthenBtn>());
        EventCenter.Self.RegisterEvent("Button_role_change_skin_btn", new DefineFactory<Button_RoleChangeSkinBtn>());
        EventCenter.Self.RegisterEvent("Button_pet_change_btn", new DefineFactory<Button_PetChangeBtn>());
        EventCenter.Self.RegisterEvent("Button_pet_back_btn", new DefineFactory<Button_PetBackBtn>());
		EventCenter.Self.RegisterEvent("Button_look_relate_btn", new DefineFactory<Button_look_relate_btn>());
		EventCenter.Self.RegisterEvent("Button_look_base_attribute_btn", new DefineFactory<Button_look_base_attribute_btn>());

        EventCenter.Self.RegisterEvent("Button_karma_button", new DefineFactory<Button_karma_button>());
    }

    public void AddAnimAlfa(GameObject obj)
    {
        if (obj.GetComponent<TweenAlpha>() == null)
        {
            TweenAlpha.Begin(obj, 1.0f, 0);
            obj.GetComponent<TweenAlpha>().style = UITweener.Style.PingPong;
        }
    }

    public virtual void InitWindow()
    {
        mMastButtonUI = GetSub("team_black");
        mTeamIconGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_icon_info_buttons_grid");
        mTeamInfoGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_info_grid");
        if (mTeamInfoGrid != null)
        {
            mCardList = new GameObject[mTeamInfoGrid.MaxCount];
            for (int i = 0; i < mTeamInfoGrid.MaxCount; i++)
            {
                GameObject obj = mTeamInfoGrid.controlList[i];

                GameObject card = GameCommon.FindObject(obj, "team_body_info_card");
                card.name = card.name + i.ToString();
                mCardList[i] = card;

                InitCard(i);

                //添加加号透明度变化
                if (obj != null)
                {
                    AddAnimAlfa(GameCommon.FindObject(obj, "add_sprite"));
                }

                obj.SetActive(false);
            }
        }
        else
        {
            DEBUG.LogError("team_info_grid is not exist");
        }

        //by chenliang
        //begin

//		GetSub("key_to_strengthen_btn").SetActive(CommonParam.isOnLineVersion);
//------------------
        //加空判断
        GameObject tmpGO = GetSub("key_to_strengthen_btn");
        if (tmpGO != null)
            tmpGO.SetActive(false);// 公测之前关闭

        //end
    }

    public override void onChange(string keyIndex, object objVal)
    {
        //by chenliang
        //begin

//        base.onChange(keyIndex, objVal);
//---------------
        if (keyIndex != "CLOSE")
            base.onChange(keyIndex, objVal);

        //end

        switch (keyIndex)
        {
            case "UPDATE_TEAM_POS_ICON":
                TeamPosIcon((int)objVal);
                break;
            case "RELATE_INFO":
                SwitchToRelateInfoPanel();
                break;
            case "UPDATE_CUR_TEAM_POS_INFO":
                UpdateUI();
                break;
		    case "SET_SAVE_ATTRIBUTE_VALUE":
			    SetAttributeValue((ActiveData)objVal);
			    SetLookRelateTips();
			    break;
            case "SET_SAVE_ATTRIBUTE_VALUE_ONLY":
                SetAttributeValue((ActiveData)objVal);
                break;
            case "REFRESH_CHANGE_NEWMARK":
                RefreshNewMark();
                break;
            //by chenliang
            //begin

            case "CLOSE":
                {
                    if (mCanClose)
                        base.onChange(keyIndex, objVal);
                }break;

            //end
		case "SET_BASE_INFOS":
            GameCommon.SetUIVisiable(mGameObjUI, "look_base_attribute_btn",false);
            GameCommon.SetUIVisiable(mGameObjUI, "look_relate_btn", true);
			break;
		case "SET_RELATE_INFOS":
			GameCommon.SetUIVisiable(mGameObjUI, "look_base_attribute_btn",true);
            GameCommon.SetUIVisiable(mGameObjUI, "look_relate_btn", false);
            GameCommon.SetUIVisiable(mGameObjUI, "look_relate_tips", false);
			break;
        }
    }

	void SetLookRelateTips()
	{
		GameCommon.FindObject (mGameObjUI, "look_base_attribute_btn").gameObject.SetActive (false);
		GameCommon.FindObject (mGameObjUI, "look_relate_btn").gameObject.SetActive (true);
		GameObject teamButtonsGroup = GameCommon.FindObject (mGameObjUI, "team_buttons_group").gameObject;
		GameObject lookRelateTipsObj = GameCommon.FindObject (mGameObjUI, "look_relate_tips").gameObject;
		int iTipsNum = UnityEngine.Random.Range(0,3);
		int iRelationCash = Relationship.GetCachedActiveRelateCount ((int)TeamManager.mCurTeamPos);
		int iRelationCount = Relationship.GetCachedRelationshipList ((int)TeamManager.mCurTeamPos).Count;
		lookRelateTipsObj.SetActive (false);
		GameCommon.FindComponent<UILabel>(lookRelateTipsObj, "relate_tips_label").text = TableCommon.getStringFromStringList(STRING_INDEX.RELATE_TIPS_DESCRIPTION);
				
		if(iRelationCash < iRelationCount)
		{
			if(iTipsNum == 1)
			{
				GameObject obj = GameObject.Instantiate (lookRelateTipsObj) as GameObject;
				obj.transform.parent = teamButtonsGroup.transform;
				obj.transform.localPosition = lookRelateTipsObj.transform.localPosition;
				obj.transform.localScale = lookRelateTipsObj.transform.localScale;
				obj.SetActive (true);
				GlobalModule.DoLater (() => GameObject.DestroyImmediate (obj), 2.0f);
//				TweenScale  twA = TweenScale.Begin (lookRelateTipsObj, 0.4f, new Vector3(1.2f,1.2f,1.2f));
//				TweenScale twB = lookRelateTipsObj.gameObject.GetComponent<TweenScale>();
//				twB.from = new Vector3(1.2f,1.2f,1.2f);
//				twB.to = Vector3.zero;
//				twB.duration = 1.0f;
//				twB.delay = 1.0f;
			}
		}
	}

	AFFECT_TYPE GetAttributeType(ADD_ATTRITUBE_VALUE_TYPE type)
	{
		if (type >= ADD_ATTRITUBE_VALUE_TYPE.ATTACK && type < ADD_ATTRITUBE_VALUE_TYPE.MAX)
		{
			string name = type.ToString();
			return GameCommon.GetEnumFromString<AFFECT_TYPE>(name, AFFECT_TYPE.NONE);
		}
		return AFFECT_TYPE.NONE;
	}
	//设置值
	public void SetAttributeValue(ActiveData mCurActiveData)
	{
		if (mCurActiveData != null)
		{
			ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
			float tmpAttribute = 0f;
			float tmpFinalATK = 0.0f;				
			
			for(int i = 0; i < 4; i++)
			{
				AFFECT_TYPE affectType = GetAttributeType((ADD_ATTRITUBE_VALUE_TYPE)(i + 1));	
				ADD_ATTRITUBE_VALUE_TYPE _AffectType =  (ADD_ATTRITUBE_VALUE_TYPE)(i + 1);
				if(affectType == AFFECT_TYPE.ATTACK)
				{
					tmpAttribute = GameCommon.GetBaseAttack(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
				}else if(affectType == AFFECT_TYPE.HP)
				{
					tmpAttribute = GameCommon.GetBaseMaxHP(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);					
				}else if(affectType == AFFECT_TYPE.MAGIC_DEFENCE)
				{
					tmpAttribute = GameCommon.GetBaseMagicDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
				}else if(affectType == AFFECT_TYPE.PHYSICAL_DEFENCE)
				{
					tmpAttribute = GameCommon.GetBasePhysicalDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);					
				}				
				if (tmpData == null)
				{
					tmpFinalATK = tmpAttribute;		
				}else
				{
					tmpFinalATK = AffectFunc.Final(mCurActiveData.teamPos, affectType, tmpAttribute);
				}	
				SaveAttributeValue(_AffectType, (int)tmpFinalATK);
			}
		}
	}
	void SaveAttributeValue(ADD_ATTRITUBE_VALUE_TYPE type, int iValue)
	{		
		if (type >= ADD_ATTRITUBE_VALUE_TYPE.ATTACK && type < ADD_ATTRITUBE_VALUE_TYPE.MAX)
		{
			if(saveTeamAttribute.ContainsKey(type) )
			{
				saveTeamAttribute[type] = iValue;
			}else
			{
				saveTeamAttribute.Add(type, iValue);
			}
		}
	}

	public void SetTipsAttribute(ActiveData mCurActiveData)
	{
		if (mCurActiveData != null && CommonParam.isOpenMoveTips)
		{
			ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
			float tmpAttribute = 0f;
			float tmpFinalATK = 0.0f;				
			
			for(int i = 0; i < 4; i++)
			{					
				AFFECT_TYPE affectType = GetAttributeType((ADD_ATTRITUBE_VALUE_TYPE)(i + 1));	
				ADD_ATTRITUBE_VALUE_TYPE tt =  (ADD_ATTRITUBE_VALUE_TYPE)(i + 1);
				if(affectType == AFFECT_TYPE.ATTACK)
				{
					tmpAttribute = GameCommon.GetBaseAttack(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
				}else if(affectType == AFFECT_TYPE.HP)
				{
					tmpAttribute = GameCommon.GetBaseMaxHP(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
					
				}else if(affectType == AFFECT_TYPE.MAGIC_DEFENCE)
				{
					tmpAttribute = GameCommon.GetBaseMagicDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
				}else if(affectType == AFFECT_TYPE.PHYSICAL_DEFENCE)
				{
					tmpAttribute = GameCommon.GetBasePhysicalDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
				}
				
				if (tmpData == null)
				{
					tmpFinalATK = tmpAttribute;
				}else
				{
					tmpFinalATK = AffectFunc.Final(mCurActiveData.teamPos, affectType, tmpAttribute);
				}
				string strName = TableCommon.GetStringFromEquipAttributeIconConfig((int)affectType, "NAME");			
				foreach(var v in saveTeamAttribute)
				{
					if(v.Key == tt)
					{
						int aa = (int)tmpFinalATK;
						if(aa - v.Value != 0)
						{
							GlobalModule.DoCoroutine(DoCastChangeAttribute (strName, aa - v.Value, tt, aa));
						}
					}
				}

                object _panelTypeValue = DataCenter.Self.getObject("CHANGE_TIP_PANEL_TYPE");
                ChangeTipPanelType _panelType = _panelTypeValue == null ? ChangeTipPanelType.COMMON : (ChangeTipPanelType)_panelTypeValue;

                //队伍属性面板做特殊处理,锁定滚动面板一定时间
                if (ChangeTipPanelType.TEAM_POS_INFO_WINDOW == _panelType)
                {
                    object _teamPosObj = DataCenter.Self.getObject("TEAM_POS_INFO_WINDOW");
                    if (_teamPosObj != null)
                    {
                        tWindow _teamPosInfoWin = _teamPosObj as tWindow;
                        GameObject _teamPosInfoWinObj = _teamPosInfoWin.mGameObjUI;
                        if (_teamPosInfoWinObj != null)
                        {
                            UIScrollView _scroll = GameCommon.FindComponent<UIScrollView>(_teamPosInfoWinObj, "Scroll View");
                            if (_scroll != null)
                            {
                                GlobalModule.DoOnNextUpdate(3, () =>
                                {
                                    if (_scroll != null) 
                                    {
                                        _scroll.enabled = false;
                                    }
                                    GlobalModule.DoLater(() => 
                                    {
                                        if (_scroll != null) 
                                        {
                                            _scroll.enabled = true;
                                        }
                                    }, mScrollViewLockTime);
                                });
                                
                            }
                        }
                    }
                }
			}
		}
	}
	//强化大师Tips
	public void SetTipsMaster(string str, int iLevel)
	{
		if(CommonParam.isOpenMoveTips)
		{
			str = string.Format (str, iLevel);
            //modified by xuke
			//-mPopupTip.Enqueue(str);		
            ChangeTip _tip = new ChangeTip() { Content = str};
            _tip.SetTargetObj(ChangeTipPanelType.TEAM_INFO_WINDOW,ChangeTipValueType.MASTER);
            ChangeTipManager.Self.Enqueue(_tip,(int)ChangeTipPriority.MASTER);
            //end
        }			
	}
    //反馈文字出现时滚动面板锁定时间
    private float mScrollViewLockTime = 2f;
	//属性变化Tips
	private IEnumerator DoCastChangeAttribute(string str, int rel, ADD_ATTRITUBE_VALUE_TYPE tt, int _newValue)
	{
		if(rel > 0)
		{
			str = "[ff9900]" + str + "[-]  [99ff66]+" + rel;
		}else
		{
			str = "[ff9900]" + str + "[-]  [ff3333]" + rel;
		}
//		ChangeValue(str);
        //modified by xuke 
        //mPopupTip.Enqueue(str);
        //yield return new WaitForSeconds(0.1f);
        ChangeTip _tip = new ChangeTip() { Content = str, TargetValue = _newValue };
        object _panelTypeValue = DataCenter.Self.getObject("CHANGE_TIP_PANEL_TYPE");
        ChangeTipPanelType _panelType = _panelTypeValue == null ? ChangeTipPanelType.COMMON : (ChangeTipPanelType)_panelTypeValue;
        
        ////符灵突破满级特殊处理
        //if (ChangeTipPanelType.BREAK_INFO_WINDOW == _panelType) 
        //{
        //    ActiveData _activeData = TeamManager.GetBodyDataByCurTeamPos();
        //    if (_activeData != null && _activeData.breakLevel == GameCommon.GetMaxBreakLevelByTid(_activeData.tid)) 
        //    {
        //        _tip.ShowType = LabelShowType.ONLY_NUMBER;
        //    }
        //}
        ////end
        _tip.SetTargetObj(_panelType, (ChangeTipValueType)tt);
        ChangeTipManager.Self.Enqueue(_tip);
        yield return new WaitForSeconds(0f);
        //end
        SaveAttributeValue(tt, _newValue);
	}
    private int mTmpChangeFitting = int.MinValue;

	//战斗力变化Tips
	public void ChangeFitting()
	{
		if(CommonParam.isOpenMoveTips)
		{

			var power = Convert.ToInt32(GameCommon.GetPower().ToString ("f0"));
            int rel = power - RoleSelTopLeftWindow.TmpChangeFitting;//RoleSelTopLeftWindow.msFightStrengthNum;
			if(RoleSelTopLeftWindow.FightStrengthNum != 0 && rel!= 0)
			{
				string str = "";
				if(rel > 0)
				{
					str = "[ff9900]战斗力" + "  [ff9900]+" + rel;
				}else
				{
					str = "[ff9900]战斗力" + "[-]  [ff3333]" + rel;
				}
	//			ChangeValue(str);
                //modified by xuke
				//--mPopupTip.Enqueue(str);
                ChangeTip _tip = new ChangeTip() { Content = str,TargetValue = power};
                _tip.SetTargetObj(ChangeTipPanelType.COMMON,ChangeTipValueType.POWER);
                ChangeTipManager.Self.Enqueue(_tip,(int)ChangeTipPriority.POWER);
                //end
			}
			//RoleSelTopLeftWindow.msFghtStrengthNum = power;
            RoleSelTopLeftWindow.TmpChangeFitting = power;
		}
	}
	//变化Value显示
//	private void ChangeValue(string str)
//	{
//		GameObject parentwindow = GameCommon.FindUI ("team_info_window");
//		GameObject parentnamelabel = GameCommon.FindObject (mGameObjUI, "Label_effect_name_tips");
//		GameObject namelabel = GameObject.Instantiate (parentnamelabel) as GameObject;
//		namelabel.transform.parent = parentwindow.transform;
//		namelabel.transform.localPosition = new Vector3 (-174, -80, -1000);
//		namelabel .transform .localScale = new Vector3 (1, 1, 1);
//		namelabel.SetActive (true);
//		UILabel petnamelabel = namelabel .GetComponent <UILabel> ();
//		petnamelabel.name = "Label_effect_name_tips(clone)";
//		petnamelabel.text = str;
////		GlobalModule.DoLater (() => GameObject.DestroyImmediate(namelabel), 1.0f);
//	}

    private void RefreshNewMark()
    {
        GameObject _changeBtnObj = GameCommon.FindObject(mGameObjUI, "pet_change_btn");
        GameCommon.SetNewMarkVisible(_changeBtnObj,TeamNewMarkManager.Self.GetPetNewMarkData((int)TeamManager.mCurTeamPos).ChangeVisible);
    }

    public void SetFromMark()
    {
        //set "from" mark
        GameCommon.SetDataByZoneUid("IS_FROM_TEAM_INFO", "1");
    }

    public override void  Open(object param)
    {
        base.Open(param);

        DataCenter.CloseWindow("TEAM_DATA_WINDOW");
        //by chenliang
        //begin

        GlobalModule.DoCoroutine(__OpenAsync(param));
        return;

        //end
    }
    //by chenliang
    //begin

    /// <summary>
    /// 异步打开窗口
    /// </summary>
    /// <returns></returns>
    private IEnumerator __OpenAsync(object param)
    {
        mPopupTip = new PopupTip();
        mPopupTip.parent = mGameObjUI;
        mPopupTip.template = GameCommon.FindObject(mGameObjUI, "Label_effect_name_tips");
        mPopupTip.offset = mPopupTip.template.transform.localPosition;
        mPopupTip.tipRate = 0.3f;
        mPopupTip.tipTime = 1.5f;
        SetFromMark();

        //设置team_icon_info_buttons_grid
        UIGridContainer tmpIconGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_icon_info_buttons_grid");
        if (tmpIconGridContainer != null && tmpIconGridContainer.MaxCount != 4)
        {
            for (int i = 0; i < 4; i++)
            {
                tmpIconGridContainer.MaxCount = i + 1;
                yield return null;
            }
        }

        //设置team_info_grid
        UIGridContainer tmpInfoGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "team_info_grid");
        if (tmpInfoGridContainer != null && tmpInfoGridContainer.MaxCount != 4)
        {
            for (int i = 0; i < 4; i++)
            {
                tmpInfoGridContainer.MaxCount = i + 1;
                yield return null;
            }
        }

        //added by xuke 反馈文字
        if (ChangeTipManager.Self.mGridContainer == null)
        {
            ChangeTipManager.Self.mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "change_tip_grid");
        }
        //end

        AppendDelegate<TeamPosChangeData>(TeamManager.OnChangeBodyTeamPos, UpdateTeamBodyPosInfo);
        AppendDelegate<TeamPosChangeData>(TeamManager.OnChangeEquipTeamPos, UpdateTeamEquipPosInfo);
        AppendDelegate<TeamPosChangeData>(TeamManager.OnChangeMagicTeamPos, UpdateTeamMagicPosInfo);
        AppendDelegate<int, HashSet<int>, HashSet<int>>(Relationship.onRelationChanged, OnRelationChanged);

        if ((bool)param)
        {
            SetAttributeValue(TeamManager.GetActiveDataByTeamPos((int)TEAM_POS.CHARACTER));
            UpdateTeamIconsUI();
            Refresh(TEAM_POS.CHARACTER);
        }
        else
        {
            InitWindow();
        }

        if (mOpenCompleteCallback != null)
        {
            mOpenCompleteCallback();
            mOpenCompleteCallback = null;
        }
    }

    //end

    public override void OnOpen()
    {
        base.OnOpen();
        //add by xuke反馈文字
        ChangeTipManager.Self.ChangeGridPos(GRID_POS.TEAM_INFO);
        //end
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("TEAM_RELATE_INFO_WINDOW");
        DataCenter.CloseWindow("TEAM_RELATE_EFFECT_WINDOW");
        if (ChangeTipManager.Self != null)
        {
            ChangeTipManager.Self.CheckChangeTipShowState(mGameObjUI);
        }
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        //by chenliang
        //begin

        GlobalModule.DoCoroutine(__RefreshAsync(param));
        return true;

        //end

        TeamManager.mCurTeamPos = (TEAM_POS)param;
        SwitchToPetInfoPanel();
        UpdateUI();
        return true;
    }
    //by chenliang
    //begin

    /// <summary>
    /// 异步刷新
    /// </summary>
    /// <returns></returns>
    private IEnumerator __RefreshAsync(object param)
    {
        TeamManager.mCurTeamPos = (TEAM_POS)param;
        yield return null;
        SwitchToPetInfoPanel();
        //added by xuke
        TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
        TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
        //end
        yield return null;
        UpdateUI();
    }

    //end

    private void OnRelationChanged(int teamPos, HashSet<int> added, HashSet<int> removed)
    {
        // 如果是显示全部缘分的标记为true,则显示该符灵本身的所有缘分和其他符灵新激活的缘分
        if (Relationship.mShowAllRel)
        {
            ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(teamPos);
            if (_activeData == null || Relationship.mTeamPosChangeData == null)
                return;
            if (_activeData.tid == Relationship.mTeamPosChangeData.upTid)
            {
                HashSet<int> _activeRelSet = Relationship.GetCachedActiveRelateTidSet(teamPos);
                using (var relSet = _activeRelSet.GetEnumerator())
                {
                    while (relSet.MoveNext())
                    {
                        AddToActiveRelations(teamPos, relSet.Current);
                    }
                }
            }
            else 
            {
                foreach (var a in added)
                {
                    AddToActiveRelations(teamPos, a);
                }      
            }        
        }
        else 
        {
            foreach (var a in added)
            {
                AddToActiveRelations(teamPos, a);
            }        
        }

        if (11 <= teamPos && teamPos <= 16)
        {
            ChangeTipManager.Self.PlayAnim();
        }
    }

    //private List<KeyValuePair<int, int>> activeRelations = new List<KeyValuePair<int,int>>();
    //private IRoutine activeRelationsRoutine = null;

    private void AddToActiveRelations(int teamPos, int rel)
    {
        string petname = null;
        int iBodyTid = TeamManager.GetBodyTidByTeamPos(teamPos);
        petname = GameCommon.GetItemName(iBodyTid);
        var r = DataCenter.mRelateConfig.GetRecord(rel);
        string text = "[ffff00]" + petname + "[-]" + "的缘分" + "[00ff00]" + r["RELATE_NAME"] + "[-]" + "激活了";
        //modified by xuke 反馈文字效果
        //--mPopupTip.Enqueue(text);
        ChangeTip _tip = new ChangeTip() { Content = text};
        ChangeTipManager.Self.Enqueue(_tip,(int)ChangeTipPriority.RELATE);
        //end
        //activeRelations.Add(new KeyValuePair<int, int>(teamPos, rel));

        //if (!Routine.IsActive(activeRelationsRoutine) && mGameObjUI != null)
        //{
        //    activeRelationsRoutine = mGameObjUI.StartRoutine(DoCastActiveRelations());
        //}
        }

    //private IEnumerator DoCastActiveRelations()
    //{
    //    while (activeRelations.Count > 0)
    //    {
    //        var pair = activeRelations[0];
    //        CastActiveRelation(pair.Key, pair.Value);
    //        activeRelations.RemoveAt(0);
    //        yield return new Wait(0.5f);
    //    }
    //}

    //private void CastActiveRelation(int teamPos, int rel)
    //{
    //    // 在此处理单条激活缘分的显示逻辑
    //        string petname = null;
    //        int iBodyTid = TeamManager.GetBodyTidByTeamPos (teamPos);
    //        petname = GameCommon.GetItemName (iBodyTid);
    //        var r = DataCenter.mRelateConfig.GetRecord (rel);
    //        GameObject parentwindow = GameCommon.FindUI ("team_info_window");
    //        GameObject parentnamelabel = GameCommon.FindObject (mGameObjUI, "Label_effect_name_tips");
    //        GameObject namelabel = GameObject.Instantiate (parentnamelabel) as GameObject;
    //        namelabel.transform.parent = parentwindow.transform;
    //        namelabel.transform.localPosition = new Vector3 (-174, -80, -1000);
    //        namelabel .transform .localScale = new Vector3 (1, 1, 1);
    //        namelabel.SetActive (true);
    //        UILabel petnamelabel = namelabel .GetComponent <UILabel> ();
    //        petnamelabel .name = "Label_effect_name_tips_(clone)";
    //        petnamelabel.text = "[ffff00]" + petname + "[-]" + "的缘分" + "[00ff00]" + r ["RELATE_NAME"] + "[-]" + "激活了";
    //}

    public void UpdateUI()
    {
        GameCommon.ToggleTrue(GameCommon.FindObject(mTeamIconGrid.controlList[(int)TeamManager.mCurTeamPos], "team_pos_head_btn"));

        UpdateTeamPosInfoUI((int)TeamManager.mCurTeamPos);
    }

    public void UpdateTeamIconsUI()
    {
        if (mTeamIconGrid != null)
        {
            for (int i = 0; i < mTeamIconGrid.MaxCount; i++)
            {
                TeamPosIcon(i);
            }
        }
        else
        {
            DEBUG.LogError("team_icon_info_buttons_grid is not exist");
        }
    }
    private string GetOpenLevelDescription(int kOpenLevel) 
    {
        return kOpenLevel + "级开放";
    }
    public void TeamPosIcon(int iTeamPos)
    {
        GameObject obj = mTeamIconGrid.controlList[iTeamPos];

        //add alfa
        if (obj != null)
        {
            GameObject objTeamAdd = GameCommon.FindObject(obj, "team_add_pos_btn");
            AddAnimAlfa(GameCommon.FindObject(objTeamAdd, "add_sprite"));
        }

        int iBodyTid = TeamManager.GetBodyTidByTeamPos(iTeamPos);

        if (iBodyTid > 0)
        {
            GameCommon.SetPetIconWithElementAndStar(obj, "team_pos_head_btn", "", "", iBodyTid);
        }

        bool isOpen = TeamManager.IsPosOpen(iTeamPos);
        obj.transform.Find("team_pos_head_btn").gameObject.SetActive(isOpen && iBodyTid > 0);
        obj.transform.Find("team_add_pos_btn").gameObject.SetActive(isOpen && iBodyTid <= 0);
        //modified by xuke
        //obj.transform.Find("team_lock_pos_btn").gameObject.SetActive(!isOpen);
        obj.transform.Find("team_lock_pos_btn").gameObject.SetActive(false);
        GameObject limitLblObj = GameCommon.FindObject(obj, "level_limit_label");
        if (limitLblObj != null) 
        {
            limitLblObj.gameObject.SetActive(!isOpen);
            if (!isOpen) 
            {
                TeamPosData _teamPosData = TeamManager.GetTeamPosData(iTeamPos);
                string _openDescription = GetOpenLevelDescription(_teamPosData.openLevel);
                //绑定按钮事件
                AddButtonAction(limitLblObj, () => 
                {
                    DataCenter.OnlyTipsLabelMessage(_openDescription);
                });
              
                UILabel _limitLbl = limitLblObj.GetComponent<UILabel>();
                if (_limitLbl != null) 
                {
                    _limitLbl.text = _openDescription;
                }
            }        
        }
        //end
        NiceData data = GameCommon.GetButtonData(obj, "team_pos_head_btn");
        if (data != null)
        {
            data.set("TEAM_POS", iTeamPos);
        }

        data = GameCommon.GetButtonData(obj, "team_add_pos_btn");
        if (data != null)
        {
            data.set("TEAM_POS", iTeamPos);
        }
    }

    public void UpdateTeamPosInfoUI(int iTeamPos)
    {
        //by chenliang
        //begin

//         for (int i = 0; i < mTeamInfoGrid.MaxCount; i++)
//         {
//             mTeamInfoGrid.controlList[i].SetActive(TeamManager.mCurTeamPos == (TEAM_POS)i);
//         }
//-------------------
        //判空
        for (int i = 0; i < mTeamInfoGrid.MaxCount; i++)
        {
            if (mTeamInfoGrid.controlList[i] != null)
                mTeamInfoGrid.controlList[i].SetActive(TeamManager.mCurTeamPos == (TEAM_POS)i);
        }

        //end

        UpdateTeamPosBodyInfoUI(iTeamPos);
        UpdateTeamPosEquipInfoUI(iTeamPos);
        UpdateTeamPosMagicInfoUI(iTeamPos);
    }

    void InitCard(int iTeamPos)
    {
        if (mCardList[iTeamPos].transform.childCount == 0)
        {
            GameCommon.InitCard(mCardList[iTeamPos], mfCardScale, mGameObjUI, "_army");
        }
    }

    public void UpdateTeamPosBodyInfoUI(int iTeamPos)
    {
        GameObject obj = mTeamInfoGrid.controlList[iTeamPos];
        ActiveData activeData = TeamManager.GetBodyDataByTeamPos(iTeamPos);
        if (obj != null && activeData != null)
        {
            int iBodyTid = TeamManager.GetBodyTidByTeamPos(iTeamPos);                

            // card
            GameCommon.SetCardInfo(mCardList[iTeamPos].name, iBodyTid, activeData.level, activeData.breakLevel, mMastButtonUI);
            if (mCardList[iTeamPos] != null) 
            {
                ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(iTeamPos);
                if (_activeData != null) 
                {
                    GlobalModule.DoCoroutine(GameCommon.IE_ChangeRenderQueue(_activeData.tid,mCardList[iTeamPos], CommonParam.AureoleRenderQueue));    
                }                         
            }

            // name
            UILabel name = GameCommon.FindComponent<UILabel>(obj, "role_name_laber");
            if (name != null)
            {
                name.text = GameCommon.GetItemName(iBodyTid);
                name.color = GameCommon.GetNameColor(iBodyTid);
            }

            // break level
            UILabel breaklevel = GameCommon.FindComponent<UILabel>(obj, "break_level_number");
            if (breaklevel != null)
            {
                breaklevel.gameObject.SetActive(activeData.breakLevel > 0);
                breaklevel.text = GameCommon.ShowAddNumUI(activeData.breakLevel);
            }
        }

        UpdateTeamPosBtn(iTeamPos);
    }

    public void UpdateTeamPosEquipInfoUI(int iTeamPos)
    {
        GameObject obj = mTeamInfoGrid.controlList[iTeamPos];
        UIGridContainer itemIconGrid = GameCommon.FindComponent<UIGridContainer>(obj, "equip_info_grid");
        if (obj != null && itemIconGrid != null)
        {
            for (int i = (int)EQUIP_TYPE.ARM_EQUIP; i < (int)EQUIP_TYPE.MAX; i++)
            {
                GameObject itemObj = itemIconGrid.controlList[i];

                //add alfa
                if (itemObj != null)
                {
                    AddAnimAlfa(GameCommon.FindObject(itemObj, "add_sprite"));
                }

                EquipData equipData = TeamManager.GetRoleEquipDataByTeamPos(iTeamPos, i);
                if (equipData != null)
                {
                    NiceData equipBtnData = GameCommon.GetButtonData(itemObj, "equip_btn");
                    if (equipBtnData != null)
					{
                        equipBtnData.set("ITEM_DATA", equipData);
					}

                    //icon
                    ItemData itemData = new ItemData();
                    itemData.mType = (int)PackageManager.GetItemTypeByTableID(equipData.tid);
                    itemData.mID = equipData.tid;
                    GameCommon.SetItemIcon(itemObj, itemData);
                    GameObject mlevel = GameCommon.FindObject(itemObj, "level_bg");
                   // mlevel.GetComponent<UISprite>().atlas = (UIAtlas)Resources.Load("ElementAtlas", typeof(UIAtlas)); 
                    mlevel.GetComponent<UISprite>().spriteName = "equip_0" + ((int)equipData.mQualityType- 1).ToString();
                    GameCommon.FindObject(mlevel, "level_label").SetActive(true);
                    GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().text = equipData.strengthenLevel.ToString();
                    GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().fontSize = 16;
                }
                else
                {
                    NiceData addEquipBtnData = GameCommon.GetButtonData(itemObj, "add_equip_btn");
                    if (addEquipBtnData != null)
                        addEquipBtnData.set("SLOT_POS", i);
                }

                GameCommon.FindObject(itemObj, "equip_btn").SetActive(equipData != null);
                GameCommon.FindObject(itemObj, "add_equip_btn").SetActive(equipData == null);
                GameCommon.FindObject(itemObj, "level_bg").SetActive(equipData != null);
            }
        }
    }

    public void UpdateTeamPosMagicInfoUI(int iTeamPos)
    {
        GameObject obj = mTeamInfoGrid.controlList[iTeamPos];
        UIGridContainer itemIconGrid = GameCommon.FindComponent<UIGridContainer>(obj, "magic_info_grid");
        if (obj != null && itemIconGrid != null)
        {
            for (int i = (int)MAGIC_TYPE.MAGIC1; i < (int)MAGIC_TYPE.MAX; i++)
            {
                GameObject itemObj = itemIconGrid.controlList[i];

                //add alfa
                if (itemObj != null)
                {
                    AddAnimAlfa(GameCommon.FindObject(itemObj, "add_sprite"));
                }

                EquipData magicData = TeamManager.GetMagicDataByTeamPos(iTeamPos, i);
                if (magicData != null)
                {
                    NiceData magicBtnData = GameCommon.GetButtonData(itemObj, "magic_btn");
                    if (magicBtnData != null)
                    {
                        magicBtnData.set("ITEM_DATA", magicData);
                    }

                    //icon
                    ItemData itemData = new ItemData();
                    itemData.mType = (int)PackageManager.GetItemTypeByTableID(magicData.tid);
                    itemData.mID = magicData.tid;
                    GameCommon.SetItemIcon(itemObj, itemData);
                    GameObject mlevel = GameCommon.FindObject(itemObj, "level_bg");
                  //   mlevel.GetComponent<UISprite>().atlas = (UIAtlas)Resources.Load("ElementAtlas", typeof(UIAtlas)); 
                    mlevel.GetComponent<UISprite>().spriteName = "equip_0" + ((int)magicData.mQualityType - 1).ToString();
                    GameCommon.FindObject(mlevel, "level_label").SetActive(true);
                    GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().text = magicData.strengthenLevel.ToString();
                    GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().fontSize = 16;
                }
                else
                {
                    NiceData addMagicBtnData = GameCommon.GetButtonData(itemObj, "add_magic_btn");
                    if (addMagicBtnData != null)
                        addMagicBtnData.set("SLOT_POS", i);
                }
                int _openLevel = TeamManager.GetMagicEquipOpenLevelByType(i);
                bool _isOpen = _openLevel <= RoleLogicData.GetMainRole().level;

                GameCommon.FindObject(itemObj, "level_bg").SetActive(magicData != null);
                GameCommon.FindObject(itemObj, "magic_btn").SetActive(magicData != null);
                GameCommon.FindObject(itemObj, "add_magic_btn").SetActive(_isOpen && magicData == null);//GetMagicEquipOpenLevelByType
                GameObject _limitLblObj = GameCommon.FindObject(itemObj, "level_limit_label");
                if (_limitLblObj != null) 
                {
                    _limitLblObj.SetActive(!_isOpen);
                    if (!_isOpen) 
                    {
                        string _openDescription = GetOpenLevelDescription(_openLevel);
                        AddButtonAction(_limitLblObj,()=>
                        {
                            DataCenter.OnlyTipsLabelMessage(_openDescription);
                        });
                        UILabel _limitLbl = _limitLblObj.GetComponent<UILabel>();
                        if (_limitLbl != null)
                        {
                            _limitLbl.text = _openDescription;
                        }
                    }              
                }
            }
        }
    }

    public void UpdateTeamPosBtn(int iTeamPos)
    {
        //GameObject strengthenMasterBtn = GameCommon.FindObject(mGameObjUI, "strengthen_master_btn");
        //GameObject keyToStrengthenBtn = GameCommon.FindObject(mGameObjUI, "key_to_strengthen_btn");
        GameObject roleChangeSkinBtn = GameCommon.FindObject(mGameObjUI, "role_change_skin_btn");
        GameObject petChangeBtn = GameCommon.FindObject(mGameObjUI, "pet_change_btn");
        GameObject petBackBtn = GameCommon.FindObject(mGameObjUI, "pet_back_btn");

        if (roleChangeSkinBtn != null)
            roleChangeSkinBtn.SetActive(CommonParam.isOnLineVersion && iTeamPos == (int)TEAM_POS.CHARACTER);

        if (petChangeBtn != null)
            petChangeBtn.SetActive(iTeamPos != (int)TEAM_POS.CHARACTER);

        if (petBackBtn != null)
            petBackBtn.SetActive(false);
    }

    public void UpdateTeamBodyPosInfo(TeamPosChangeData teamPosChangeData)
    {
        if (null == teamPosChangeData)
        {
            return;
        }
        int iTeamPos = teamPosChangeData.teamPos;
        if (iTeamPos < (int)TEAM_POS.MAX)
        {
            DataCenter.CloseWindow("TEAM_DATA_WINDOW");

            TeamPosIcon(iTeamPos);

			ActiveData _mCurActiveData = TeamManager.GetActiveDataByTeamPos (iTeamPos);
//			GlobalModule.DoCoroutine( SetTipsAttribute(_mCurActiveData));
            
            DataCenter.Set("CHANGE_TIP_PANEL_TYPE",ChangeTipPanelType.TEAM_POS_INFO_WINDOW);
			SetTipsAttribute(_mCurActiveData);
			ChangeFitting();
            ChangeTipManager.Self.PlayAnim();

            Refresh((TEAM_POS)iTeamPos);
            //added by xuke 刷新红点逻辑
            TeamNewMarkManager.Self.CheckChangePet((int)TeamManager.mCurTeamPos);
            TeamNewMarkManager.Self.RefreshTeamNewMark();
            TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
            TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
            //end
        }
    }

    public void UpdateTeamEquipPosInfo(TeamPosChangeData teamPosChangeData)
    {
        if (null == teamPosChangeData)
        {
            return;
        }

        if (teamPosChangeData.teamPos < (int)TEAM_POS.MAX)
        {
            UpdateTeamPosEquipInfoUI(teamPosChangeData.teamPos);
            //added by xuke
            TeamNewMarkManager.Self.CheckEquip();
            TeamNewMarkManager.Self.RefreshTeamNewMark();
            //end
            if (-1 == teamPosChangeData.upItemId)
            {
                //added by xuke 属性变化反馈效果相关
                DataCenter.Set("CHANGE_TIP_PANEL_TYPE", ChangeTipPanelType.TEAM_POS_INFO_WINDOW);
                CommonParam.mIsRefreshPetInfo = false;
                //end
                DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
            }
            else
            {
                DataCenter.SetData("TEAM_POS_INFO_WINDOW", "UPFATE_BASE_ATTRIBUTE_INFO", null);
                DataCenter.OpenWindow("TEAM_DATA_WINDOW", RoleEquipLogicData.Self.GetEquipDataByItemId(teamPosChangeData.upItemId));
            }
			ActiveData _mCurActiveData = TeamManager.GetActiveDataByTeamPos (teamPosChangeData.teamPos);
//			GlobalModule.DoCoroutine( SetTipsAttribute(_mCurActiveData));
			
            SetTipsAttribute(_mCurActiveData);
			ChangeFitting();

            ChangeTipManager.Self.PlayAnim();
            
            
        }
    }

    public void UpdateTeamMagicPosInfo(TeamPosChangeData teamPosChangeData)
    {
        if (null == teamPosChangeData)
        {
            return;
        }

        if (teamPosChangeData.teamPos < (int)TEAM_POS.MAX)
        {
            UpdateTeamPosMagicInfoUI(teamPosChangeData.teamPos);
            //added by xuke
            TeamNewMarkManager.Self.CheckMagicEquip();
            TeamNewMarkManager.Self.RefreshTeamNewMark();
            //end
            if (-1 == teamPosChangeData.upItemId)
            {
                //added by xuke 属性变化反馈效果相关
                DataCenter.Set("CHANGE_TIP_PANEL_TYPE", ChangeTipPanelType.TEAM_POS_INFO_WINDOW);
                CommonParam.mIsRefreshPetInfo = false;
                //end
                DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
            }
            else
            {
                DataCenter.OpenWindow("TEAM_DATA_WINDOW", MagicLogicData.Self.GetEquipDataByItemId(teamPosChangeData.upItemId));
            }            
			ActiveData _mCurActiveData = TeamManager.GetActiveDataByTeamPos (teamPosChangeData.teamPos);
//			GlobalModule.DoCoroutine( SetTipsAttribute(_mCurActiveData));
			SetTipsAttribute(_mCurActiveData);
			ChangeFitting();
            ChangeTipManager.Self.PlayAnim();
        }
    }

    private void SwitchToPetInfoPanel()
    {
        SetVisible("team_info_grid", true);
        SetVisible("team_buttons_group", true);
        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
        DataCenter.CloseWindow("TEAM_RELATE_INFO_WINDOW");       
    }

    private void SwitchToRelateInfoPanel()
    {
        SetVisible("team_info_grid", false);
        SetVisible("team_buttons_group", false);
        DataCenter.CloseWindow("TEAM_POS_INFO_WINDOW");
        DataCenter.OpenWindow("TEAM_RELATE_INFO_WINDOW");
    }

    static public void CloseAllWindow()
    {
        DataCenter.CloseWindow("TEAM_POS_INFO_WINDOW");
        DataCenter.CloseWindow("TEAM_RELATE_EFFECT_WINDOW");
        DataCenter.CloseWindow("TEAM_RIGHT_ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("TEAM_DATA_WINDOW");
        DataCenter.CloseWindow("EQUIP_INFO_WINDOW");
        DataCenter.CloseWindow("MAGIC_INFO_WINDOW");
        DataCenter.CloseWindow("EQUIP_REFINE_INFO_WINDOW");
        DataCenter.CloseWindow("MAGIC_REFINE_INFO_WINDOW");
        DataCenter.CloseWindow("PET_FRAGMENT_INFO_WINDOW");
        DataCenter.CloseWindow("EQUIP_FRAGMENT_INFO_WINDOW");
        DataCenter.CloseWindow(UIWindowString.master_container);
    }
}

public class TeamPosHeadBtn : CEvent
{
    public override bool _DoEvent()
    {
        TeamManager.mAddPetPos = -1;

        PetLevelUpInfoWidnow.eatExp = 0;
        int teamPos = get("TEAM_POS");
        if (teamPos == (int)TeamManager.mCurTeamPos)
        {
            tWindow window = DataCenter.GetData("TEAM_RELATE_INFO_WINDOW") as tWindow;
            if (window != null)
            {
                if (!window.IsOpen())
                {
                    tWindow teamPosWindow = DataCenter.GetData("TEAM_POS_INFO_WINDOW") as tWindow;
                    if (teamPosWindow != null && !teamPosWindow.IsOpen())
                    {
                        TeamInfoWindow.CloseAllWindow();
                        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos(teamPos));
                        //added by xuke
                        TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
                        TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
                        //end
                    }
                    return false;
                }
            }
        }

        DataCenter.SetData("TEAM_INFO_WINDOW", "REFRESH", (TEAM_POS)teamPos);
		DataCenter.SetData ("TEAM_INFO_WINDOW", "SET_SAVE_ATTRIBUTE_VALUE",  TeamManager.GetActiveDataByTeamPos(teamPos));

        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos(teamPos));

        //added by xuke
        TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
        TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
        //end
        return true;
    }
}

public class TeamPosAddBtn : CEvent
{
    public override bool _DoEvent()
    {
        int teamPos = get("TEAM_POS");
        TeamManager.mAddPetPos = teamPos;

        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = (itemData) =>
        {
            return !TeamManager.IsPetInTeamByTid(itemData.tid);  /*指定tid的宠物，只要有一个在队伍中，便都不能显示*/
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER_RELATION", true);
			tempList.Sort(new SortPetDataByRelation<PetData>(teamPos));
            return tempList;
        };

        openObj.mSelectAction = (upItemData) =>
        {
            if (null == upItemData)
                return;

            // send message to server
            int curTeamPos = (int)get("TEAM_POS");
            PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;
            if (logic != null)
            {
                PetData downPetData = TeamManager.GetPetDataByTeamPos(curTeamPos);
	
				for(int i = (int) ADD_ATTRITUBE_VALUE_TYPE.ATTACK; i < (int) ADD_ATTRITUBE_VALUE_TYPE.MAX; i++)
				{
					ADD_ATTRITUBE_VALUE_TYPE tt =  (ADD_ATTRITUBE_VALUE_TYPE)(i);
					if(TeamInfoWindow.saveTeamAttribute.ContainsKey(tt) )
					{
						TeamInfoWindow.saveTeamAttribute[tt] = 0;
					}else
					{
						TeamInfoWindow.saveTeamAttribute.Add(tt, 0);
					}
				}
                //added by xuke 反馈文字相关
                TeamPosInfoWindow.mShowZeroInfo = true;
                //end
                TeamManager.RequestChangeTeamPos((int)curTeamPos, upItemData.itemId, upItemData.tid,
                    downPetData != null ? downPetData.itemId : -1, downPetData != null ? downPetData.tid : -1);
            }
        };
        DataCenter.Set("I_AM_FROM_TEAMINO", false);
        bool teamInfo = DataCenter.Get("I_AM_FROM_TEAMINO");
        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
        return true;
    }
}

public class TeamLockPosBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_TEAM_POS_LOCKED);
        return true;
    }
}

public class TeamSlotEquipInfoBtn : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;
        TeamInfoWindow.CloseAllWindow();
		tWindow equip_info_window = DataCenter.GetData("TEAM_DATA_WINDOW") as tWindow;
		if (equip_info_window == null || equip_info_window.mGameObjUI == null) {
			DataCenter.OpenWindow ("TEAM_DATA_WINDOW", itemData);
			return true;
		}
		tWindow tWin = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
		if (tWin != null) {
			equip_info_window.mGameObjUI.transform.parent = GameObject.Find ("CenterAnchor").gameObject.transform;
			equip_info_window.mGameObjUI.transform.localPosition = Vector3.zero;
		}
        DataCenter.OpenWindow("TEAM_DATA_WINDOW", itemData);

        return true;
    }
}

public class TeamSlotEquipAddBtn : CEvent
{
    public override bool _DoEvent()
    {
        int iType = get("SLOT_POS");
		int equip_num = RoleEquipLogicData.Self.GetEquipDataNumByType(iType) - TeamManager.GetEqupNumInTeamByType(iType);
        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = (itemData) =>
        {
            return !itemData.IsInTeam() && iType == PackageManager.GetSlotPosByTid(itemData.tid);
        };

        openObj.mSelectAction = (upItemData) =>
        {
            EquipData downEquipData = TeamManager.GetRoleEquipDataByCurTeamPos(PackageManager.GetSlotPosByTid(upItemData.tid));

            TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, upItemData.itemId, upItemData.tid,
                downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
        };

		if (equip_num > 0) {
			DataCenter.SetData ("BAG_EQUIP_WINDOW", "OPEN", openObj);

            tWindow equip_info_window = DataCenter.GetData("TEAM_DATA_WINDOW") as tWindow;
            if (equip_info_window == null || equip_info_window.mGameObjUI == null)
            {
                return true;
            }
            tWindow tWin = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
            if (tWin != null)
            {
                equip_info_window.mGameObjUI.transform.parent = GameObject.Find("CenterAnchor").gameObject.transform;
                equip_info_window.mGameObjUI.transform.localPosition = Vector3.zero;
            }
		} else {
			DataRecord r =DataCenter.mEquipPostion.GetRecord(iType + 1);
			int tid = r["ITEM_ID"];
			DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW",tid);
			DataCenter.SetData("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW","RELATE_TO_GET_PATH",tid);
		}
        return true;
    }
}

public class TeamSlotMagicInfoBtn : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;
        TeamInfoWindow.CloseAllWindow();
		tWindow equip_info_window = DataCenter.GetData("TEAM_DATA_WINDOW") as tWindow;
		if (equip_info_window == null || equip_info_window.mGameObjUI == null) {
			DataCenter.OpenWindow ("TEAM_DATA_WINDOW", itemData);
			return true;
		}
		tWindow tWin = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
		if (tWin != null) {
			equip_info_window.mGameObjUI.transform.parent = GameObject.Find ("CenterAnchor").gameObject.transform;
			equip_info_window.mGameObjUI.transform.localPosition = Vector3.zero;
		}
        DataCenter.OpenWindow("TEAM_DATA_WINDOW", itemData);
        return true;
    }
}

public class TeamSlotMagicAddBtn : CEvent
{
    public override bool _DoEvent()
    {
        int iType = get("SLOT_POS");
		int magic_num = MagicLogicData.Self.GetEquipDataNumByType(iType) - TeamManager.GetMagicNumInTeamByType(iType);
        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = (itemData) =>
        {
            return !itemData.IsInTeam() && iType == PackageManager.GetSlotPosByTid(itemData.tid);
        };

        openObj.mSelectAction = (upItemData) =>
        {
            EquipData downEquipData = TeamManager.GetMagicDataByCurTeamPos(PackageManager.GetSlotPosByTid(upItemData.tid));

            TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, upItemData.itemId, upItemData.tid,
                downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
        };

		if (magic_num > 0) {
			DataCenter.SetData ("BAG_MAGIC_WINDOW", "OPEN", openObj);

            tWindow equip_info_window = DataCenter.GetData("TEAM_DATA_WINDOW") as tWindow;
            if (equip_info_window == null || equip_info_window.mGameObjUI == null)
            {
                return true;
            }
            tWindow tWin = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
            if (tWin != null)
            {
                equip_info_window.mGameObjUI.transform.parent = GameObject.Find("CenterAnchor").gameObject.transform;
                equip_info_window.mGameObjUI.transform.localPosition = Vector3.zero;
            }
		}else {
			DataRecord r =DataCenter.mEquipPostion.GetRecord(iType + 5);
			int tid = r["ITEM_ID"];
			DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW",tid);
			DataCenter.SetData("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW","RELATE_TO_GET_PATH",tid);
		}

        return true;
    }
}

public class Button_StrengthenMasterBtn : CEvent
{
    public override bool _DoEvent()
    {
        TeamInfoWindow.CloseAllWindow();
        //added by xuke begin
        // 强化大师需要穿齐四件装备的时候才能开启
        if (!TeamManager.IsEquipFull(TeamManager.mCurTeamPos))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_STRENGTHEN_MASTER_NEED_4_EQUIP);
            //DataCenter.OpenMessageWindow("穿齐4件装备即可开启强化大师");
            return true;
        }
        //end
        DataCenter.OpenWindow(UIWindowString.master_container);
        return true;
    }
}

public class Button_KeyToStrengthenBtn : CEvent
{
    public override bool _DoEvent()
    {
        return true;
    }
}

public class Button_RoleChangeSkinBtn : CEvent
{
    public override bool _DoEvent()
    {

        //int iType = get("SLOT_POS");
        //DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", iType);

        return true;
    }
}

public class Button_PetChangeBtn : CEvent
{
    public override bool _DoEvent()
    {
        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = (itemData) =>
        {
			//DataCenter.CloseWindow("TEAM_RIGHT_ROLE_INFO_WINDOW");
            ActiveData activeData = TeamManager.GetActiveDataByCurTeamPos();
            return !TeamManager.IsPetInTeamByTid(itemData.tid)  /*指定tid的宠物，只要有一个在队伍中，便都不能显示*/
                || (itemData.tid == activeData.tid && itemData.itemId != activeData.itemId);/*当前阵位的宠物对应的tid的宠物，是可以显示的，除了自身*/
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER_RELATION", true);
            int teamPos = (int)TeamManager.mCurTeamPos;
			tempList.Sort(new SortPetDataByRelation<PetData>(teamPos));
            return tempList;
        };
        
        openObj.mSelectAction = (upItemData) =>
        {
            if (null == upItemData)
                return;
            //added by xuke 
            CommonParam.mIsRefreshPetInfo = false;
            //end
            // send message to server
            int curTeamPos = (int)TeamManager.mCurTeamPos;
            PetData downPetData = TeamManager.GetPetDataByTeamPos(curTeamPos);

            TeamManager.RequestChangeTeamPos((int)curTeamPos, upItemData.itemId, upItemData.tid,
                downPetData != null ? downPetData.itemId : -1, downPetData != null ? downPetData.tid : -1);
        };

        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
        return true;
    }
}

public class Button_PetBackBtn : CEvent
{
    public override bool _DoEvent()
    {

        int iType = get("SLOT_POS");
        DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", iType);

        return true;
    }
}

public class Button_karma_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("TEAM_INFO_WINDOW", "RELATE_INFO", null);
        return true;
    }
}

public class Button_look_relate_btn : CEvent
{
	public override bool _DoEvent()
	{
		tWindow teamPosWindow = DataCenter.GetData("TEAM_POS_INFO_WINDOW") as tWindow;
		if (teamPosWindow != null && !teamPosWindow.IsOpen())
		{
			TeamInfoWindow.CloseAllWindow();
			DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
			TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
			TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
		}	
		DataCenter.SetData ("TEAM_POS_INFO_WINDOW", "SET_RELATE_POSITION", true);
		DataCenter.SetData ("TEAM_INFO_WINDOW", "SET_RELATE_INFOS", true);		
		return true;
	}
}
public class Button_look_base_attribute_btn : CEvent
{
	public override bool _DoEvent()
	{
		tWindow teamPosWindow = DataCenter.GetData("TEAM_POS_INFO_WINDOW") as tWindow;
		if (teamPosWindow != null && !teamPosWindow.IsOpen())
		{
			TeamInfoWindow.CloseAllWindow();
			DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos));
			TeamNewMarkManager.Self.RefreshTeamInfoNewMark();
			TeamNewMarkManager.Self.RefreshTeamPosInfoNewMark();
		}
		DataCenter.SetData ("TEAM_POS_INFO_WINDOW", "SET_BASE_POSITION", true);
		DataCenter.SetData ("TEAM_INFO_WINDOW", "SET_BASE_INFOS", true);
		return true;
	}
}