using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ObjectPath))]
public class ObjectPathInspector : Editor
{
    protected SerializedProperty pathNodesProp;
    protected ObjectPath targetComponent;

    private void OnEnable()
    {
        targetComponent = target as ObjectPath;
        pathNodesProp = serializedObject.FindProperty("pathNodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "pathNodes");

        DrawList(pathNodesProp);      

        DrawExtra();

        DrawPlayStop();

        serializedObject.ApplyModifiedProperties();        
    }

    private void DrawList(SerializedProperty prop)
    {
        GUILayout.Label("Path Nodes");

        for (int i = 0; i < prop.arraySize; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("point"), new GUIContent());
            EditorGUILayout.PropertyField(element.FindPropertyRelative("speed"), new GUIContent(), GUILayout.MaxWidth(35));

            if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(20f)))
            {
                targetComponent.DeleteAtIndex(i);
            }

            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(20f)))
            {
                ObjectPath.PathNode node = targetComponent.InsertAtIndex(i + 1);
                Selection.activeTransform = node.point;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Add New", EditorStyles.miniButton))
        {
            var size = prop.arraySize;
            ObjectPath.PathNode node = targetComponent.InsertAtIndex(prop.arraySize);

            if (size == 0)
                node.point.position = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

            Selection.activeTransform = node.point;
        }
    }

    private void DrawPlayStop()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Play", EditorStyles.miniButton))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                targetComponent.demonstrate = true;
                EditorApplication.isPlaying = true;
            }
        }

        if (GUILayout.Button("Stop", EditorStyles.miniButton))
        {
            EditorApplication.isPlaying = false;
        }

        EditorGUILayout.EndHorizontal();
    }

    protected virtual void DrawExtra()
    { }
}