using UnityEngine;
using System.Collections;

public class levelreset : MonoBehaviour {
	public string levelname;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) 
		{
			Application.LoadLevel(levelname);
				}
	
	}
}
