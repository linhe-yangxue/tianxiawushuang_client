using UnityEngine;
using System.Collections;

public enum PAOTEXT_TYPE
{
    WHITE,
    RED,
    YELLOW,
    BLUE,
	GREEN,
	GOLD,
    DODGE,
    RESIST,
}

//---------------------------------------------------------------------------------------
public class UI_PaoPaoText : BaseEffect
{
    static public GameObject textPanel = null;
    static public float nomeaning = 0.25f;
    //static public float msSizeTime = 0.25f;

	public float mfSizeRatio = 1.0f;
    static public float startsize = 0f;
    static public float endsize = 1f;
    public float sizechangetime = 1f;  
	public GameObject mTextObject;
	SubSpeedMove mMoveValue;
    AddSpeedMove mbackMoveValue;
    //WaveAnimation mWaveSizeVaule = new WaveAnimation();
    ValueAnimation mValueSizeVaule = new ValueAnimation();
    public PAOTEXT_TYPE mTextType;
    private UILabel mLabel;
    BaseObject mparentobj;
    Vector3 pos;
    float mtime=1f;
    bool canfall = false;
    bool arrivelowest = false;
    bool arrivehighest = false;
	float _timeCount = 0;

    public static GameObject GetTextPanel()
    {
        if (textPanel == null)
        {
            GameObject centerAnchor = GameCommon.FindUI("Camera");

            if (centerAnchor == null)
                return null;

            textPanel = new GameObject("text_panel");
            textPanel.transform.parent = centerAnchor.transform;
            textPanel.transform.localPosition = Vector3.zero;
            textPanel.transform.localScale = Vector3.one * 0.5f;
            textPanel.transform.localRotation = Quaternion.identity;
        }

        if(!textPanel.activeSelf)
            textPanel.SetActive(true);

        return textPanel;
    }

    public void InitText(PAOTEXT_TYPE textType)
    {
        mTextType = textType;
        GameObject textUI = new GameObject("show_demage");
        textUI.layer = CommonParam.UILayer;

        //if (textPanel == null)
        //{
        //    GameObject centerAnchor = GameCommon.FindUI("Camera");

        //    if (centerAnchor == null)
        //        return;

        //    textPanel = new GameObject("text_panel");
        //    textPanel.transform.parent = centerAnchor.transform;
        //    textPanel.transform.localPosition = Vector3.zero;
        //    textPanel.transform.localScale = Vector3.one * 0.4f;
        //    textPanel.transform.localRotation = Quaternion.identity;
        //}

        //textPanel.SetActive(true);
        var panel = GetTextPanel();

        if (panel == null)
            return;
        //GameObject plane = GameCommon.FindUI("text_panel");
        //if (plane == null)
        //{
        //    GameObject centerAnchor = GameCommon.FindUI("Camera");
        //    if (centerAnchor == null)
        //        return;

        //    plane = new GameObject("text_panel");
        //    plane.transform.parent = centerAnchor.transform;
        //    plane.transform.localPosition = Vector3.zero;
        //    plane.transform.localScale = Vector3.one * 0.4f;
        //    plane.transform.localRotation = Quaternion.identity;
        //}
        textUI.transform.parent = panel.transform;
        textUI.transform.localPosition = Vector3.zero;
        textUI.transform.localScale = Vector3.one;
        textUI.transform.localRotation = Quaternion.identity;
        mLabel = textUI.AddComponent<UILabel>();
        mLabel.text = "";
        mLabel.width = 300;
        mLabel.height = 50;
        mLabel.fontSize = 32;
		mLabel.pivot = UIWidget.Pivot.Left;
        mLabel.overflowMethod = UILabel.Overflow.ClampContent;
        mfSizeRatio = 1.0f;
        //label.trueTypeFont = Resources.Load<Font>("Fonts/font");

        string fontRes = "Fonts/Number";
        switch (textType)
        {
            case PAOTEXT_TYPE.WHITE:
                fontRes = "Fonts/Number_white";
                break;

            case PAOTEXT_TYPE.RED:
                fontRes = "Fonts/Number";
                break;

            case PAOTEXT_TYPE.YELLOW:
                fontRes = "Fonts/Number_yellow";
                mfSizeRatio = 1.0f;
				InitBackground (mLabel);
                break;

            case PAOTEXT_TYPE.BLUE:
                fontRes = "Fonts/Number_blue";
                break;

            case PAOTEXT_TYPE.GREEN:
                fontRes = "Fonts/Number_green";
                break;

            case PAOTEXT_TYPE.GOLD:
                fontRes = "Fonts/Number_gold";
                break;

            case PAOTEXT_TYPE.DODGE:
                fontRes = "Fonts/Shan";
                break;

            case PAOTEXT_TYPE.RESIST:
                fontRes = "Fonts/Shan";
                break;
        }

        GameObject font = GameCommon.LoadPrefabs(fontRes); // GameCommon.mResources.LoadPrefab(fontRes, "");//
        if (font != null)
        {
            mLabel.bitmapFont = font.GetComponent<UIFont>();
        }

        mTextObject = textUI;
        mTextObject.SetActive(false);

        //mWaveSizeVaule.Start(0, msSizeTime, msSizeTime * 0.5f, 0.2f * mfSizeRatio);

        //Vector3 pos = parentobject.GetPosition();
        //pos.y += 2.0f;
        //pos = MainProcess.WorldToUIPosition(pos);

        //mTextObject.transform.position = pos;

        //StartUpdate();
        //Vector3 endpos = pos;
        //endpos.y += 0.5f;
        //mMoveValue = new SubSpeedMove();
        //mMoveValue.SetPos(pos, endpos, 1.0f);
    }

