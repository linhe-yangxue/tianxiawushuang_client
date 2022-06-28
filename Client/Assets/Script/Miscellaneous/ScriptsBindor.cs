using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptsBindor : MonoBehaviour {

    public List<string> allScripts;

	// Use this for initialization
	void Awake () {
	    foreach(var scipt in allScripts)
        {
            gameObject.AddComponent(scipt);
        }
	}
}
