using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System.Linq;

class PetDetail : tWindow {
    PetData curPet;
    //int relateCount;
    GameObject infoBoard;
    GameObject pathBoard;
    GameObject _mSKILL;
    GameObject _mRELATE;
    GameObject _mTALENT;
	GameObject _mINTRODUCE;
    private GameObject mRelateBoard;

    // card
    private float mfCardScale = 1.0f;
    private GameObject mMastButtonUI;
    private GameObject mCard;
    private UIGrid mTabGrid;
    private UIGridContainer mRelateGridContainer;
    private UIScrollView mRelateScrollView;
    struct SkillInfo {
        public readonly int id;
        public readonly string skillName;
        public readonly string atlasName;
        public readonly string spriteName;
        public readonly string describe;

        public SkillInfo(int tid,int skillNum) {
            id=TableCommon.GetNumberFromActiveCongfig(tid,"PET_SKILL_"+skillNum);
            if (id == 0)
                return;
            DataRecord dataRecord = GameCommon.GetSkillDataRecord(id);
            //if(skillNum==1) dataRecord=DataCenter.mSkillConfigTable.GetRecord(id);
            //else dataRecord = DataCenter.mAffectBuffer.GetRecord(id);
            //else dataRecord=(id<300001)?DataCenter.mAttackState.GetRecord(id):DataCenter.mAffectBuffer.GetRecord(id);
            skillName=dataRecord.getData("NAME");
            atlasName = dataRecord.getData("SKILL_ATLAS_NAME");
            spriteName=dataRecord.getData("SKILL_SPRITE_NAME");
            describe = GameCommon.InitSkillDescription(1, id);
        }
    }

    private void SetGridTabState(string kTabName,bool kShowState) 
    {
        Transform _tabObj = mTabGrid.transform.FindChild(kTabName);
        if (_tabObj != null)
            _tabObj.gameObject.SetActive(kShowState);
        mTabGrid.Reposition();
    }

    protected override void OpenInit() {
        base.OpenInit();
        infoBoard= GetCurUIGameObject("infoBoard");
        _mSKILL = GameCommon.FindObject(infoBoard, "Skill");
        _mRELATE = GameCommon.FindObject(infoBoard, "relateGroup");
        _mTALENT = GameCommon.FindObject(infoBoard, "talentGroup");
		_mINTRODUCE = GameCommon.FindObject (infoBoard, "roleIntroduceGroup");
        pathBoard = GetCurUIGameObject("pathBoard");
        mRelateBoard = GetCurUIGameObject("relateBoard");
        mTabGrid = GetCurUIComponent<UIGrid>("Grid");
        mRelateGridContainer = GetCurUIComponent<UIGridContainer>("relate_grid");
        mRelateScrollView = GetCurUIComponent<UIScrollView>("relateBoard");

        AddButtonAction("pet_details_window_bg", () => DataCenter.CloseWindow(UIWindowString.petDetail));
        AddButtonAction("close",() => DataCenter.CloseWindow(UIWindowString.petDetail));
        AddButtonAction("infoBtn",() => RefreshInfo());
        AddButtonAction("pathBtn",() => RefreshPath());
        AddButtonAction("relateBtn",() => RefreshRelate());
        AddButtonAction("relateSprite", () => { RefreshRelate(); GetUIToggle("relateBtn").value = true; });
        // card
        mCard = GetSub("pet_details_card_group_window");
        mMastButtonUI = GetSub("window_black");
        if (mCard != null && mCard.transform.childCount == 0)
            GameCommon.InitCard(mCard, mfCardScale, mGameObjUI, "_pet_detail");
    }

    public void SetZForShow()
    {
        //set z
        if (GameCommon.GetDataByZoneUid("vip_window_new_from") == "YES")
        {
            GameCommon.SetDataByZoneUid("vip_window_new_from", "NO");
            GameCommon.SetWindowZ(mGameObjUI, CommonParam.petDetailPosZ);
        }
        else
        {
            GameCommon.SetWindowZ(mGameObjUI, 0);
        }
    }

    

