using UnityEngine;
using System.Collections;

public class PetAtailsDetailsPanel : MonoBehaviour
{
    public KnightDesInfo knightDesInfo;
    public CardGroupUI CardInstance;
    public KnightDesInfo RightDataInfo;
    public DetailsTip DetailsTip;

    public UISprite BackBlack;
    public UISprite MaxLevelIcon;

    public int CurrPetId = -1;
    public KnightData knightData;
    private TweenScale scale;

	public GameObject mRoleMastButtonUI;

    private void OnEnable()
    {
        if (GetComponent<TweenScale>() == null)
            gameObject.AddComponent<TweenScale>();
        if (scale == null)
            scale = GetComponent<TweenScale>();
        InitRegister();
        OpenWindow();
    }

    
    public void OpenWindow()
    {
        scale.duration = 0.58f;
        scale.method = UITweener.Method.BounceIn;
        EventDelegate.Add(scale.onFinished, OpenFinish);
        scale.PlayForward();
    }

    private void OpenFinish()
    {
        BackBlack.gameObject.SetActive(true);
        CardInstance.ShowStars(knightData.StarLevel);   // 6星 写死   可以放别的地方显示
        scale.RemoveOnFinished(new EventDelegate(OpenFinish));
    }

    public void CloseWindow()
    {
        BackBlack.gameObject.SetActive(false);
        scale.duration = 0.4f;
        scale.method = UITweener.Method.BounceIn;
        EventDelegate.Add(scale.onFinished, CloseFinish);
        scale.PlayReverse();
    }

    private void CloseFinish()
    {
        ReSetDetailsPanel();
        this.gameObject.SetActive(false);
        scale.RemoveOnFinished(new EventDelegate(CloseFinish));
    }


    
    //注册当前的窗口对象
    void InitRegister()
    {
        InitCard();

        if (DataCenter.Self.getData("PetAtailsDetailsWindow") == null)
        {
            PetAtailsDetailsWindow detailsWindow = new PetAtailsDetailsWindow();
            DataCenter.Self.set("PetAtailsDetailsWindow", detailsWindow);
            detailsWindow.DesModel = knightDesInfo;
            detailsWindow.SetPetByModelIndex(CurrPetId != -1 ? CurrPetId : 1000);
        }
        else
        {
            PetAtailsDetailsWindow detailsWindow = DataCenter.Self.getData("PetAtailsDetailsWindow") as PetAtailsDetailsWindow;
            detailsWindow.DesModel = knightDesInfo;
            detailsWindow.SetPetByModelIndex(CurrPetId);
			detailsWindow.mRoleMastButtonUI = mRoleMastButtonUI;
        }

        if (knightData.ButtonState == KnightDetailsButtonState.GetMaxPrice)
            DetailsTip.ShowButton();
        else if (knightData.ButtonState == KnightDetailsButtonState.GetTip)
            DetailsTip.ShowTip("");

        MaxLevelIcon.gameObject.SetActive(knightData.TujianStatus == TUJIAN_STATUS.TUJIAN_FULL);
    }

    private void InitCard()
    {
        GameObject obj = CardInstance.gameObject;
        if (obj != null)
        {
            CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
            uiScript.InitPetInfo(knightDesInfo.mCard.name, knightDesInfo.mfCardScale, gameObject);
        }
    }
  


    //重置详情面板的内容
    private void ReSetDetailsPanel()
    {
        CardInstance.ResetCardToBegin();
    }

}


[System.Serializable]
public class DetailsTip
{
    public UIButton MaxLevelPrise;

    public UILabel tipContent;
    public UISprite backTexture;

    public void ShowButton()
    {
        MaxLevelPrise.gameObject.SetActive(true);
    }

    public void ShowTip(string tip)
    {
        tipContent.gameObject.SetActive(true);
        backTexture.gameObject.SetActive(true);
        MaxLevelPrise.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(tip))
            tipContent.text = "获取的途径。。。。。";
        else
            tipContent.text = tip;
    }
}