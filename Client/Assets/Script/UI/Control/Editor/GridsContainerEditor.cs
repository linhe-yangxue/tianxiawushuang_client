using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GridsContainer))]
public class GridsContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridsContainer tmpGridsContainer = target as GridsContainer;

        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical();

        GUI.changed = false;

        tmpGridsContainer.ControlTemplate = EditorGUILayout.ObjectField("Control Template", tmpGridsContainer.ControlTemplate, typeof(GameObject)) as GameObject;
        tmpGridsContainer.uiScrollBar = EditorGUILayout.ObjectField("UI Scroll Bar", tmpGridsContainer.uiScrollBar, typeof(UIScrollBar)) as UIScrollBar;
        tmpGridsContainer.Panel = EditorGUILayout.ObjectField("Panel", tmpGridsContainer.Panel, typeof(UIPanel)) as UIPanel;

        tmpGridsContainer.MaxPerLine = EditorGUILayout.IntField("Max Per Line", tmpGridsContainer.MaxPerLine);
        tmpGridsContainer.CellWidth = EditorGUILayout.FloatField("Cell Width", tmpGridsContainer.CellWidth);
        tmpGridsContainer.CellHeight = EditorGUILayout.FloatField("Cell Height", tmpGridsContainer.CellHeight);
        tmpGridsContainer.IsSrollEnabled = EditorGUILayout.Toggle("Is Scroll Enabled", tmpGridsContainer.IsSrollEnabled);
        tmpGridsContainer.MaxCount = EditorGUILayout.IntField("Max Count", tmpGridsContainer.MaxCount);

        tmpGridsContainer.EdgeShowLimitCount = EditorGUILayout.IntField("Edge Show Limit Count", tmpGridsContainer.EdgeShowLimitCount);
        tmpGridsContainer.NeedDynamicCreateCell = EditorGUILayout.Toggle("Need Dynamic Create Cell", tmpGridsContainer.NeedDynamicCreateCell);
        tmpGridsContainer.MaxCreateCountPerTime = EditorGUILayout.IntField("Max Create Count Per Time", tmpGridsContainer.MaxCreateCountPerTime);
        tmpGridsContainer.AutoResetMaxCreateCount = EditorGUILayout.Toggle("Auto Reset Max Create Count", tmpGridsContainer.AutoResetMaxCreateCount);

        EditorGUILayout.EndVertical();

        if (GUI.changed)
            EditorUtility.SetDirty(tmpGridsContainer);
    }
}
