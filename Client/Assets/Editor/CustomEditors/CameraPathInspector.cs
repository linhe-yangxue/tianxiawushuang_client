using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CameraPath))]
public class CameraPathInspector : ObjectPathInspector
{
    protected override void DrawExtra()
    {
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add By Game View", EditorStyles.miniButton))
        {
            var t = Camera.main.transform;
            targetComponent.Add(t.position, t.rotation);
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Add By Scene View", EditorStyles.miniButton))
        {
            var t = SceneView.lastActiveSceneView.camera.transform;
            targetComponent.Add(t.position, t.rotation);
        }
    }
}