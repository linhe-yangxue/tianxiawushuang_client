using UnityEngine;
using System.Collections;
using DataTable;

public class GainPetInfoUI : MonoBehaviour {	
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;

	public GameObject mCard;
	public float mfCardScale = 1.0f;
	public GameObject mRoleMastButtonUI;

	// Use this for initialization
	void Start () {
		InitCard();

		Init();
	}

	public virtual void InitCard()
	{
		GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window", mCard.name);
		if(obj != null)
		{
			CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
			uiScript.InitPetInfo(mCard.name, mfCardScale, gameObject);
		}
		
		mCard.transform.localScale = mCard.transform.localScale * mfCardScale;
	}

	// Update is called once per frame
	void Update () {
	
	}

	public virtual void Init()
	{
		GainPetInfoWindow petInfoWindow = new GainPetInfoWindow();
		DataCenter.Self.registerData("GainPetInfoWindow", petInfoWindow);

		petInfoWindow.mPetStarLevelLabel = mPetStarLevelLabel;
		petInfoWindow.mPetTitleLabel = mPetTitleLabel;
		petInfoWindow.mPetNameLabel = mPetNameLabel;
		petInfoWindow.mCard = mCard;
		petInfoWindow.mRoleMastButtonUI = mRoleMastButtonUI;
	}

	public void OnDestroy()
	{
        //by chenliang
        //begin

//		DataCenter.Remove("GainPetInfoWindow");
//--------------
        //不能移除GainPetInfoWindow

        //end
	}
}


public class GainPetInfoWindow : tWindow{

	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;

	public PetData mPetData = null;
	public DataRecord mDataRecord = null;

	public float mfScaleRatio = 1.0f;

	public GameObject mCard;
	public GameObject mRoleMastButtonUI;

    public string m_strWindowName = "GainPetInfoWindow";

	public override void Init ()
	{
		mGameObjUI = GameCommon.FindUI(m_strWindowName);

		Close();
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SET_SELECT_PET_BY_MODEL_INDEX")
		{
			SetPetByModelIndex((int)objVal);
		}
		else if(keyIndex == "SET_SELECT_PET_BY_DBID")
		{
			SetPetByDBID((int)objVal);
		}
	}

	public virtual void SetPetByModelIndex(int iModelIndex)
	{
		GameCommon.SetCardInfo(mCard.name, iModelIndex, 1, 0, mRoleMastButtonUI);
	}

	public virtual void SetPetByDBID(int iItemId)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iItemId);
		if(mPetData != null)
		{
			GameCommon.SetCardInfo(mCard.name, mPetData.tid, mPetData.level, mPetData.strengthenLevel, mRoleMastButtonUI);
		}
	}
}