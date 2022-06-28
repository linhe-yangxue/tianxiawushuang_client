using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DataTable;

public class CardGroupUI : MonoBehaviour
{
    public UILabel mStrengthenLevelLabel;
    public UILabel mAttackLabel;
    public UILabel mMaxHPLabel;
	public UILabel mNameLabel;
    public UILabel mTypeLabel;
    public UILabel mElementLabel;
    public UILabel mLevelLabel;
    public UILabel mAptitudeLabel;
    public UISprite mSkillIcon;
    public UISprite mElementIcon;
    public UISprite mTypeIcon;
    public UISprite mInfoCard;
	public GameObject mInfoCardBG;

	public GameObject mEffectFiveBlue;
	public GameObject mEffectFiveGreen;
	public GameObject mEffectFiveGold;
	public GameObject mEffectFiveRed;
	public GameObject mEffectFiveShadow;
	public GameObject mEffectFour;
	
    public GameObject mPetObj;
    public string mStrName;

    public float mfCardScale = 1.0f;

    public List<StarModel> Stars = new List<StarModel>();
    // Use this for initialization
    void Start()
    {
    }

    public void InitPetInfo(string strName, float fCardScale, GameObject obj)
    {
        mStrName = strName;
        mfCardScale = fCardScale;
        CardGroupWindow cardGroupWindow = new CardGroupWindow();
        DataCenter.Self.registerData("CardGroupWindow" + mStrName, cardGroupWindow);

        //PetInfoSingleWindow petInfoData = DataCenter.GetData("PET_INFO_SINGLE_WINDOW") as PetInfoSingleWindow;

        cardGroupWindow.mStrengthenLevelLabel = mStrengthenLevelLabel;
        cardGroupWindow.mAttackLabel = mAttackLabel;
        cardGroupWindow.mMaxHPLabel = mMaxHPLabel;
		cardGroupWindow.mNameLabel = mNameLabel;
        cardGroupWindow.mTypeLabel = mTypeLabel;
        cardGroupWindow.mElementLabel = mElementLabel;
		cardGroupWindow.mLevelLabel = mLevelLabel;
        cardGroupWindow.mAptitudeLabel = mAptitudeLabel;
        cardGroupWindow.mSkillIcon = mSkillIcon;
        cardGroupWindow.mElementIcon = mElementIcon;
        cardGroupWindow.mTypeIcon = mTypeIcon;
        cardGroupWindow.mInfoCard = mInfoCard;
        cardGroupWindow.mInfoCardBG = mInfoCardBG;
		cardGroupWindow.mEffectFiveBlue = mEffectFiveBlue;
		cardGroupWindow.mEffectFiveGold = mEffectFiveGold;
		cardGroupWindow.mEffectFiveGreen = mEffectFiveGreen;
		cardGroupWindow.mEffectFiveRed = mEffectFiveRed;
		cardGroupWindow.mEffectFiveShadow = mEffectFiveShadow;
		cardGroupWindow.mEffectFour = mEffectFour;

        cardGroupWindow.mPetObj = mPetObj;

        cardGroupWindow.mfCardScale = mfCardScale;

        //cardGroupWindow.set ("CLOSE", true);

        SetDepth(obj);

		SetStartingRenderQueue();
    }

	public void SetStartingRenderQueue()
	{
		UIPanel parentPanel = gameObject.transform.parent.GetComponent<UIPanel>();
		if(parentPanel != null)
		{
			UIPanel panel = gameObject.GetComponent<UIPanel>();
			if(panel != null)
			{
				panel.renderQueue = UIPanel.RenderQueue.StartAt;
				panel.startingRenderQueue = parentPanel.startingRenderQueue + 5;
			}
		}

//		GameObject StarsGridObj = gameObject.transform.Find ("info/StarsGrid").gameObject;
//		UIPanel p = StarsGridObj.GetComponent<UIPanel>();
//		if(p == null)
//		{
//			StarsGridObj.AddComponent<UIPanel>();
//			UIPanel pp = StarsGridObj.GetComponent<UIPanel>();
//			pp.renderQueue = UIPanel.RenderQueue.StartAt;
//			pp.startingRenderQueue = gameObject.GetComponent<UIPanel>().startingRenderQueue;
//		}

		SetEffectRenderQueue(mEffectFiveBlue);
		SetEffectRenderQueue(mEffectFiveRed);
		SetEffectRenderQueue(mEffectFiveGold);
		SetEffectRenderQueue(mEffectFiveGreen);
		SetEffectRenderQueue(mEffectFiveShadow);
		SetEffectRenderQueue(mEffectFour);

	}

