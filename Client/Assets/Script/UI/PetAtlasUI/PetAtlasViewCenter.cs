using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using UnityEngine;
using System.Collections;

public enum PetElement : int
{
    All = 0,
    Fire = 1,
    Water = 2,
    Wood = 3,
    Gold = 4,
    Soil = 5
}

public enum PetElementState : int
{
    NotHave,
    Own,
    MaxLevel
}


/*
 *    注册协议 获取服务端数据  
 *    玩家的代币
 *      
 */
public class PetAtlasViewCenter : PetAtlasBehaviourBase
{
    private static PetAtlasViewCenter _Instance;
    public static PetAtlasViewCenter Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<PetAtlasViewCenter>();
            }
            return _Instance;
        }
    }

    public PetAtailsDetailsPanel PetAtailsDetails;
    public List<UIGrid> TableAnchors = new List<UIGrid>();

    public InfoInStartView StartView = new InfoInStartView();

    public UIScrollView ScrollView_1;
    public UIScrollView ScrollView_2;
    public GameObject TransParent;

    public UIGrid KnightOfFateTrans;


	//符灵详情面板
    [HideInInspector]
    private PetInfoDescriptionUI infoDescriptionUi = null;
    public GameObject Tablefab;
    public KnightOfFateEleData knightOfFateEle;
    //tableContext 对象的缓存队列
    private List<PetAtlasTableContext> _tableContexts = new List<PetAtlasTableContext>();
    private List<string> petElements = new List<string>() { "火", "水", "木", "阳", "阴" };
    private TuJianCategoricalData categoricalData;

    void Awake() { _Instance = this; }

    void Start()
    {
        if (Tablefab == null) Tablefab = (GameObject)Resources.Load("Prefabs/UI/PetAtlasfab/TableContext");
        if (knightOfFateEle == null) knightOfFateEle = (KnightOfFateEleData)Resources.Load("Prefabs/UI/PetAtlasfab/KnightOfFateEle");


        PetAtlasWindowCenter petAtlasWindow = new PetAtlasWindowCenter();
        KnightPartyWindow knightParty = new KnightPartyWindow();

        DataCenter.RegisterData("PetAtlasViewCenter", petAtlasWindow);
        DataCenter.RegisterData("KnightPartyWindow", knightParty);
        DataCenter.RegisterData("KnightDetailsWindow", new PetAtailsDetailsWindow());

        base.RegisDataName.Add("KnightPartyWindow");
        base.RegisDataName.Add("PetAtlasViewCenter");
        base.RegisDataName.Add("KnightDetailsWindow");


        petAtlasWindow.Open(true);
        knightParty.Open(true);
    }


    //初始化所有侠客面板 
    public void InitAllInfo(TuJianCategoricalData categoricalData)
    {
        this.categoricalData = categoricalData;
        InitPlayerTokenInfo();
        StartCoroutine(InitPetView(categoricalData));
    }
	//传入的参数现在是全部符灵信息  未分类 
    private IEnumerator InitPetView(TuJianCategoricalData categoricalData)
    {
        float uiGridPos = 0;
        float uiSubHight = 0;
        for (int i = 0; i < TableAnchors.Count; i++)
        {
            GameObject tableClone = Instantiate(Tablefab) as GameObject;
            tableClone.transform.parent = TableAnchors[i].transform;
            tableClone.transform.localPosition = Vector3.zero;
            tableClone.transform.localScale = Vector3.one;
            //set data to init
            tableClone.GetComponent<PetAtlasTableContext>().Init(categoricalData.TujianDatas[i], categoricalData.LocalTujianDatas[i], petElements[i]);

            TableAnchors[i].transform.localPosition = new Vector3(0, uiGridPos, 0);
            uiGridPos = uiGridPos + ComputeHight(categoricalData.LocalTujianDatas[i].Count);

            yield return new WaitForSeconds(0.12f);   //没创建一个Table 等待一下    为解决加载时假死
        }
        Tablefab = null;
        Resources.UnloadUnusedAssets();

        ScrollView_1.ResetPosition();
    }
    //初始化 代币信息
    private void InitPlayerTokenInfo()
    {
        StartView.LabPower.text = "99999";
        StartView.LabGold.text = "99999";
        StartView.LabDiamond.text = "99999";
        int totalCount = 0;
        int ownCount = 0;
        for (int i = 0; i < categoricalData.LocalTujianDatas.Count; i++)
        {
            ownCount += categoricalData.TujianDatas[i].Count;
            totalCount += categoricalData.LocalTujianDatas[i].Count;
        }
        StartView.KnightTotalCount.text = ownCount + "/" + totalCount;  //   10/100
    }



    //初始化侠客缘分面板
    public void InitKnightPartyPanel(List<KnightPartyInfo> knightPartyInfos)
    {
        InitKnightParty(knightPartyInfos);
    }
    //初始化缘分列表
    private void InitKnightParty(List<KnightPartyInfo> knightPartyInfos)
    {
        for (int i = 0; i < knightPartyInfos.Count; i++)
        {
            KnightOfFateEleData fate = (KnightOfFateEleData)Instantiate(knightOfFateEle);
            fate.transform.parent = KnightOfFateTrans.transform;
            fate.transform.localPosition = Vector3.zero;
            fate.transform.localScale = Vector3.one;
            fate.Init(knightPartyInfos[i]);
        }
        KnightOfFateTrans.Reposition();
        ScrollView_2.ResetPosition();
    }


    //Tabclick  点击事件
    public void TypeTabClick(string thisTab)
    {
        DEBUG.Log("thisTab====" + thisTab);
        int GridIndex = int.Parse(thisTab);
        if (GridIndex == 11)
        {
            ReComputeTabChild();
            return;
        }

        for (int i = 0; i < TableAnchors.Count; i++)
        {
            TableAnchors[i].gameObject.SetActive(i == GridIndex);
            if (i == GridIndex)
            {
                TableAnchors[i].transform.localPosition = Vector3.zero;
                TableAnchors[i].keepWithinPanel = true;
                TableAnchors[i].transform.GetChild(0).transform.localPosition = Vector3.zero;
            }
        }
        TableAnchors[GridIndex].Reposition();
        ScrollView_1.ResetPosition();
    }
    //Tab0 的click事件   同时初始化图鉴的方法 
    void ReComputeTabChild()
    {
        float uiGridPos = 0;

        for (int i = 0; i < TableAnchors.Count; i++)
        {
            TableAnchors[i].gameObject.SetActive(true);
            TableAnchors[i].transform.localPosition = new Vector3(0, uiGridPos, 0);   //_tableContexts[i].TableHight  
            uiGridPos = uiGridPos + ComputeHight(categoricalData.LocalTujianDatas[i].Count);
            TableAnchors[i].transform.localScale = Vector3.one;
            TableAnchors[i].keepWithinPanel = false;
            TableAnchors[i].sorted = false;
        }
        StartCoroutine(UpdateScrollView());
    }
	//显示所有符灵 初始化位置
    private IEnumerator UpdateScrollView()
    {
        //保证列表刷新
        yield return new WaitForEndOfFrame();
        ScrollView_1.ResetPosition();
        yield return new WaitForEndOfFrame();
        ScrollView_1.ResetPosition();
    }


    //打开详细面板
    public void OpenDetailsWindow(KnightData knightData)
    {
        PetAtailsDetails.CurrPetId = knightData.PetId;
        PetAtailsDetails.knightData = knightData;
        PetAtailsDetails.gameObject.SetActive(true);
    }
    //关闭详细面板
    public void CloseDetailsWindow()
    {
        PetAtailsDetails.CloseWindow();
    }


    //设置Uigrid的位置  不叠加
    public float ComputeHight(int count)
    {
        float hight = (Mathf.Ceil(count * 1.0f / 8.0f));
        float realHight = -(113f * hight + 50);
        return realHight;
    }


}


