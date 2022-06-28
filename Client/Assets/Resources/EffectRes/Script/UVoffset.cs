using UnityEngine;
using System.Collections;

public class UVoffset : MonoBehaviour {
	public int targetSlot;
	public float UV_X;

	public float UV_Y;


	
	// Update is called once per frame
	void Update () {
	
		float offsetX = Time.time * UV_X;
		float offsetY = Time.time * UV_Y;
		renderer.materials[targetSlot].SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));

	}
}
