/// <summary>
/// Shake camera.
/// Shaking Camera System using adding to Camera with <OrbitGameObject>
/// </summary>

using UnityEngine;
using System.Collections;
using Logic;


public class ShakeCamera : MonoBehaviour
{
    public float delay = 0f;
    public float duration = 1f;
    public float amplitude = 1f;

    private void Start()
    {
        StartCoroutine(WaitForShake());
    }

    private IEnumerator WaitForShake()
    {
        yield return new WaitForSeconds(delay);

        if (Effect_ShakeCamera.Self != null && Effect_ShakeCamera.Self.mCameraObject != null)
        {
            Effect_ShakeCamera.Shake(amplitude, duration);
        }
    }
}


//[RequireComponent(typeof(OrbitGameObject))]
public class Effect_ShakeCamera : BaseEffect
{
	public static Effect_ShakeCamera Self;

	public static Effect_ShakeCamera Shake(float magnitude,float duration)
	{
        if (MainProcess.mCameraMoveTool == null || !MainProcess.mCameraMoveTool.mShakeEnabled)
        {
            return null;
        }
        //by chenliang
        //begin

        if (Self == null)
            return null;

        //end

		Self.Finish ();
		Self.Magnitude	= magnitude;
		Self.Duration	= duration;
		Self.SetFinished(false);
		Self.DoEvent();
		return Self;
	}

	static public void InitCamera(GameObject cameraObj)
	{
		Self = Logic.EventCenter.Self.StartEvent("Effect_ShakeCamera") as Effect_ShakeCamera;
		if (Self!=null)
		{
			Self.mCameraObject = cameraObj;
			Self.mCameraPos = Self.mCameraObject.transform.localPosition;
		}
	}

	public GameObject mCameraObject;
	public float Magnitude	= 1;
	public float Duration	= 1;
	public Vector3 mCameraPos;

	public override bool _DoEvent()
	{
		mCameraPos = mCameraObject.transform.localPosition;
		StartUpdate();

		return true;
	}

	// Update is called once per frame
	public override bool Update(float dT)
	{
		Duration	-= dT;
		if(Duration < 0)
		{
			Finish();
			return false;
		}
		//Shaking Camera
		if(mCameraObject!=null)
		{
			Vector3 p = mCameraObject.transform.localPosition;
			p.y	= mCameraPos.y + Mathf.Sin(100 * Time.time) * Duration * Magnitude;
			mCameraObject.transform.localPosition = p;
		}

		return true;
	}

	public override bool _OnFinish()
	{
		if(mCameraObject != null)
			mCameraObject.transform.localPosition = mCameraPos;

		return true;
	}


}