	void SetEffectRenderQueue(GameObject obj)
	{
		ParticleSystem [] particleSystems = obj.GetComponentsInChildren <ParticleSystem>();
		if(particleSystems != null)
		{
			foreach(ParticleSystem p in particleSystems)
			{
				p.renderer.material.renderQueue = gameObject.GetComponent<UIPanel>().startingRenderQueue + 2;

				for(int i = 1; i < 6; i++)
				{
					if(p.transform.name == "star" + i.ToString ())
					{
						p.renderer.material.renderQueue = gameObject.GetComponent<UIPanel>().startingRenderQueue + 4;
						p.gameObject.SetActive (false);
					}
				}
			}
		}

		MeshRenderer[] meshRenderers = obj.GetComponentsInChildren <MeshRenderer>();
		if(meshRenderers != null)
		{
			foreach(MeshRenderer m in meshRenderers)
			{
				m.material.renderQueue = gameObject.GetComponent<UIPanel>().startingRenderQueue + 2;
			}
		}
	}


    void onDestroy()
    {
        DataCenter.Remove("CardGroupWindow" + mStrName);
    }

    void SetDepth(GameObject obj)
    {
        UIPanel srcPanel = obj.GetComponent<UIPanel>();
        UIPanel panel = gameObject.GetComponent<UIPanel>();
        if (obj != null && srcPanel != null && panel != null)
        {
            panel.depth = srcPanel.depth + 1;
        }
    }

    public void ShowStars(int starNum)
    {
        StartCoroutine(ShowStar(starNum));
    }
    private IEnumerator ShowStar(int starNum)
    {
        if (starNum == -1) yield return null;

        for (int i = 0; i < starNum; i++)
        {
            Stars[i].Play();
            yield return new WaitForSeconds(0.1f);
        }
    }

    //初始化 重置侠客详情
    public void ResetCardToBegin()
    {
        Stars.ForEach(s => s.ReSetState());
    }

}


public class CardGroupWindow : tWindow
{
    public UILabel mStrengthenLevelLabel;
    public UILabel mAttackLabel;
    public UILabel mMaxHPLabel;
	public UILabel mNameLabel;
    public UILabel mTypeLabel;
    public UILabel mElementLabel;
	public UILabel mLevelLabel;
    public UILabel mAptitudeLabel;
    public UISprite mSkillIcon;
    public UISprite mElementIcon;
    public UISprite mTypeIcon;
    public UISprite mInfoCard;
	public GameObject mInfoCardBG;
	public GameObject mEffectFiveBlue;
	public GameObject mEffectFiveGreen;
	public GameObject mEffectFiveGold;
	public GameObject mEffectFiveRed;
	public GameObject mEffectFiveShadow;
	public GameObject mEffectFour;

    public GameObject mPetObj;

    public float mfCardScale = 1.0f;
    public float mfScaleRatio = 1.0f;

    public int mModelIndex = -1;
    private ITEM_TYPE mItemType = ITEM_TYPE.PET;
    public int mLevel = 0;
    public int mStrengthenLevel = 0;

    public PetAtlasViewCenter ViewCenter;

