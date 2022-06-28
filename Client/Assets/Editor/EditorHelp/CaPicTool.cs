using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class CaPicTool : MonoBehaviour
{
    private Camera cam;
    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>(); 
        cam.clearFlags = CameraClearFlags.Nothing;
        cam.orthographic = true;      
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pai();
            SavaData();
        }
    }

    private void SavaData()
    {
        //已经改为左下  和  右上
        StringBuilder sb = new StringBuilder();
        sb.Append("左下角的坐标=" + GetBottomLeftPosition().ToString());
        sb.AppendLine("右上角的坐标=" + GetTopRightPosition().ToString());
        File.WriteAllBytes(Application.dataPath + "/Position/Pos.txt", Encoding.Default.GetBytes(sb.ToString()));
        Debug.Log("Work Done!!!");
        AssetDatabase.Refresh();
    }

    private Vector3 GetBottomLeftPosition()
    {
        Vector3 ScPos = new Vector3(0, 0, 0);
        return cam.ScreenToWorldPoint(ScPos);
    }

    private Vector3 GetTopRightPosition()
    {
        Vector3 ScPos = new Vector3(Screen.width, Screen.height, 0);
        return cam.ScreenToWorldPoint(ScPos);
    }

    public void Pai()
    {
        if (!Directory.Exists(Application.dataPath + "/Position/"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Position/");
        }
        Application.CaptureScreenshot(Application.dataPath + "/Position/"+Time.frameCount + ".png");
        AssetDatabase.Refresh();
    }

}
