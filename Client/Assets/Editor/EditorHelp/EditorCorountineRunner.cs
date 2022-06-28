using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace EditorHelp
{
    public static class EditorCoroutineRunner
    {
        private class EditorCoroutine : IEnumerator
        {
            private Stack<IEnumerator> executionStatck;

            public EditorCoroutine(IEnumerator ce)
            {
                executionStatck = new Stack<IEnumerator>();
                executionStatck.Push(ce);
            }

            public bool MoveNext()
            {
                var next = executionStatck.Peek();

                if(next.MoveNext())
                {
                    var result = next.Current as IEnumerator;
                    if(result != null)
                    {
                        executionStatck.Push(result);
                    }

                    return true;
                }
                else
                {
                    if(executionStatck.Count > 1)
                    {
                        executionStatck.Pop();
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException("Unsupported operation");
            }

            public object Current
            {
                get { return executionStatck.Peek().Current; }
            }

            public bool Find(IEnumerator ce)
            {
                return executionStatck.Contains(ce);
            }
        }

        private static List<EditorCoroutine> editorCoroutines;
        private static List<IEnumerator> tempList;

        static EditorCoroutineRunner()
        {
            editorCoroutines = new List<EditorCoroutine>();
            tempList = new List<IEnumerator>();

            EditorApplication.update += Update;
        }

        public static IEnumerator StartEditorCoroutine(IEnumerator coroutine)
        {
            tempList.Add(coroutine);

            return coroutine;
        }

        private static bool Find(IEnumerator coroutine)
        {
            foreach(var ecc in editorCoroutines)
            {
                if(ecc.Find(coroutine))
                    return true;
            }

            return false;
        }

        private static void Update()
        {
            // run coroutines
            if(editorCoroutines.Count > 0)
                editorCoroutines.RemoveAll(cc => !cc.MoveNext());

            if(tempList.Count > 0)
            {
                foreach(var cc in tempList)
                {
                    if(!Find(cc))
                        editorCoroutines.Add(new EditorCoroutine(cc));
                }

                tempList.Clear();
            }
        }
    }
}