    public override void Init()
    {
        //mGameObjUI = GameCommon.FindUI("PET_INFO_SINGLE_WINDOW");
        ViewCenter = PetAtlasViewCenter.Instance;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "SET_SELECT_PET_LEVEL")
        {
            SetLevel((int)objVal);
        }
        if (keyIndex == "SET_SELECT_PET_STRENGTHEN_LEVEL")
        {
            SetStrengthenLevel((int)objVal);
        }
        else if (keyIndex == "SET_SELECT_PET_BY_MODEL_INDEX")
        {
            SetPetByModelIndex((int)objVal);
        }
    }

    public void SetModelIndex(int iModelIndex)
    {
        mModelIndex = iModelIndex;
        mItemType = PackageManager.GetItemTypeByTableID(iModelIndex);
    }
    public void SetLevel(int iLevel)
    {
        mLevel = iLevel;
    }
    public void SetStrengthenLevel(int iStrengthenLevel)
    {
        mStrengthenLevel = iStrengthenLevel;
    }

    public void SetPetByModelIndex(int iModelIndex)
    {
        SetModelIndex(iModelIndex);
        SetCardInfo();
    }

    public void SetCardInfo()
    {
		SetCardSkillInfo();
        SetCardElementInfo();
        SetCardTypeInfo();
        //SetCardBG();
		//SetCardStar();
		//SetCardEffect ();
        SetCardText();
        InitActiveBirthForUI();
    }

	public void SetSkillIcon()
	{
		int iSkillIndex = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "PET_SKILL_1");
		DataRecord config = DataCenter.mSkillConfigTable.GetRecord(iSkillIndex);
		if (config != null)
		{
			string iconAtlas = config["SKILL_ATLAS_NAME"];
			string iconName = config["SKILL_SPRITE_NAME"];
			GameObject parent = mSkillIcon.transform.parent.gameObject;
			
			CircularSprite mySprite = new CircularSprite(true);
			mySprite.Init(parent, 0.9f);
			mySprite.SetAtlasTexture(iconAtlas, iconName);
			
			//GameCommon.SetUISprite(parent, "Background", icon);
		}
		else
		{
			DEBUG.LogError("SkillConfig->Icon is null");
		}
	}

    public void SetCardSkillInfo()
    {
        if (null == mSkillIcon)
            return;

        int iSkillIndex = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "PET_SKILL_1");

        // 去技能表里取技能数据
        string strAtlasName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);

        string strSpriteName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_SPRITE_NAME");

        mSkillIcon.atlas = tu;
        mSkillIcon.spriteName = strSpriteName;
        //mSkillIcon.MakePixelPerfect();
    }

    public void SetCardElementInfo()
    {
        if (null == mElementIcon)
            return;

        int iElement = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "ELEMENT_INDEX");

        string strAtlasName = TableCommon.GetStringFromElement(iElement, "ELEMENT_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        string strSpriteName = TableCommon.GetStringFromElement(iElement, "ELEMENT_SPRITE_NAME");

        mElementIcon.atlas = tu;
        mElementIcon.spriteName = strSpriteName;

        mElementLabel.text = TableCommon.GetStringFromElement(iElement, "ELEMENT_NAME");
    }

    public void SetCardTypeInfo()
    {
        if (null == mTypeIcon)
            return;

        string strAtlasName = GameCommon.GetAttackType(mModelIndex, "TPYE_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        string strSpriteName = GameCommon.GetAttackType(mModelIndex, "TYPE_SPRITE_NAME");

        mTypeIcon.atlas = tu;
        mTypeIcon.spriteName = strSpriteName;

        mTypeLabel.text = GameCommon.GetAttackType(mModelIndex);
    }

    // 改为星级判定
    public void SetCardBG()
    {
        if (null == mInfoCard)
            return;

        int iStarLevel = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "STAR_LEVEL");

        // set icon
        string strAtlasName = TableCommon.GetStringFromQualityConfig(iStarLevel, "CARD_BG_ATLAS_NAME_" + mItemType.ToString());
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        string strSpriteName = TableCommon.GetStringFromQualityConfig(iStarLevel, "CARD_BG_SPRITE_NAME_" + mItemType.ToString());
        mInfoCard.atlas = tu;
        mInfoCard.spriteName = strSpriteName;
    }

	public void SetCardStar()
	{
		UIGridContainer curPageGrid = mInfoCardBG.transform.parent.Find("info/StarsGrid").GetComponent<UIGridContainer>();
		curPageGrid.MaxCount = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "STAR_LEVEL");
	}

	public void SetCardEffect()
	{
		mEffectFiveBlue.SetActive (false);
		mEffectFiveGold.SetActive (false);
		mEffectFiveGreen.SetActive (false);
		mEffectFiveRed.SetActive (false);
		mEffectFiveShadow.SetActive (false);
		mEffectFour.SetActive (false);

		if (ITEM_TYPE.CHARACTER == PackageManager.GetItemRealTypeByTableID (mModelIndex))
			return;
		int iStarLevel = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "STAR_LEVEL");
		if(iStarLevel < 4) return;

		int iElementIndex = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "ELEMENT_INDEX");
		if(iStarLevel == 5)
		{
			if(iElementIndex == 0) mEffectFiveRed.SetActive (true);
			else if(iElementIndex == 1) mEffectFiveBlue.SetActive (true);
			else if(iElementIndex == 2) mEffectFiveGreen.SetActive (true);
			else if(iElementIndex == 3) mEffectFiveGold.SetActive (true);
			else if(iElementIndex == 4) mEffectFiveShadow.SetActive (true);
		}
		else mEffectFour.SetActive (true);
	}

    public void SetCardText()
    {
		mLevelLabel.text = mLevel.ToString();
        mNameLabel.text = TableCommon.GetStringFromActiveCongfig(mModelIndex, "NAME");
        mNameLabel.color = GameCommon.GetNameColor(mModelIndex);
        mStrengthenLevelLabel.color = mNameLabel.color;
        if (mStrengthenLevel > 0)
            mStrengthenLevelLabel.text = "+" + mStrengthenLevel.ToString();
        else
            mStrengthenLevelLabel.text = "";

        mAptitudeLabel.text = TableCommon.GetStringFromActiveCongfig(mModelIndex, "APTITUDE_LEVEL");
    }

    public void InitActiveBirthForUI()
    {
        if (mPetObj != null)
        {
            ActiveBirthForUI activeBirthForUI = mPetObj.GetComponent<ActiveBirthForUI>();
            if (activeBirthForUI != null)
            {
                int iChildCount = mPetObj.transform.parent.childCount;
                if (iChildCount > 1)
                {
                    for (int i = 0; i < iChildCount; i++)
                    {
                        GameObject obj = mPetObj.transform.parent.GetChild(i).gameObject;
                        if (obj == mPetObj)
                        {
                            continue;
                        }
                        else
                        {
							activeBirthForUI.OnDestroy();
                        }
                    }
                }
                activeBirthForUI.mBirthConfigIndex = mModelIndex;

				bool bIsCanOperate = (bool)get ("SET_SELECT_PET_OPERATE_STATE");

				object val;		
				bool b = getData("SET_SELECT_PET_COLLIDER_UI", out val);
				GameObject colliderObj = val as GameObject;

				activeBirthForUI.Init(bIsCanOperate, mfScaleRatio * mfCardScale, colliderObj);
                // 设置UI

            }
        }

    }

    public void SetStars(int starNum)
    {
        ViewCenter.PetAtailsDetails.CardInstance.ShowStars(starNum);
    }

}