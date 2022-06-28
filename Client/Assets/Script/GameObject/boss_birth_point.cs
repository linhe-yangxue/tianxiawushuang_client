using UnityEngine;
using System.Collections;

public class boss_birth_point : MonoBehaviour {

	// Use this for initialization
	void Awake () 
    {
        DataCenter.Set("BOSS_POSITION_X", transform.position.x);
        DataCenter.Set("BOSS_POSITION_Z", transform.position.z);
        GameObject.Destroy(gameObject);
	}
	

}
