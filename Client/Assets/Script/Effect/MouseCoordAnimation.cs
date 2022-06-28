using UnityEngine;
using System.Collections;
using System;

public class MouseCoordAnimation : MonoBehaviour 
{

    static float msWaveTime = 0.5f;
	float mWaveHeight = 1.5f;
    float mMinSize = 1.3f;
	WaveAnimation	mWaveValue;

	Projector mProjector;
	// Use this for initialization
	void Start () {
		mWaveValue = new WaveAnimation();
        mWaveValue.Start(msWaveTime*0.5f, 100000000, msWaveTime, mWaveHeight);	

		mProjector = gameObject.GetComponent<Projector>();

        //mWaveHeight = 0.2f; // mProjector.orthographicSize - mMinSize;
	}
	
	// Update is called once per frame
	void Update () {
		if (mWaveValue.Update(Time.deltaTime))
            mWaveValue.Start(msWaveTime*0.5f, 100000000, msWaveTime, mWaveHeight);

        //mProjector.orthographicSize = mMinSize + Math.Abs(mWaveValue.NowValue());

        float scale = mMinSize + Math.Abs(mWaveValue.NowValue());
        transform.localScale = new Vector3(scale, scale, scale);

        //gameObject.transform.Rotate(Vector3.up, Time.deltaTime*2);
	}


}