[System.Serializable]
public class InfoInStartView
{
    public UILabel LabPower;
    public UILabel LabGold;
    public UILabel LabDiamond;
    public UILabel KnightTotalCount;
}

public class PetAtlasWindowCenter : tWindow
{
    public PetAtlasViewCenter ViewCenter;
    public override void Init()
    {
        mGameObjUI = PetAtlasViewCenter.Instance.gameObject;
        ViewCenter = PetAtlasViewCenter.Instance;
    }

    public override void Open(object param)
    {
        if (ViewCenter == null)
        {
            ViewCenter = PetAtlasViewCenter.Instance;
        }

        TujianLogicData logicData = (TujianLogicData)DataCenter.GetData("TUJIAN_DATA");
        TuJianCategoricalData categoData = new TuJianCategoricalData(logicData.mTujianList);
        TujianData[] tujianDatas = TableCommon.GetTujianDataListFromActiveConfig();

        EventCenter.Log(false, "tujianDatas.length==" + tujianDatas.Length);
        EventCenter.Log(false, "categoData.count=" + categoData.TujianDatas.Count);
		//所有的静态数据  和  服务端的符灵数据
        ViewCenter.InitAllInfo(categoData);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public void OpenDetailsWindow(KnightData knightData)
    {
        mGameObjUI.GetComponent<PetAtlasViewCenter>().OpenDetailsWindow(knightData);
    }

    public void CloseDetailsWindow()
    {
        mGameObjUI.GetComponent<PetAtlasViewCenter>().CloseDetailsWindow();
    }

}



//侠客指令  侠客图鉴中的单个侠客的一切由指令提供
[System.Serializable]
public class KnightData
{
    public bool Own;
    public bool ShangZhen;
    public string ModelIcon;
    public int PetId;
    public string Atk = "?";
    public string Blood = "?";
    public int ElementType;
    public int StarLevel;
    /// <summary>
    /// 当前的侠客处于的状态   
    /// </summary>
    public TUJIAN_STATUS TujianStatus;
    /// <summary>
    /// 详情面板的显示提示 还是 领取的按钮
    /// </summary>
    public KnightDetailsButtonState ButtonState
    {
        get
        {
            switch (TujianStatus)
            {
                case TUJIAN_STATUS.TUJIAN_FULL:
                    return KnightDetailsButtonState.GetMaxPrice;
                case TUJIAN_STATUS.TUJIAN_NEW:
                    return KnightDetailsButtonState.GetTip;
                case TUJIAN_STATUS.TUJIAN_NORMAL:
                    return KnightDetailsButtonState.GetTip;
                case TUJIAN_STATUS.TUJIAN_REWARD:
                    return KnightDetailsButtonState.GetTip;
            }
            return KnightDetailsButtonState.GetTip;
        }
    }

}

public enum KnightDetailsButtonState
{
    GetTip,
    GetMaxPrice
}

public enum KnightPartyState
{
    CanActive,      //可以激活 但是还没有激活
    NoSomething, //不符合提交
    ActivationTag //已经激活
}



//图鉴显示 数据模型
public class TuJianCategoricalData
{
    public List<List<KnightData>> TujianDatas;
    public List<List<KnightData>> LocalTujianDatas;
	private Dictionary<int, PetData> petDatas;
    public TuJianCategoricalData(TujianData[] tujianDatas)
    {
        TujianDatas = CategoricalData(tujianDatas, false);
        LocalTujianDatas = CategoricalData(TableCommon.GetTujianDataListFromActiveConfig(), true);
    }
    //解析图鉴数据
    private List<List<KnightData>> CategoricalData(TujianData[] tujianDatas, bool isLocal)
    {
        List<List<KnightData>> TempTujianDatas = new List<List<KnightData>>();
        int eleType = 0;
        for (int i = 1; i < 6; i++)
        {
            List<KnightData> box = new List<KnightData>();
            for (int j = 0; j < tujianDatas.Length; j++)
            {
                eleType = TableCommon.GetNumberFromActiveCongfig(tujianDatas[j].mID, "ELEMENT_INDEX");
                if (eleType != (i - 1)) continue;

                KnightData data = new KnightData();
                data.ElementType = TableCommon.GetNumberFromActiveCongfig(tujianDatas[j].mID, "ELEMENT_INDEX");
                data.PetId = tujianDatas[j].mID;
                data.Own = !isLocal;
                data.ShangZhen = CheckPetShangZhen(tujianDatas[j].mID);
                data.ModelIcon = TableCommon.GetStringFromActiveCongfig(tujianDatas[j].mID, "HEAD_SPRITE_NAME");
                data.TujianStatus = (TUJIAN_STATUS)tujianDatas[j].mStatus;
                data.StarLevel = TableCommon.GetNumberFromActiveCongfig(tujianDatas[j].mID, "STAR_LEVEL");
                if (data.TujianStatus == TUJIAN_STATUS.TUJIAN_FULL)
                {
                    data.Atk = GameCommon.GetBaseAttack(tujianDatas[j].mID, 30, 0).ToString();
                    data.Blood = GameCommon.GetBaseMaxHP(tujianDatas[j].mID, 30, 0).ToString();
                }
                box.Add(data);
            }
            TempTujianDatas.Add(box);
        }
        return TempTujianDatas;
    }
	//检测符灵是否上阵
    private bool CheckPetShangZhen(int mId)
    {
		PetLogicData logicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		
		if (petDatas == null)
			petDatas = logicData.mDicPetData;

		if(petDatas.ContainsKey(mId))
		{
			return true;
		}
        return false;
    }

}

//侠客缘分 数据模型
public class KnightPartyInfo
{

