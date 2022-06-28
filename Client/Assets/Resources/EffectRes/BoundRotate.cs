using UnityEngine;
using System.Collections;

public class BoundRotate : MonoBehaviour {

	// Use this for initialization

	
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3(0, Time.deltaTime*120, 0));
	}
}
