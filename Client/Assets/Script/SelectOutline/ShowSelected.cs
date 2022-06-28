using UnityEngine;
using System.Collections;

public class ShowSelected : MonoBehaviour {
	
	//public Shader selectedShader;
	public Color outterColor;

	
	private Color myColor ;
	private Shader myShader;
	private bool Selected = false;
	
	// Use this for initialization
	void Start () {
		myColor = renderer.material.color;
		myShader = renderer.material.shader;
		//selectedShader = GameCommon.FindShader("Hidden/RimLightSpce");
		//if(!selectedShader)
		{
		//	enabled = false;
		//	return;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown(){
		Selected  = !Selected;
		gameObject.layer= Selected ? 30 : 8;
	}
}