	private Dictionary<int, PetData> petDatas;
    private TujianLogicData logicData;
    public int GroupId;
    //将表数据模型转换成View指令
    public List<KnightData> KnightIdGroups;
    public string AddtiveTitle;
    public string AddtiveDes;
    private KnightPartyState partyState;
    public KnightPartyState PartyState
    {
        get
        {
            int shangzhanNum = KnightIdGroups.Count(k => k.ShangZhen);
            if (shangzhanNum == KnightIdGroups.Count)
                return KnightPartyState.ActivationTag;
            if (KnightIdGroups.Count(k => k.Own) == KnightIdGroups.Count)
                return KnightPartyState.CanActive;
            else
                return KnightPartyState.NoSomething;
        }
    }

    private List<int> knightIds;
    public List<int> KnightIds
    {
        get
        {
            if (knightIds != null || knightIds.Count == 0)
            {
                knightIds = new List<int>();
                for (int i = 0; i < KnightIdGroups.Count; i++)
                    knightIds.Add(KnightIdGroups[i].PetId);
            }
            return knightIds;
        }
    }

    public KnightPartyInfo()
    {
    }

    public KnightPartyInfo(KnightPartyTableModel knightIds, TujianData[] tujianDatas)
    {
        AddtiveTitle = knightIds.title;
        AddtiveDes = knightIds.property;
        GroupId = knightIds.groundId;
        ResolveTableModel(knightIds, tujianDatas);
    }
    //解析侠客缘分表模型实体
    private void ResolveTableModel(KnightPartyTableModel tableModel, TujianData[] tujianDatas)
    {
        KnightIdGroups = new List<KnightData>();
        for (int j = 0; j < tableModel.KnghitIds.Length; j++)
        {
            KnightData data = new KnightData();
            data.Own = CheckOwn(tableModel.KnghitIds[j]);
            data.TujianStatus = (TUJIAN_STATUS)CheckPetState(tableModel.KnghitIds[j], tujianDatas);
            data.ElementType = TableCommon.GetNumberFromActiveCongfig(tableModel.KnghitIds[j], "ELEMENT_INDEX");
            data.PetId = tableModel.KnghitIds[j];
            data.ShangZhen = CheckPetShangZhen(tableModel.KnghitIds[j]);
            data.ModelIcon = TableCommon.GetStringFromActiveCongfig(tableModel.KnghitIds[j], "HEAD_SPRITE_NAME");
            KnightIdGroups.Add(data);
        }
    }

