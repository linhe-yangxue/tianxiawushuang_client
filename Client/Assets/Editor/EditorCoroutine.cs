using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class EditorCoroutine{

    static List<IEnumerator> methods = new List<IEnumerator>();

    public static void Start(IEnumerator method)
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;

        methods.Add(method);
    }

    static void Update()
    {

        lock (methods)
        {
            for (int index = 0; index < methods.Count; index++)
            {
                if (methods[index] != null)
                {
                    methods[index].MoveNext();
                }
                else if (methods[index] == null)
                {
                    methods.RemoveAt(index);
                }

            }
        }

    }
}
