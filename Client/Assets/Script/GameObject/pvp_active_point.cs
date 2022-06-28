using UnityEngine;
using System.Collections;

public class pvp_active_point : MonoBehaviour 
{
	public int mIndex = 0;

    private void Awake()
    {
        if (MainProcess.mStage != null)
            MainProcess.mStage.RegisterBirthPoint(this);
    }

	private void Start () 
	{
		//string key = "POINT_"+mIndex.ToString();
		//DataCenter.Set(key, transform.position);

        if (MainProcess.mStage != null)
            MainProcess.mStage.InitSetBirthPoint(this);
        else
        {
            DEBUG.LogError("MainProcess.mStage is null when pvp_active_point.Start()");
        }

        GameObject.Destroy(gameObject);
	}

}
