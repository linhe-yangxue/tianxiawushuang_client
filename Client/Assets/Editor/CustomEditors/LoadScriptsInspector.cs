using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LoadScripts))]
public class LoadScriptsInspector : Editor
{
    private SerializedProperty scriptsProp;
    
    private void OnEnable()
    {       
        scriptsProp = serializedObject.FindProperty("mScripts");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "mScripts");

        DrawScripts(scriptsProp);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScripts(SerializedProperty prop)
    {
        string noExistScript = string.Empty;
        GUILayout.Label("Script List");

        for (int i = 0; i < prop.arraySize; ++i)
        {
            Rect ctrlRect = EditorGUILayout.BeginHorizontal();

            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element);

            OnDragAndDrop(ctrlRect, element);

            string scriptName = element.stringValue;

            if (!string.IsNullOrEmpty(scriptName) && string.IsNullOrEmpty(noExistScript))
            {
                System.Type t = System.Reflection.Assembly.GetAssembly(target.GetType()).GetType(scriptName);

                if (t == null || !t.IsSubclassOf(typeof(Component)))
                {
                    noExistScript = scriptName;
                }
            }

            if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20f)))
            {
                prop.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+", EditorStyles.miniButton))
        {
            prop.InsertArrayElementAtIndex(prop.arraySize);
            SerializedProperty element = prop.GetArrayElementAtIndex(prop.arraySize - 1);
            element.stringValue = "";
        }

        if (!string.IsNullOrEmpty(noExistScript))
        {
            EditorGUILayout.HelpBox("No Exist Component \"" + noExistScript + "\"", MessageType.Warning);
        }
    }

    private void OnDragAndDrop(Rect rect, SerializedProperty prop)
    {
        if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            && rect.Contains(Event.current.mousePosition)
            && DragAndDrop.paths != null
            && DragAndDrop.paths.Length > 0)
        {
            string scriptName = GetScriptName(DragAndDrop.paths[0]);

            if (!string.IsNullOrEmpty(scriptName))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                prop.stringValue = scriptName;
            }
        }
    }

    private string GetScriptName(string path)
    {
        string[] pathSplit = path.Split('/');

        if (pathSplit.Length > 0)
        {
            string fileName = pathSplit[pathSplit.Length - 1];
            int postfixIndex = fileName.IndexOf(".cs");

            if (postfixIndex < 0)
                postfixIndex = fileName.IndexOf(".js");

            if (postfixIndex >= 0)
            {
                return fileName.Substring(0, postfixIndex);
            }
        }

        return string.Empty;
    }
}