    //是否拥有
    private bool CheckOwn(int petid)
    {
        if (logicData == null) logicData = (TujianLogicData)DataCenter.GetData("TUJIAN_DATA");
        for (int i = 0; i < logicData.mTujianList.Length; i++)
        {
            if (logicData.mTujianList[i].mID == petid) return true;
        }
        return false;
    }
	//检测符灵是否上阵
    private bool CheckPetShangZhen(int mId)
    {
		PetLogicData logicData = DataCenter.GetData("PET_DATA") as PetLogicData;

        if (petDatas == null)
			petDatas = logicData.mDicPetData;


		if(petDatas.ContainsKey(mId))
		{
			return true;
		}
//        for (int i = 0; i < petDatas.Length; i++)
//        {
//            if (petDatas[i].mDBID == mId)
//                return true;
//        }
        return false;
    }
    //检测静态数据中的侠客 玩家是否拥有 有返回状态  没有 返回-1
    private int CheckPetState(int petId, TujianData[] tujianDatas)
    {
        int state = -1;
        for (int i = 0; i < tujianDatas.Length; i++)
        {
            if (tujianDatas[i].mID == petId)
                state = tujianDatas[i].mStatus;
        }
        return state;
    }

}

//侠客缘分表模型实体
public class KnightPartyTableModel
{
    public int groundId;
    public string title;
    public string property;
    public int addtiveId;
    public int activeCount;
    public int[] KnghitIds;
}



public class PetAtailsDetailsWindow : tWindow
{
    public TuJianCategoricalData CategoricalData;
    public KnightDesInfo DesModel;
	public GameObject mRoleMastButtonUI;

