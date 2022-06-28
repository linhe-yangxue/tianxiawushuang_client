using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

/// <summary>
/// 法器/装备详情界面
/// </summary>
public class GrabTreasureMagicEquipDetailUI : tWindow
{
    private GameObject mInfoBoard = null;
    private GameObject mPathBoard = null;
    private UILabel mTitleLbl;

    private int mEquipTid = 0;
    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_grab_magic_equip_detail_close_button", new DefineFactoryLog<Button_grab_magic_equip_detail_close_button>());
        EventCenter.Self.RegisterEvent("Button_magic_details_window_bg", new DefineFactoryLog<Button_grab_magic_equip_detail_close_button>());
    
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        mInfoBoard = GetCurUIGameObject("magic_info_board");
        mPathBoard = GetCurUIGameObject("pathBoard");
        mTitleLbl = GetCurUIComponent<UILabel>("title_label");

        AddButtonAction("infoBtn",() => RefreshInfo());
        AddButtonAction("pathBtn",() => RefreshPathInfo());
    }

    public override void Open(object param)
    {
       base.Open(param);
       if (param != null && param is int) 
       {
           mEquipTid = (int)param;
           RefreshTitle();
           RefreshLeftPanelInfo();
           RefreshInfo();
       }
    }

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex) 
		{
		case "BASE_INFO":
			break;
		case "RELATE_TO_GET_PATH":
			GetUIToggle("pathBtn").value = true;
			RefreshPathInfo();
			break;
		case "BASIC_INFO":
			int _tid = (int)objVal;
			GetUIToggle("infoBtn").value = true;
			RefreshInfo();
			break;
		}
	}
    private void RefreshTitle()
    {
        mTitleLbl.text = PackageManager.GetItemTypeByTableID(mEquipTid) == ITEM_TYPE.EQUIP ? "装备信息" : "神器信息";
    }

    private void RefreshLeftPanelInfo() 
    {
        string _colorPrefix = GameCommon.GetEquipTypeColor(TableCommon.GetNumberFromRoleEquipConfig(mEquipTid, "QUALITY"));
        GetUILabel("magic_equip_name").text = _colorPrefix + TableCommon.GetStringFromRoleEquipConfig(mEquipTid, "NAME")+"[-]";
        GetUILabel("type_name").text = _colorPrefix + GameCommon.GetEquipTypeDesc(TableCommon.GetNumberFromRoleEquipConfig(mEquipTid, "EQUIP_TYPE")) + "[-]";
        GetUILabel("point").text = _colorPrefix + "·" + "[-]";

        GameCommon.SetOnlyItemIcon(GetCurUIGameObject("magic_icon"), mEquipTid);

        DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(mEquipTid);
        GameObject _strengthRoot = GameCommon.FindObject(mGameObjUI, "strengthen_level_num");
        GameCommon.SetUIText(_strengthRoot, "num_label", "1");
        GameCommon.SetUIText(_strengthRoot, "num_label01", r["MAX_GROW_LEVEL"]);
        GameObject _typeRoot = GameCommon.FindObject(mGameObjUI, "potential_num");
        GameCommon.SetUIText(_typeRoot, "num_label", GameCommon.GetEquipTypeName(r["EQUIP_TYPE"]));
    }

    private void RefreshInfo()
    {
        GetUIToggle("infoBtn").value = true;
        mInfoBoard.SetActive(true);
        mPathBoard.SetActive(false);

        UpdateBaseAttributeInfo();
        UpdateRefineInfo();
        EquipSetInfos();
        UpdateDescribe();
        //UpdateLegendSkillInfo();
        SetPanelPos();
    }

    struct SequenceData 
    {
        public Transform mTrans;
        public int mCellHeight;
    }
    private void SetPanelPos() 
    {
        List<SequenceData> _sequenceList = new List<SequenceData>();
        GameObject _baseRoot = GetCurUIGameObject("base_root");
        GameObject _refineRoot = GetCurUIGameObject("refine_root");
        GameObject _setRoot = GetSub("equip_set_root");
        GameObject _descRoot = GetSub("desc_root");
        GameObject _legendRoot = GetSub("legend_root");

        UISprite _baseSprite = GameCommon.FindComponent<UISprite>(_baseRoot, "base_attribute_bg");
        UISprite _refineSprite = GameCommon.FindComponent<UISprite>(_refineRoot, "bg");
        UISprite _equipSetSprite = GameCommon.FindComponent<UISprite>(_setRoot, "equip_set_icon_group");
        UISprite _descSprite = GameCommon.FindComponent<UISprite>(_descRoot, "bg");
        UISprite _legendSprite = GameCommon.FindComponent<UISprite>(_legendRoot, "bg");
        if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID(mEquipTid))
        {
            _sequenceList.Add(new SequenceData() { mTrans = _baseRoot.transform, mCellHeight = _baseSprite.height });
            _sequenceList.Add(new SequenceData() { mTrans = _refineRoot.transform, mCellHeight = _refineSprite.height });
            _sequenceList.Add(new SequenceData() { mTrans = _descRoot.transform, mCellHeight = _descSprite.height });
            _sequenceList.Add(new SequenceData() { mTrans = _legendRoot.transform, mCellHeight = _legendSprite.height });
           // _refineRoot.transform.localPosition = new Vector3(0, _baseRoot.transform.localPosition.y - _baseSprite.height - _heightStep, 0);
           // _descRoot.transform.localPosition = new Vector3(0,_refineRoot.transform.lo,0);
        }
        else
        {
            _sequenceList.Add(new SequenceData() { mTrans = _baseRoot.transform, mCellHeight = _baseSprite.height });
            _sequenceList.Add(new SequenceData() { mTrans = _refineRoot.transform, mCellHeight = _refineSprite.height });
            _sequenceList.Add(new SequenceData() { mTrans = _setRoot.transform,mCellHeight = _equipSetSprite.height});
            _sequenceList.Add(new SequenceData() { mTrans = _descRoot.transform, mCellHeight = _descSprite.height });
            //_refineRoot.transform.localPosition = new Vector3(0, _baseRoot.transform.localPosition.y - _baseSprite.height - _heightStep, 0);
            //_setRoot.transform.localPosition = new Vector3(0, _refineRoot.transform.localPosition.y - _refineSprite.height - _heightStep, 0);
        }
        int _heigtStep = 15; //> 高度间距调节参数
        for (int i = 0, count = _sequenceList.Count; i < count; i++) 
        {
            if (i + 1 >= count)
                return;
            _sequenceList[i + 1].mTrans.localPosition = new Vector3(0, _sequenceList[i].mTrans.localPosition.y - _sequenceList[i].mCellHeight - _heigtStep, 0);
        }
    }

    //刷新法器基础信息
    private void UpdateBaseAttributeInfo() 
    {
        GameObject _baseGroup = GetCurUIGameObject("base_attribute_group");
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(_baseGroup, "Grid");
        
        DataRecord _record = DataCenter.mRoleEquipConfig.GetRecord(mEquipTid);
        GameCommon.SetUIText(_baseGroup, "num_label", "1");
        GameCommon.SetUIText(_baseGroup, "num_label01", _record["MAX_GROW_LEVEL"]);       
        // 如果是法器
        if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID(mEquipTid))
        {
            _gridContainer.MaxCount = 2;
            GameCommon.SetBaseAttrInfo(_gridContainer.controlList[0], _record["ATTRIBUTE_TYPE_0"], _record["BASE_ATTRIBUTE_0"]);
            GameCommon.SetBaseAttrInfo(_gridContainer.controlList[1], _record["ATTRIBUTE_TYPE_1"], _record["BASE_ATTRIBUTE_1"]);
        }
        // 如果是装备
        else
        {
            _gridContainer.MaxCount = 1;
            GameCommon.SetBaseAttrInfo(_gridContainer.controlList[0], _record["ATTRIBUTE_TYPE_0"], _record["BASE_ATTRIBUTE_0"]);
        }
        // 得到基础属性group的背景图片
        UISprite _baseAttrSprite = GameCommon.FindComponent<UISprite>(_baseGroup, "base_attribute_bg");
        _baseAttrSprite.height = (int)(_gridContainer.CellHeight * (_gridContainer.MaxCount + 1) + 25);
    }

    //刷新精炼信息
    private void UpdateRefineInfo() 
    {
        GameObject _refineGroup = GetCurUIGameObject("refine_attribute_group");
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(_refineGroup, "Grid");
        _gridContainer.MaxCount = 2;
        DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(mEquipTid);
        GameCommon.SetUIText(_refineGroup, "num_label", "0");
        GameCommon.SetUIText(_refineGroup, "num_label01", r["MAX_REFINE_LEVEL"]);
        GameCommon.SetRefineAttribute(_gridContainer.controlList[0], r["REFINE_TYPE_0"], r["REFINE_VALUE_0"]*0);
        GameCommon.SetRefineAttribute(_gridContainer.controlList[1], r["REFINE_TYPE_1"], r["REFINE_VALUE_1"]*0);
    }

    private void UpdateDescribe()
    {
        DataRecord _record = DataCenter.mRoleEquipConfig.GetRecord(mEquipTid);
        GameObject _descGroup = GetCurUIGameObject("equip_tips_group");
        GameCommon.SetUIText(_descGroup, "equip_tips_label", _record["DESCRIPTION"]);
    }

    public void EquipSetInfos()
    {
        GameObject setGroup = GetSub("equip_set_group");
        if (null == setGroup)
            return;

        if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID(mEquipTid))
        {
            setGroup.SetActive(false);
            return;
        }
        setGroup.SetActive(true);
        GameObject equipSetGroupObj = GameCommon.FindObject(mGameObjUI, "equip_set_group").gameObject;
        UILabel equipSetName = equipSetGroupObj.transform.Find("equip_set_icon_group/equip_set_name").GetComponent<UILabel>();

        int equipSetNameIndex = 0;
        foreach (KeyValuePair<int, DataRecord> v in DataCenter.mSetEquipConfig.GetAllRecord())
        {
            int _index = TableCommon.GetNumberFromRoleEquipConfig(mEquipTid, "EQUIP_TYPE") - 1;
            if (v.Value.get("SET_EQUIP_" + _index.ToString()) == mEquipTid)
            {
                equipSetNameIndex = v.Key;
            }
        }
        equipSetName.text = TableCommon.GetStringFromSetEquipConfig(equipSetNameIndex, "SET_NAME");

        UIGridContainer equipSetIconGrid = equipSetGroupObj.transform.Find("equip_set_icon_group/Grid").GetComponent<UIGridContainer>();
        equipSetIconGrid.MaxCount = (int)EQUIP_TYPE.MAX;
        UIGridContainer equipSetTipsGrid = equipSetGroupObj.transform.Find("equip_set_tips_group/Grid").GetComponent<UIGridContainer>();
        equipSetTipsGrid.MaxCount = (int)EQUIP_TYPE.MAX - 1;
        //套装图标
        for (int i = 0; i < equipSetIconGrid.MaxCount; i++)
        {
            GameObject obj = equipSetIconGrid.controlList[i];
            UISprite sprite = obj.transform.Find("icon_sprite").GetComponent<UISprite>();
            int equipItemId = TableCommon.GetNumberFromSetEquipConfig(equipSetNameIndex, "SET_EQUIP_" + i.ToString());

            string strAtlasName = TableCommon.GetStringFromRoleEquipConfig(equipItemId, "ICON_ATLAS_NAME");
            string strSpriteName = TableCommon.GetStringFromRoleEquipConfig(equipItemId, "ICON_SPRITE_NAME");
            GameCommon.SetIcon(sprite, strAtlasName, strSpriteName);

            //checkmark
            obj.transform.Find("Checkmark").gameObject.SetActive(false);
            if (mEquipTid == equipItemId)
            {
                obj.transform.Find("Checkmark").gameObject.SetActive(true);
            }
        }
        //套装加成介绍
        for (int i = 0; i < equipSetTipsGrid.MaxCount; i++)
        {
            GameObject obj = equipSetTipsGrid.controlList[i];
            UILabel equip_set_introduce_label = obj.transform.Find("equip_set_introduce_label").GetComponent<UILabel>();
            string equipBuffs = "";

            for (int j = 0; j < 3; j++)
            {
                int equipSetBuffId = TableCommon.GetNumberFromSetEquipConfig(equipSetNameIndex, "SET" + (i + 2).ToString() + "_BUFF_ID_" + (j + 1).ToString());
                if (null == equipSetBuffId)
                    return;
                string equipSetTipsLabel = TableCommon.GetStringFromAffectBuffer(equipSetBuffId, "TIP_TITLE");

                equipBuffs = equipBuffs + equipSetTipsLabel + "     ";
            }
            string strEquipTips = "[" + (i + 2).ToString() + "件套装]  " + equipBuffs;
            GameCommon.SetUIText(obj, "equip_set_introduce_label", strEquipTips);
        }
    }

    //刷新传奇技能信息
    private void UpdateLegendSkillInfo()
    {
        GameObject _legendGroup = GetCurUIGameObject("legend_skill_group");
        UIGridContainer _gridContainder = GameCommon.FindComponent<UIGridContainer>(_legendGroup,"Grid");
        //判断是否需要显示,只有金色品质的法器才会有传奇技能
        int _quality = TableCommon.GetNumberFromRoleEquipConfig(mEquipTid,"QUALITY");
        if (EQUIP_QUALITY_TYPE.BEST != (EQUIP_QUALITY_TYPE)_quality) 
        {
            _legendGroup.SetActive(false);
            return;
        }

        _legendGroup.SetActive(true);
        //tmp,暂时设定为两条
        _gridContainder.MaxCount = 2;

    }

    //设置每条传奇技能的信息
    private void SetLegendSkillInfo() 
    {
        
    }

    private void RefreshPathInfo() 
    {
        mInfoBoard.SetActive(false);
        mPathBoard.SetActive(true);

        UpdateGetWayUI();
    }

    private void UpdateGetWayUI() 
    {
        GameObject _pathGroup = GetCurUIGameObject("fragment_path_board");
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(_pathGroup,"Grid");
        GameCommon.GetParth(_gridContainer,mEquipTid);
    }
}

class Button_grab_magic_equip_detail_close_button : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW");
        return true;
    }
}