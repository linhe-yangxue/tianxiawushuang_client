using UnityEngine;
using System.Collections;

public class delaydestroy : MonoBehaviour {

	public float delaytime;
	public GameObject destroyObject;

	// Use this for initialization
	void Start () {
		StartCoroutine ("zihui");
	}
	
	IEnumerator zihui()
	{
		while (true) {
		
			yield return new WaitForSeconds(delaytime);
			Destroy(destroyObject);
		}

	}
}
