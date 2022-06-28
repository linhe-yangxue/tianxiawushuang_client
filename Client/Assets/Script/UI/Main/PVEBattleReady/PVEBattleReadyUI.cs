using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PVEBattleReadyUI : MonoBehaviour {

	public GameObject mPlayer;

	public float mfScaleRatio = 1.0f;
	// Use this for initialization
	void Start () {
		mfScaleRatio = 1.5f;
        DataCenter.Self.registerData("PVEBattleReadyWindow", new PVEBattleReadyWindow(gameObject));

        DataCenter.SetData("PVEBattleReadyWindow", "OPEN", true);

		InitActiveBirthForUI();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy()
	{
        DataCenter.Remove("PVEBattleReadyWindow");
	}

	public void InitActiveBirthForUI()
	{
		if(mPlayer != null)
		{
			ActiveBirthForUI activeBirthForUI = mPlayer.GetComponent<ActiveBirthForUI>();
			if (activeBirthForUI != null)
			{
				if (activeBirthForUI.mIndex == 0)
				{
					RoleLogicData logicData = RoleLogicData.Self;
					activeBirthForUI.mBirthConfigIndex = RoleLogicData.GetMainRole().tid;
				}
				activeBirthForUI.Init(false, mfScaleRatio);
			}
		}

	}
}

public class PVEBattleReadyWindow : tWindow
{
	public PVEBattleReadyWindow(GameObject obj)
	{
		mGameObjUI = obj;

		//Open (true);
	}
	public override void Open(object param)
	{
		base.Open(param);
		Refresh(param);
        CloseAllWindow();

	}
	
	public override void Close()
	{

	}

    public void CloseAllWindow()
	{

	}

    public override bool Refresh(object param)
    {
        int levelIndex = DataCenter.Get("CURRENT_STAGE");

        string name = TableManager.GetData("Stage", levelIndex, "NAME");

        GameCommon.SetUIText(mGameObjUI, "StageNameLabel", name);

        for (int i = 1; i < 4; i++)
        {
            string index = TableManager.GetData("Stage", levelIndex, "AWARD_" + i.ToString());
            string headspritename = TableManager.GetData("ActiveObject", Convert.ToInt32(index), "HEAD_SPRITE_NAME");
            GameCommon.SetUISprite(mGameObjUI, "Demotest" + i.ToString(), headspritename);
        }
        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        for(int i = 1;i<4;i++)
        {
            GameObject objskill = GameCommon.FindObject(mGameObjUI, "SkillSmall" + i.ToString());
            SetButtonTexture(mGameObjUI, "Demotest" + i.ToString() + "_Button", "");
            SetSkillInfo(objskill, 1, null);

			foreach(KeyValuePair<int, PetData> iter in petLogicData.mDicPetData)
            {
				if(petLogicData.GetPosInTeam(0, iter.Key) == i)
                {
					string headspritename = TableManager.GetData("ActiveObject",iter.Value.tid , "HEAD_SPRITE_NAME");
                    SetButtonTexture(mGameObjUI, "Demotest" + i.ToString() + "_Button", headspritename);
					SetSkillInfo(objskill, 1, iter.Value);
                    continue;
                }
            }
        }
        return true;
    }

    public void SetSkillInfo(GameObject objBtn, int iIndex, PetData mPetData)
    {
        GameObject obj = GameCommon.FindObject(objBtn, "Background");
        if (obj == null)
            return;

        UISprite sprite = obj.GetComponent<UISprite>();

        if (sprite != null)
        {
            if (mPetData == null)
            {
                sprite.spriteName = "";
                return;
            }
            int iSkillIndex = mPetData.GetSkillIndexByIndex(iIndex);
            //int iSkillIndex = TableCommon.GetNumberFromActiveCongfig(mPetData.mModelIndex, "PET_SKILL_" + iIndex.ToString());

            string strAtlasName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);

            string strSpriteName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_SPRITE_NAME");

            sprite.atlas = tu;
            sprite.spriteName = strSpriteName;
            sprite.MakePixelPerfect();
        }
    }

    void SetButtonTexture(GameObject winObj, string SpriteOwnerName, string argSpriteName)
    {
        GameObject obj = GameCommon.FindObject(winObj, SpriteOwnerName);
        if (obj != null)
        {
            UIButton sprite = obj.GetComponentInChildren<UIButton>();
            if (sprite != null)
            {
                sprite.tweenTarget.GetComponent<UISprite>().spriteName = argSpriteName;
            }
        }
    }

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		if(keyIndex == "CLOSEALL")
		{
            CloseAllWindow();
		}
	}
}