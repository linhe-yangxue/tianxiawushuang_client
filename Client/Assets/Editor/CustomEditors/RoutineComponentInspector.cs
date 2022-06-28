using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Utilities.Routines;


[CustomEditor(typeof(RoutineComponent))]
public class RoutineComponentInspector : Editor
{
    private RoutineComponent targetComponent;
    private bool foldOutRunningRountines = false;

    private void OnEnable()
    {
        targetComponent = target as RoutineComponent;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical();
        foldOutRunningRountines = EditorGUILayout.Foldout(foldOutRunningRountines, foldOutRunningRountines ? "Hide running routines" : "Show running routines");

        if (foldOutRunningRountines)
        {
            EditorGUILayout.Space();
            DrawRoutines(targetComponent.rootRoutines);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRoutines(IEnumerable<IRoutine> iter)
    {
        using (var e = iter.GetEnumerator())
        {
            while (e.MoveNext())
            {
                EditorGUILayout.LabelField("- " + e.Current.GetType().Name);
                ++EditorGUI.indentLevel;
                DrawRoutines(e.Current.subRoutines);
                --EditorGUI.indentLevel;
            }
        }
    }
}