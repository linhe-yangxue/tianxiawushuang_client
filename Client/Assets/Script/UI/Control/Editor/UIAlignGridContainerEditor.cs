using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UIAlignGridContainer))]
public class UIAlignGridContainerEditor : Editor
{
    UIGridContainer m_Instance;
    PropertyField[] m_fields;

    public void OnEnable()
    {
        m_Instance = target as UIAlignGridContainer;
        m_fields = ExposeProperties.GetProperties(m_Instance);
    }

    public override void OnInspectorGUI()
    {

        if (m_Instance == null)
            return;

        this.DrawDefaultInspector();

        ExposeProperties.Expose(m_fields);
    }
}
