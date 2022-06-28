using UnityEngine;
using System.Collections;

public class PlayBgSound : MonoBehaviour {

    public string BgMusicName;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnEnable()
    {
        string path = "Sound/" + BgMusicName;
        GameCommon.SetBackgroundSound(path, 0.7f);

    }
}