    public override void Init()
    {
        if (mGameObjUI != null)
            mGameObjUI = GameCommon.FindUI("knightDesPanel");
        // CategoricalData=new TuJianCategoricalData( )
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "SET_SELECT_PET_BY_MODEL_INDEX")
        {
            SetPetByModelIndex((int)objVal);
        }
    }

    public void SetPetByModelIndex(int iModelIndex)
    {
        DesModel.mModelIndex = iModelIndex;
        if (DesModel.mModelIndex == -1)
            return;
        DesModel.mPetStarLevelLabel.text = (TableCommon.GetNumberFromActiveCongfig(DesModel.mModelIndex, "STAR_LEVEL")).ToString();
        DesModel.mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(DesModel.mModelIndex, "NAME");
        DesModel.mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(DesModel.mModelIndex, "NAME");
        DesModel.mDescriptionLabel.text = "请叫我描述";

        SetAttack();
        SetMaxHP();
        SetElementInfo();
        // 主动技能
        SetSkillInfo(DesModel.mSkillBtn1, 1);

        // 被动技能
        //SetSkillInfo(mSkillBtn2, 2);
        //SetSkillInfo(mSkillBtn3, 3);

		GameCommon.SetCardInfo(DesModel.mCard.name, DesModel.mModelIndex, DesModel.mLevel, DesModel.mStrengthenLevel, mRoleMastButtonUI);
	}
	
    public void SetAttack()
    {
        int iAcctack = (int)GameCommon.GetBaseAttack(DesModel.mModelIndex, DesModel.mLevel, DesModel.mStrengthenLevel);
        DesModel.mAttackLabel.text = iAcctack.ToString();
    }

    public void SetMaxHP()
    {
        int iMaxHP = GameCommon.GetBaseMaxHP(DesModel.mModelIndex, DesModel.mLevel, DesModel.mStrengthenLevel);
        DesModel.mMaxHPLabel.text = iMaxHP.ToString();
    }

    public void SetElementInfo()
    {
        int iElementIndex = TableCommon.GetNumberFromActiveCongfig(DesModel.mModelIndex, "ELEMENT_INDEX");

        DesModel.mElementNameLabel.text = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_NAME");

        // set icon
        string strAtlasName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);

        string strSpriteName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_SPRITE_NAME");

