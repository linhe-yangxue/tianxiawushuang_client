using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------------------
public class Effect_MoveText : BaseEffect
{
    static public float msMinSize = 0.25f;
    static public float msSizeTime = 0.25f;
	public GameObject mTextObject;
	SubSpeedMove mMoveValue;
    WaveAnimation mWaveSizeVaule = new WaveAnimation();
	
	public void Start(BaseObject parentobject, string showText, Color co)
	{
		//base.InitCreate(parentobject, null, 100);

        //mTextObject = new GameObject("DamageHp");
        //Material m = new Material(GameCommon.FindShader("GUI/Text Shader"));
        //m.color = co;
        //m.mainTexture = ResourcesManger.FindTexture("Font Texture");
		
        //mTextObject.AddComponent<MeshRenderer>().material = m;
        //TextMesh textMesh = mTextObject.AddComponent<TextMesh>();
        //textMesh.text = showText;
        //textMesh.font = Resources.Load("Arial", typeof(Font)) as Font;
        //textMesh.fontSize = 16;
        mTextObject = GameCommon.LoadAndIntanciatePrefabs("Prefabs/3DText");

        TextMesh textMesh = mTextObject.GetComponent<TextMesh>();
        textMesh.text = showText;

		MeshRenderer r = mTextObject.GetComponent<MeshRenderer>();
		Material m = new Material(r.material);
		m.color = co;
		r.material = m;

        mWaveSizeVaule.Start(0, msSizeTime, msSizeTime*0.5f, 0.2f);
		//Vector3 pos = parentobject.GetPosition();
		//pos.y += 2;

		Vector3 pos = parentobject.GetTopPosition(); //mShowPosition.Update(mOwner, null);

		mTextObject.transform.position = pos;
		mTextObject.transform.rotation =  Quaternion.Euler(48.1f, 0, 0);
		mTextObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
		StartUpdate();
		Vector3 endpos = pos;
		endpos.y += 3;
		mMoveValue = new SubSpeedMove();
		mMoveValue.SetPos(pos, endpos, 6.0f);
	}
	
	public override bool Update(float d)
	{
		Vector3 nowPos;
		if (mMoveValue.Update(d, out nowPos))
		{
			GameObject.Destroy(mTextObject);
			return false;
		}
        if (mTextObject != null)
        {
            mWaveSizeVaule.Update(d);
            mTextObject.transform.position = nowPos;
            mTextObject.transform.localScale = new Vector3(msMinSize + mWaveSizeVaule.NowValue(), msMinSize + mWaveSizeVaule.NowValue(), 1);
        }
		return true;

		
	}
}
//---------------------------------------------------------------------------------------