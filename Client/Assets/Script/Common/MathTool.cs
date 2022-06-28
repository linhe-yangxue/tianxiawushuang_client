using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathTool
{
    static public Vector3 RandPosInCircle(Vector3 center, float radius)
    {
        float r = Random.Range(0.0f, (float)System.Math.PI*2);
		//Quaternion q = Quaternion.AngleAxis(r, Vector3.up);
        radius = Random.Range(0, radius);
        return new Vector3((float)(center.x + System.Math.Sin(r) * radius), center.y, (float)(center.z + System.Math.Cos(r) * radius));        
    }

}
