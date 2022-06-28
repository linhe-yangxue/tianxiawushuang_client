using System.Collections.Generic;
using Logic;
using UnityEngine;
using System.Collections;


public class KnightOfFateEleData : PetAtlasBehaviourBase
{
    private string btnNormal = "ui_knightgo_normal";
    private string btnDown = "ui_knightgo_down";
    private string nosomething = "ui_nosomething";
    private string activationtag = "ui_activationtag";

    public UILabel EffectTitle;
    public UILabel EffectContext1;
    public UILabel EffectContext2;
    public UILabel EffectContext3;

    public UIImageButton Equite;
    public UISprite EquitBtnSprite;

    public UIScrollView ScrollView;
    public Transform UIGrid;
    public PageScrollView PageScroll;  //for paging
    public UISprite ActiveIcon;
    public PetAtlasTableCellInfo SpriPetFab;
    //缓存侠客缘分数据  为上阵  为激活
    public KnightPartyInfo KnightPartyData;
    
    public void Init(KnightPartyInfo data)
    {
        if (SpriPetFab == null)
        {
            SpriPetFab = (PetAtlasTableCellInfo)Resources.Load("Prefabs/UI/PetAtlasfab/SubCell");
        }      
        this.KnightPartyData = data;
        EffectTitle.text = data.AddtiveTitle;
        EffectContext1.text = data.AddtiveDes;

        //初始化组队效果 
        CreateKnightIcon(data.KnightIdGroups);

      
        switch (data.PartyState)
        {
            case KnightPartyState.ActivationTag:
                ShowActivedState();
                break;
            case KnightPartyState.CanActive:
                ShowCanActiveState();
                break;
            case KnightPartyState.NoSomething:
                ShowNotSatisfiedState();
                break;
        }

        SpriPetFab = null;
        Resources.UnloadUnusedAssets();
    }

    //创建侠客缘分预置物
    private void CreateKnightIcon(List<KnightData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            PetAtlasTableCellInfo Knight = (PetAtlasTableCellInfo)Instantiate(SpriPetFab);
            Knight.name = "KnightClone";
            Knight.transform.parent = UIGrid;
            Knight.transform.localPosition = Vector3.zero;
            Knight.transform.localScale = Vector3.one;
            Knight.UseItInKnightParty();
            Knight.InitCellInfo(data[i]);          
        }
        ScrollView.ResetPosition();
    }

    //显示可以激活状态
    private void ShowCanActiveState()
    {
        Equite.GetComponent<BoxCollider>().enabled = true;
        Equite.enabled = true;
        EquitBtnSprite.enabled = !Equite.enabled;
        //EquitBtnSprite.spriteName = btnNormal;
        ActiveIcon.gameObject.SetActive(false);
    }
    //显示已经激活的状态
    private void ShowActivedState()
    {
        Equite.GetComponent<BoxCollider>().enabled = false;
        Equite.enabled = false;

        EquitBtnSprite.enabled = !Equite.enabled;
        EquitBtnSprite.spriteName = activationtag;
        ActiveIcon.gameObject.SetActive(true);
    }
    //显示不满足缘分条件状态
    private void ShowNotSatisfiedState()
    {
        Equite.enabled = false;
        EquitBtnSprite.enabled = !Equite.enabled;
        Equite.GetComponent<BoxCollider>().enabled = false;       
        EquitBtnSprite.spriteName = nosomething;
        ActiveIcon.gameObject.SetActive(false);
    }

}


public class KnightPartyEvent : CEvent
{
    public override bool _DoEvent()
    {
        object button;
        if (getData("BUTTON", out button))
        {
            KnightOfFateEleData knightParty = NGUITools.FindInParents<KnightOfFateEleData>(((GameObject) button));
            if (knightParty!=null)
            {
                //knightParty.KnightPartyData   //记录上阵侠客的petId
            }
        }

        //viewCenter.mGameObjUI.GetComponent<PetAtlasViewCenter>();
        return true;
    }
}





