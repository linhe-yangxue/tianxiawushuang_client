using UnityEngine;
public class MeshData : ScriptableObject
{
    public Vector3[] Vertices = null;
    public Vector2[] UV = null;
    public Vector2[] UV1 = null;
    public Vector2[] UV2 = null;
    public int[] Triangles = null;
    public Vector4[] Tangents = null;
    public int SMeshCount = 0;
}