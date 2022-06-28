using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class KnightDesInfo : MonoBehaviour
{
    public UILabel mPetStarLevelLabel;
    public UILabel mPetTitleLabel;
    public UILabel mPetNameLabel;
    public UILabel mDescriptionLabel;
  
    public UILabel mAttackLabel;
    public UILabel mMaxHPLabel;

    public GameObject mCard;
    public float mfCardScale = 1.0f;

    public UILabel mElementNameLabel;
    public UISprite mElementIcon;
    public UISprite MaxlevelIcon;

    public GameObject mSkillBtn1;
    public GameObject mSkillBtn2;
    public GameObject mSkillBtn3;

    public int mModelIndex = -1;
    public int mLevel = 30;
    public int mStrengthenLevel = 0;

}


[System.Serializable]
public class StarModel
{
    public UISprite StarTexture;
    private TweenScale scale;

    public void Play()
    {
        if (scale == null)
        {
            scale = StarTexture.GetComponent<TweenScale>();
        }

        StarTexture.gameObject.SetActive(true);
        scale.duration = 0.28f;
        scale.method = UITweener.Method.BounceIn;
        scale.PlayForward();

    }

    public void ReSetState()
    {
        if (scale == null) scale = StarTexture.GetComponent<TweenScale>();
        
        StarTexture.gameObject.SetActive(false);
        scale.ResetToBeginning();
    }

    public void ResetAndPlay()
    {
        ReSetState();
        Play();
    }

    public void Disable()
    {
        StarTexture.gameObject.SetActive(false);
    }
}