    public void Start(BaseObject parentobject, string showText)
    {
      //  GameObject.Find("UI ROOT").GetComponent<UIPanel>().depth = 5;
     /*   GameObject[] mcamera;
        mcamera = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (GameObject a in mcamera)
        {
            if (a.name == "Camera")
            {
                DEBUG.Log(1);
                a.GetComponent<Camera>().cullingMask = ~(1 << 31);
            }
        }
      */
        mparentobj = parentobject;
        mLabel.text = showText;

        switch (mTextType)
        {
            case PAOTEXT_TYPE.YELLOW:
                mLabel.text = showText.Replace("-", "#");
                break;

            case PAOTEXT_TYPE.GOLD:
                mLabel.text = showText + "#";
                break;

            case PAOTEXT_TYPE.DODGE:
                mLabel.text = "#";
                break;

            case PAOTEXT_TYPE.RESIST:
                mLabel.text = "$";
                break;
        }

        mTextObject.SetActive(true);
      //  mWaveSizeVaule.Start(0, msSizeTime, msSizeTime * 0.5f, 0.2f * mfSizeRatio);
       
        pos = parentobject.GetPosition();
       // pos.y += 100.0f;
        //pos = MainProcess.WorldToUIPosition(pos);
        //pos.y += 2f;
        //pos = MainProcess.UIToWorldPosition(pos);
        mTextObject.transform.position = pos;
       // mTextObject.transform.eulerAngles = new Vector3(45, 0, 0);
       
        mFinish = false;
        StartUpdate();
        Vector3 endpos = pos;
		float iRandom = 0;
        if (mTextType == PAOTEXT_TYPE.YELLOW || mTextType == PAOTEXT_TYPE.BLUE)
        {
            endpos.y += 1.3f;
        }
        else 
        {
            endpos.y += 1.5f; 
			iRandom =  Random.Range (-0.5f, 0.5f);
			endpos.x += iRandom;
        }        
        mMoveValue = new SubSpeedMove();
        mbackMoveValue = new AddSpeedMove();
        sizechangetime=mMoveValue.SetPos(pos, endpos, 20f);
        //DEBUG.Log(sizechangetime.ToString()+"上升时间");
        mValueSizeVaule.Start(startsize, endsize, sizechangetime);  //
        Vector3 halfpos;
        halfpos = pos;
        halfpos.y += 0.05f;
		halfpos.x += iRandom;
        mbackMoveValue.SetPos(endpos,halfpos,4f);//
      mLabel.alpha = 1f;
    }

    public override void Finish()
    {
        base.Finish();
        mTextObject.SetActive(false);
    }

    public void Destroy()
    {
        base.Finish();
        GameObject.Destroy(mTextObject);
    }

	public void InitBackground(UILabel  labelObj)
	{
		UISprite s = NGUITools.AddChild<UISprite>(labelObj.gameObject);
		s.transform.localPosition = new Vector3(15f, 18f, 0f);
		s.atlas = GameCommon.LoadUIAtlas ("CommonUIAtlas");
		s.spriteName = "ui_baojibiaozhi";
		s.type = UISprite.Type.Simple;
//		s.depth = labelObj.depth - 1;
		s.MakePixelPerfect ();
	}

