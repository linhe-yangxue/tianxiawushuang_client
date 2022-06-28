using UnityEngine;
using System.Collections;

public class EffectMove : MonoBehaviour {

    public VectorAnimation mMovePath;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (mMovePath != null)
        {            
            mMovePath.Update(Time.deltaTime);
            transform.position = mMovePath.NowValue();
        }
	}
}