    public override void Open(object param) {
        base.Open(param);

        //设置
        SetZForShow();

        var tid=(int)param;
        curPet=new PetData(tid);
        GetUIToggle("infoBtn").value=true;        
        SetGridTabState ("pathBtn",ITEM_TYPE.CHARACTER != PackageManager.GetItemTypeByTableID(tid));
        GetUILabel("petName").text=TableCommon.GetStringFromActiveCongfig(tid,"NAME");
		GetUILabel("petName").color = GameCommon.GetNameColor (tid);
		GetUILabel("petName").effectColor = GameCommon.GetNameEffectColor ();
        UILabel label = GetUILabel("name_label");
		label.text = TableCommon.GetStringFromActiveCongfig(tid,"NAME");
        var preModel=GetCurUIGameObject("UIPoint").transform.parent.FindChild("_role_");
        if(preModel!=null) GameObject.Destroy(preModel.gameObject);
        //GameCommon.ShowModel(tid,GetCurUIGameObject("UIPoint"),1);
        GameCommon.SetCardInfo(mCard.name, tid, 1, 0, mMastButtonUI);
        RefreshInfo();
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
                RefreshPath();
                break;
            case "RELATE_INFO":
                int _tid = (int)objVal;
                GetUIToggle("relateBtn").value = true;
                RefreshRelate();
                break;
        }
    }
    #region 缘分面板
    struct RelateDetail 
    {
        public int mRelateID;   //> 缘分ID
        public int mTid;        //> 关联物品Tid
    }
    /// <summary>
    /// 获得相应符灵缘分所关联的物品的ID列表
    /// </summary>
    /// <param name="kPetID"></param>
    /// <returns></returns>
    private List<RelateDetail> GetRelateDetailList(int kPetID) 
    {
        List<RelateDetail> _relateDetailList = new List<RelateDetail>();
        List<int> _relateIDList = GetPetRelateIDList(kPetID);
        for (int i = 0, count = _relateIDList.Count; i < count; i++) 
        {
            DataRecord _record = DataCenter.mRelateConfig.GetRecord(_relateIDList[i]);
            for (int j = 1; j <= 6; j++) 
            {
                int _relateDetail = _record["NEED_CONTENT_" + j];
                if (_relateDetail != 0)
                    _relateDetailList.Add(new RelateDetail() { mRelateID = _relateIDList[i], mTid = _relateDetail });
            }
        }
        return _relateDetailList;
    }
    /// <summary>
    /// 获得符灵的缘分ID列表
    /// </summary>
    /// <param name="kPetTid"></param>
    /// <returns></returns>
    private List<int> GetPetRelateIDList(int kPetTid) 
    {
        List<int> _relateIDList = new List<int>();
        DataRecord _record = DataCenter.mActiveConfigTable.GetRecord(curPet.tid);
        for (int i = 1; i <= 15; i++) 
        {
            int _relateID = _record["RELATE_ID_" + i];
            if (_relateID != 0)
                _relateIDList.Add(_relateID);
            else
                break;
        }
        return _relateIDList;
    }
    private void RefreshRelateCell(GameObject kGridItem,RelateDetail kRelateData) 
    {
        DataRecord _record = DataCenter.mRelateConfig.GetRecord(kRelateData.mRelateID);
        if(_record == null)
            return;
        //设置缘分物品图标
        GameObject _relateIconObj = GameCommon.FindObject(kGridItem, "icon");
        if (_relateIconObj != null)
            GameCommon.SetOnlyItemIcon(_relateIconObj, kRelateData.mTid);
        //设置物品名称
        UILabel _relateItemNameLbl = GameCommon.FindComponent<UILabel>(kGridItem,"relate_item_name_label");
        Color _color = GameCommon.GetNameColor(kRelateData.mTid);
        if (_relateItemNameLbl != null) 
        {
            _relateItemNameLbl.text = GameCommon.GetItemName(kRelateData.mTid);
            _relateItemNameLbl.color = _color;
        }
        //设置是否拥有 ---------------------------------------
        GameObject _hasLblObj = GameCommon.FindObject(kGridItem,"has_label");
        _hasLblObj.GetComponent<UILabel>().color = _color;

        _hasLblObj.SetActive(PackageManager.IsHasItemByTid(kRelateData.mTid));
        //设置缘分描述
        UILabel _relateDescLbl = GameCommon.FindComponent<UILabel>(kGridItem, "tips");
        if (_relateDescLbl != null) 
        {
            _relateDescLbl.text = "【"+_record["RELATE_NAME"]+"】"+_record["RELATE_DWSCRIBE"];
        }
        //设置响应事件
        ITEM_TYPE _itemType = PackageManager.GetItemTypeByTableID(kRelateData.mTid);
        switch (_itemType) 
        {
            case ITEM_TYPE.PET:
                AddButtonAction(GameCommon.FindObject(kGridItem,"relate_get_btn"), () =>
                {
                    DataCenter.CloseWindow("SHOP_ALBUM_WINDOW");
                    DataCenter.OpenWindow(UIWindowString.petDetail, kRelateData.mTid);
                    DataCenter.SetData(UIWindowString.petDetail, "RELATE_TO_GET_PATH", kRelateData.mTid);
                });
                break;
            case ITEM_TYPE.EQUIP:
                AddButtonAction(GameCommon.FindObject(kGridItem, "relate_get_btn"), () =>
                {
                    DataCenter.CloseWindow("SHOP_ALBUM_WINDOW");
                    DataCenter.CloseWindow(UIWindowString.petDetail);
                    DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", kRelateData.mTid);
                });
                break;
            case ITEM_TYPE.MAGIC:
                AddButtonAction(GameCommon.FindObject(kGridItem, "relate_get_btn"), () =>
                {
                    DataCenter.CloseWindow("SHOP_ALBUM_WINDOW");
                    DataCenter.CloseWindow (UIWindowString.petDetail);
                    DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", kRelateData.mTid);
                    
                });
                break;
        }
   
    }
    
    private List<RelateDetail> GetDetailsListByRemoveSelf(List<RelateDetail> list)
    {
        List<RelateDetail> retList = new List<RelateDetail>();
        foreach (var temp in list)
        {
            if (temp.mTid != curPet.tid)
            {
                retList.Add(temp);
            }
        }
        return retList;
    }

    private void RefreshRelate() 
    {
        infoBoard.SetActive(false);
        pathBoard.SetActive(false);
        mRelateBoard.SetActive(true);

        List<RelateDetail> _relateDetailListTemp = GetRelateDetailList(curPet.tid);
        List<RelateDetail> _relateDetailList = GetDetailsListByRemoveSelf(_relateDetailListTemp);
        int _relateCount = _relateDetailList.Count;
        mRelateGridContainer.MaxCount = _relateCount;
        for (int i = 0; i < _relateCount; i++) 
        {
            RefreshRelateCell(mRelateGridContainer.controlList[i], _relateDetailList[i]);
        }
        mRelateGridContainer.Reposition();
        mRelateScrollView.ResetPosition();
    }
    #endregion
    void RefreshInfo() {
        infoBoard.SetActive(true);
        pathBoard.SetActive(false);
        mRelateBoard.SetActive(false);
		ITEM_TYPE tmpItemType = PackageManager.GetItemRealTypeByTableID(curPet.tid);
		if(tmpItemType == ITEM_TYPE.CHARACTER)
		{
			curPet.teamPos = (int)TEAM_POS.CHARACTER;
		}
        if (curPet.teamPos != (int)TEAM_POS.CHARACTER)
        {
            _mSKILL.SetActive(true);
            RefreshSkillBoard();
        }
        else
        {
            _mSKILL.SetActive(false);          
        }
        
        //if (!_mSKILL.activeSelf)
        //{
        //    _mRELATE.transform.localPosition = new Vector3(-47, -55, 0);
        //    _mTALENT.transform.localPosition = new Vector3(-47, -510, 0);
        //}        
        RefreshAttribute();
        RefreshRelationship();
        RefreshTalent();
        RefreshFragment();
		RefreshIntroduce();
    }
	void RefreshIntroduce()
	{
		if (curPet != null)
		{
			GameObject introduceObj = GameCommon.FindObject (_mINTRODUCE, "role_introduce_label").gameObject;
			var r = DataCenter.mActiveConfigTable.GetRecord(curPet.tid);
			string strDesc = "";
			if (r != null)
			{
				strDesc = r["DESCRIBE"];
			}
			introduceObj.GetComponent<UILabel>().text = strDesc;		
		}
	}

    void RefreshPath() {
        pathBoard.SetActive(true);
        infoBoard.SetActive(false);
        mRelateBoard.SetActive(false);
		UpdateGetWayUI();
    }

    void RefreshSkillBoard() {
        if(curPet!=null) {                     
            var skillName=GetUILabel("skillName");
            var skillDescribe=GetUILabel("skillDescribe");

            var info=new SkillInfo(curPet.tid,1);
            skillName.text=info.skillName;
            skillDescribe.text=info.describe;

            for(int i=0;i<4;i++) {
                var skillInfo=new SkillInfo(curPet.tid,i+1);
                var btn=GetCurUIGameObject("Button"+(i+1));
                if (skillInfo.id == 0) {
                    btn.SetActive(false);
                    continue;
                }
                btn.SetActive(true);
                var sprite = btn.transform.FindChild("sprite").GetComponent<UISprite>();
                sprite.atlas = GameCommon.LoadUIAtlas(skillInfo.atlasName);
                sprite.spriteName = skillInfo.spriteName;
                AddButtonAction(btn,() => {
                    skillName.text=skillInfo.skillName;
                    skillDescribe.text=skillInfo.describe;
                    DEBUG.Log(skillInfo.describe);
                });
            }
            //added by xuke begin
            GameObject _gridObj = GameCommon.FindObject(mGameObjUI, "Skill_Grid_Pos");
            if(_gridObj != null)
                _gridObj.GetComponent<UIGrid>().Reposition();
            //end
        }
    }
    void RefreshAttribute() {
        GetUILabel("AttackValue").text=((int)curPet.GetAttribute(AFFECT_TYPE.ATTACK)).ToString();
        GetUILabel("hpValue").text=((int)curPet.GetAttribute(AFFECT_TYPE.HP)).ToString();
        GetUILabel("phyDefValue").text=((int)curPet.GetAttribute(AFFECT_TYPE.PHYSICAL_DEFENCE)).ToString();
        GetUILabel("magicDefValue").text=((int)curPet.GetAttribute(AFFECT_TYPE.MAGIC_DEFENCE)).ToString();
    }

    public void AddIconList(DataRecord r, GameObject cell)
    {
        //add iconlist
        List<int> tidList = new List<int>();
        tidList.Clear();
        for (int j = 1; j <= 6; j++)
        {
            int tempTid = r["NEED_CONTENT_" + j.ToString()];
            if (tempTid != 0)
            {
                tidList.Add(tempTid);
            }
        }
        var iconGrid = GameCommon.FindObject(cell, "icon_grid").GetComponent<UIGridContainer>();
        iconGrid.MaxCount = tidList.Count;
        var iconGridList = iconGrid.controlList;
        for (int k = 0; k < tidList.Count; k++)
        {
            GameObject board = iconGridList[k];
            if (board != null)
            {
                int tid = tidList[k];
                GameCommon.SetOnlyItemIcon(GameCommon.FindObject(board, "iconBtn"), tid);
                GameCommon.FindObject(board, "iconBtn").GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f);
                GameCommon.SetUIText(board, "name", GameCommon.GetItemName(tid));
                GameCommon.FindObject(board, "name").GetComponent<UILabel>().color = GameCommon.GetNameColor(tid);

                //btn 
                GameCommon.AddButtonAction(GameCommon.FindObject(board, "iconBtn"), () =>
                {
                    //GameCommon.SetItemDetailsWindow(tid);
                });
            }
        }
    }

    void RefreshRelationship() {
        List<int> list=new List<int>();
        for(int i=0;i<15;i++) {
            var index=TableCommon.GetNumberFromActiveCongfig(curPet.tid,"RELATE_ID_"+(i+1));
            if(index!=0) list.Add(index);
        }
        var container=GetUIGridContainer("relateGrid",list.Count);

        //set bg height
        GameObject objBg = GameCommon.FindObject(_mRELATE, "relateBG");
        objBg.GetComponent<UISprite>().height = list.Count * (int)container.CellHeight + 25;

        for(int i=0;i<list.Count;i++) {
            var grid=container.controlList[i].transform;
            var record=DataCenter.mRelateConfig.GetRecord(list[i]);
            grid.FindChild("relate_name").GetComponent<UILabel>().text = record.getData("RELATE_NAME");
            grid.FindChild("relate_tips").GetComponent<UILabel>().text = record.getData("RELATE_DWSCRIBE");

            //add list
            AddIconList(record, container.controlList[i]);
        }
        container.Reposition();
        container.repositionNow = false;
        container.gameObject.GetComponent<DynamicGridContainer>().Adjust();
        //container.Reposition();
        //GetSprite("relateBG").height=150+list.Count*10;
        //GetSprite("relateSprite").height=100+list.Count*10;
        //relateCount=list.Count;
    }

    void RefreshTalent() {
        Vector3 relateGroupPos=GetCurUIGameObject("relateGroup").transform.localPosition;
        //GetCurUIGameObject("talentGroup").transform.localPosition=new Vector3(relateGroupPos.x,relateGroupPos.y-15*relateCount-130,relateGroupPos.z);
        var record=DataCenter.mBreakBuffConfig.GetRecord(curPet.tid);
        List<int> list=new List<int>();
        for(int i=0;i<15;i++) {
            var index=TableCommon.GetNumberFromBreakBuffConfig(curPet.tid,"BREAK_"+(i+1));
            if(index!=0) list.Add(index);            
        }

        var container=GetUIGridContainer("talentGrid",list.Count);
        for(int i=0;i<list.Count;i++) {
            var grid=container.controlList[i].transform;
            var affectRecord=DataCenter.mAffectBuffer.GetRecord(list[i]);
            grid.FindChild("name").GetComponent<UILabel>().text=affectRecord.getData("NAME");
            grid.FindChild("describe").GetComponent<UILabel>().text = affectRecord.getData("TIP_TITLE");
        }
        container.Reposition();
        container.repositionNow = false;
		container.gameObject.GetComponent<DynamicGridContainer>().Adjust();
        //container.Reposition();
        //GetSprite("talentBG").height=60+list.Count*(int)container.CellHeight;
        //GetSprite("talentSprite").height=15+list.Count*(int)container.CellHeight;
    }

    void RefreshFragment() {

        var allRecord=DataCenter.mFragment.GetAllRecord();
        int fragmentTid=0;
        allRecord.Values.Foreach(record=>{
            if(record.getData("ITEM_ID")==curPet.tid)
                fragmentTid=record.getIndex();
        });

        var curFragment=PetFragmentLogicData.Self.mDicPetFragmentData.Values.Where(data => data.tid==fragmentTid).SingleOrDefault();
        var curCount=(curFragment==null)?0:curFragment.itemNum;
        var totalCount=TableCommon.GetNumberFromFragment(fragmentTid,"COST_NUM");
        GetUILabel("fragment").text=curCount+"/"+totalCount;
    }

	void UpdateGetWayUI()
	{
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grid");
		int petFragmentTid = 0;
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mFragment.GetAllRecord ())
		{
			if(v.Value["ITEM_ID"] == curPet.tid)
			{
				petFragmentTid = v.Key;
			}
		}
		GameCommon.GetParth (grid, petFragmentTid);
	}
}