        DesModel.mElementIcon.atlas = tu;
        DesModel.mElementIcon.spriteName = strSpriteName;
        DesModel.mElementIcon.MakePixelPerfect();
    }

    public void SetSkillInfo(GameObject objBtn, int iIndex)
    {
        GameObject obj = GameCommon.FindObject(objBtn, "Background");
        if (obj == null)
            return;

        UISprite sprite = obj.GetComponent<UISprite>();

        if (sprite != null)
        {
            int iSkillIndex = TableCommon.GetNumberFromActiveCongfig(DesModel.mModelIndex, "PET_SKILL_" + iIndex.ToString());

            // 去技能表里取技能数据
            string strAtlasName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);

            string strSpriteName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_SPRITE_NAME");

            sprite.atlas = tu;
            sprite.spriteName = strSpriteName;
            sprite.MakePixelPerfect();
        }
    }

    public void SetInfoByKnightInfo(KnightDesInfo desInfo)
    {

    }

    public void ShowStar(int starcount)
    {
        mGameObjUI.GetComponent<PetAtailsDetailsPanel>().CardInstance.ShowStars(starcount);
    }
}

public class KnightPartyWindow : tWindow
{
    public PetAtlasViewCenter ViewCenter;
    public override void Init()
    {
        mGameObjUI = PetAtlasViewCenter.Instance.gameObject;
        ViewCenter = PetAtlasViewCenter.Instance;
    }

    // 缓存到NiceData中
    public override void set(string indexName, object objVal)
    {
        base.set(indexName, objVal);
    }

    public override void Open(object param)
    {
        if (ViewCenter == null) ViewCenter = PetAtlasViewCenter.Instance;
        KnightPartyTableModel[] knightPartys = TableCommon.GetPetPartyModelFromPetOfPredestined();
        TujianData[] localData = TableCommon.GetTujianDataListFromActiveConfig();
        List<KnightPartyInfo> knightPartyInfos = new List<KnightPartyInfo>();
        for (int i = 0; i < knightPartys.Length; i++)
        {
            KnightPartyInfo info = new KnightPartyInfo(knightPartys[i], localData);
            knightPartyInfos.Add(info);
        }
        ViewCenter.InitKnightPartyPanel(knightPartyInfos);
    }
}


public class ButtonTypeEvent : CEvent
{
    public override bool _DoEvent()
    {
        PetAtlasWindowCenter logicData = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
        string[] names = GetEventName().Split('_');
        logicData.mGameObjUI.GetComponent<PetAtlasViewCenter>().TypeTabClick(names[names.Length - 1]);
        return true;
    }
}

public class SubCellClick : CEvent
{
    public override bool _DoEvent()
    {
        PetAtlasWindowCenter windowCenter = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
        object obj;
        bool result = getData("BUTTON", out obj);

        if (result)
        {
            //得到侠客的信息   赋值到 详情面板上
            GameObject subCell = obj as GameObject;
            KnightData knightData = subCell.GetComponent<PetAtlasTableCellInfo>().knightData;
            windowCenter.OpenDetailsWindow(knightData);    //int.Parse(knightData.PetId)  or 1006
        }
        else
        {
            EventCenter.Log(false, "No object in Button");
        }


        return true;
    }
}

public class PetAtlasDetailClose : CEvent
{
    public override bool _DoEvent()
    {
        PetAtlasWindowCenter center = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
        center.mGameObjUI.GetComponent<PetAtlasViewCenter>().CloseDetailsWindow();
        return base._DoEvent();
    }
}

public class PetAtlasReturnBack : CEvent
{
    public override bool _DoEvent()
    {
        //GlobalModule.Instance.LoadScene("MainUI", false);

        //PetAtlasWindowCenter center = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
        //center.mGameObjUI.SetActive(false);
        return true;
    }
}

public class ButtonPageLeft : CEvent
{
    public override bool _DoEvent()
    {
        object obj;
        getData("BUTTON", out obj);
        GameObject btn = (GameObject)obj;
        NGUITools.FindInParents<KnightOfFateEleData>(btn).PageScroll.NextPage();
        return true;
    }
}

public class ButtonPageRight : CEvent
{
    public override bool _DoEvent()
    {
        object obj;
        getData("BUTTON", out obj);
        GameObject btn = (GameObject)obj;
        NGUITools.FindInParents<KnightOfFateEleData>(btn).PageScroll.PreviousPage();
        return true;
    }
}

public class BtnKnightPanelClose : CEvent
{
    public override bool _DoEvent()
    {
        PetAtlasWindowCenter center = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
        center.CloseDetailsWindow();
        return base._DoEvent();
    }
}

