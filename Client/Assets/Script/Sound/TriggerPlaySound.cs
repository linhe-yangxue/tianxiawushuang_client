using UnityEngine;
using System.Collections;

public class TriggerPlaySound : MonoBehaviour
{
    public float mTiggerBound = 4;
    public AudioClip mSoundClip;
	// Use this for initialization
    void Start () {
        if (mSoundClip == null)
        {
            Destroy(gameObject);
            return;
        }
        Renderer r = gameObject.GetComponent<Renderer>(); 
        if (r!=null)
            r.enabled = false;
    }
	
	// Update is called once per frame
    void Update()
    {
        if (Character.Self != null && Vector3.Distance(Character.Self.GetPosition(), transform.position) < mTiggerBound)
        {

            DEBUG.Log("+! play trigger sound >" + mSoundClip.name);
            AudioSource.PlayClipAtPoint(mSoundClip, GameCommon.GetMainCamera().transform.position, CommonParam.DefaultVolume);

            Destroy(gameObject);
        }
    }
}
