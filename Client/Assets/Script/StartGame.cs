using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//LoadScene ("UI", true);
		//LoadScene ("game_1", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LoadScene(string sceneName, bool bAdd)
	{
		Application.CanStreamedLevelBeLoaded (sceneName);
		Application.LoadLevelAdditiveAsync (sceneName);
	}
}