    /*
    public void Start(BaseObject parentobject, string showText, PAOTEXT_TYPE textType)
	{        
        GameObject textUI = new GameObject("show_demage");
        textUI.layer = CommonParam.UILayer;
        GameObject plane = GameCommon.FindUI("text_panel");
		if (plane==null)
		{
            GameObject centerAnchor = GameCommon.FindUI("Camera");
			if (centerAnchor==null)
				return;

			plane = new GameObject("text_panel");
			plane.transform.parent = centerAnchor.transform;
			plane.transform.localPosition = Vector3.zero;
			plane.transform.localScale = Vector3.one * 0.4f;
			plane.transform.localRotation = Quaternion.identity;
		}
        textUI.transform.parent = plane.transform;
        textUI.transform.localPosition = Vector3.zero;
        textUI.transform.localScale = Vector3.one;
        textUI.transform.localRotation = Quaternion.identity;
        UILabel label = textUI.AddComponent<UILabel>();
        label.text = showText;
        label.width = 300;
        label.height = 50;
		label.fontSize = 32;
		label.overflowMethod = UILabel.Overflow.ClampContent;
		mfSizeRatio = 1.0f;
        //label.trueTypeFont = Resources.Load<Font>("Fonts/font");

        string fontRes = "Fonts/Number";
        switch (textType)
        {
            case PAOTEXT_TYPE.WHITE:
                fontRes = "Fonts/Number_white";
                break;

            case PAOTEXT_TYPE.RED:
                fontRes = "Fonts/Number";
                break;

            case PAOTEXT_TYPE.YELLOW:
				label.text = showText.Replace("-", "#");
                fontRes = "Fonts/Number_yellow";
				mfSizeRatio = 2.0f;
                break;

            case PAOTEXT_TYPE.BLUE:
                fontRes = "Fonts/Number_blue";
                break;

			case PAOTEXT_TYPE.GREEN:
				fontRes = "Fonts/Number_green";
				break;

			case PAOTEXT_TYPE.GOLD:
				fontRes = "Fonts/Number_gold";
				label.text = showText + "#";
				break;
            case PAOTEXT_TYPE.DODGE:
                fontRes = "Fonts/Shan";
                label.text = "#";
                break;
            case PAOTEXT_TYPE.RESIST:
                fontRes = "Fonts/Shan";
                label.text = "$";
                break;
		}

        GameObject font = GameCommon.mResources.LoadPrefab(fontRes, "");//GameCommon.LoadPrefabs(fontRes);
        if (font != null)
        {
            label.bitmapFont = font.GetComponent<UIFont>();
        }

        mTextObject = textUI;

		mWaveSizeVaule.Start(0, msSizeTime, msSizeTime*0.5f, 0.2f * mfSizeRatio);

		Vector3 pos = parentobject.GetPosition();
		pos.y += 2.0f;
        pos = MainProcess.WorldToUIPosition(pos);

        mTextObject.transform.position = pos;

		StartUpdate();
		Vector3 endpos = pos;
		endpos.y += 0.5f;
		mMoveValue = new SubSpeedMove();
		mMoveValue.SetPos(pos, endpos, 1.0f);
	}*/

 
	public override bool Update(float d)
	{
        float size = 0;
        size *= mfSizeRatio;
		Vector3 nowPos;        
        arrivehighest=mMoveValue.Update(d, out nowPos);
       
       // mLabel.alpha += Time.deltaTime / sizechangetime;
        if (arrivehighest)
		{
            if (mTextType == PAOTEXT_TYPE.YELLOW || mTextType == PAOTEXT_TYPE.BLUE)
            {
                mtime -= Time.deltaTime*3;  //123321 *3=1/3秒
                if (mtime <= 0)
                {
                    mtime = 1;
                    canfall = true;
                    arrivehighest = false;
                }
            }
            else
            {
                canfall = true;
                arrivehighest = false;
            }
		}
        if (canfall)
        {
            arrivelowest=mbackMoveValue.Update(d, out nowPos);
            // add by LC
            // 屏蔽伤害字体的淡出效果
            //mLabel.alpha -= Time.deltaTime * 3f;
            //DEBUG.Log(Time.deltaTime.ToString()+"time.deltatime时间");
            if (size >= 0)
            {
                size -= Time.deltaTime * 1f;
            }           
            if (arrivelowest)
            {
                canfall = false;
                PaoPaoTextPool.Disable(this);               
                arrivelowest = false;
                return false;
                
            }
			_timeCount = _timeCount+ Time.deltaTime;
			if(_timeCount >= sizechangetime)
			{
				mLabel.alpha -= Time.deltaTime * 8f;
				if(mLabel.alpha <= 0.2f)
					_timeCount = 0;
			}
            //mtime = 1;
            //mLabel.alpha -= Time.deltaTime*2;                     
            //mtime -= Time.deltaTime*2;
            //if (mtime <= 0.001f)
            //{
            //    mtime = 1;
            //    PaoPaoTextPool.Disable(this);
            //    return false;
            //}
        }

        if (mTextObject != null)
        {
           // mWaveSizeVaule.Update(d);
            mValueSizeVaule.Update(d,out size);
            //nowPos.z += 0.5f;
           
            nowPos = nowPos + mparentobj.GetPosition() - pos;
            nowPos = MainProcess.WorldToUIPosition(nowPos);
            nowPos.y += 0.25f;
            mTextObject.transform.position = nowPos;
            //float size = msMinSize + mWaveSizeVaule.NowValue();
            //float size = mValueSizeVaule.NowValue();
           
			//size *= 0.75f;
           
            mTextObject.transform.localScale = new Vector3(size, size, 1);
        }
		return true;

		
	}
}
//---------------------------------------------------------------------------------------