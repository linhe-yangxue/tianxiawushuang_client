using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PetAtlasTableCellInfo : MonoBehaviour
{
    private string shangzhanIcon = "ui_taggoup";
    private string newIcon = "ui_new";
    private string PriseIcon = "ui_tagaward";

    public UISprite KnightIcon;
    public UISprite KnightState;
    public UISprite MaxLevelIcon;
    public Color StateColor;

    public KnightData knightData; //缓存 当前侠客的需要显示到详情窗口上的信息
    public void InitCellInfo(KnightData knightData)
    {     
        //预置物默认颜色  为灰暗
        this.knightData = knightData;
        KnightIcon.spriteName = knightData.ModelIcon;

        if (knightData.ShangZhen)
            KnightState.spriteName = shangzhanIcon;  //上阵

        if (!knightData.Own)
            KnightIcon.GetComponent<UIWidget>().color = StateColor;
        else
            KnightIcon.GetComponent<UIWidget>().color = Color.white;

        //显示 相应的状态
        switch (knightData.TujianStatus)
        {
            case TUJIAN_STATUS.TUJIAN_FULL:
                MaxLevelIcon.gameObject.SetActive(true);
                KnightState.gameObject.SetActive(false);
                break;
            case TUJIAN_STATUS.TUJIAN_NEW:
                MaxLevelIcon.gameObject.SetActive(false);
                KnightState.gameObject.SetActive(true);
                KnightState.spriteName = newIcon;
                break;
            case TUJIAN_STATUS.TUJIAN_NORMAL:
                MaxLevelIcon.gameObject.SetActive(false);
                KnightState.gameObject.SetActive(true);
                KnightState.gameObject.SetActive(false);
                break;
            case TUJIAN_STATUS.TUJIAN_REWARD:
                MaxLevelIcon.gameObject.SetActive(false);
                KnightState.gameObject.SetActive(true);
                KnightState.spriteName = PriseIcon;
                break;
        }
    }


    //领取奖励之后
    public void RewardAfter()
    {

    }
	//查看新的符灵
    public void CheckNewIcon()
    {

    }
    //设置深度
    public void SetDepth(int depth)
    {
        KnightIcon.depth += depth;
        KnightState.depth += depth;
        MaxLevelIcon.depth += depth;
    }
    //显示在侠客缘分中 使用时
    public void UseItInKnightParty()
    {
        GetComponent<UIDragScrollView>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<UIButtonEvent>().enabled = false;
    }
}

