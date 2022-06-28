using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CGTimeLine))]
public class CGTimeLineInspector : Editor
{
    private SerializedProperty cgEventObjectsProp;
    private CGTimeLine targetComponent;

    private void OnEnable()
    {
        targetComponent = target as CGTimeLine;
        cgEventObjectsProp = serializedObject.FindProperty("cgEventObjects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "cgEventObjects");

        DrawList(cgEventObjectsProp);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawList(SerializedProperty prop)
    {
        GUILayout.Label("CG Event Objects");

        for (int i = 0; i < prop.arraySize; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("gameObject"), new GUIContent());
            EditorGUILayout.PropertyField(element.FindPropertyRelative("time"), new GUIContent(), GUILayout.MaxWidth(35));

            if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(20f)))
            {
                targetComponent.DeleteAtIndex(i);
            }

            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(20f)))
            {
                targetComponent.InsertAtIndex(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Add New", EditorStyles.miniButton))
        {
            targetComponent.Add();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Sort By Time", EditorStyles.miniButton))
        {
            targetComponent.Refresh();
        }

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
}