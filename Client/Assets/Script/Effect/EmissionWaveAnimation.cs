using UnityEngine;
using System.Collections;

public class EmissionWaveAnimation : MonoBehaviour
{
    public float mMinLight = 0.5f;
    public float mMaxLight = 1;
    public float mOnceTime = 1;

    public float mNowValue = 1;

    WaveAnimation mWaveAnimation = new WaveAnimation();
    Material mCurrentMat;

	// Use this for initialization
	void Start () {
        mCurrentMat = GetComponent<Renderer>().material;
		mWaveAnimation.Start(0, 99999999, mOnceTime*0.5f, mMaxLight-mMinLight);
    }
    
	// Update is called once per frame
	void Update () 
    {
		mWaveAnimation.Update(Time.deltaTime);
        float a = mMinLight + System.Math.Abs(mWaveAnimation.NowValue());
        mNowValue = a;
        Color col = new Color(a, a, a, 1);
        mCurrentMat.SetColor("_Emission", col);

        //Light l = GetComponentInChildren<Light>();
        //l.intensity = 1 + a;
	}
}
