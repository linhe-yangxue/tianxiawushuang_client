using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	private static T _instance;
	private static object _lock = new object();
	public static T Instace {
		get {
			lock(_lock) {
				if(_instance == null) {
					_instance = (T) FindObjectOfType(typeof(T));
					if(_instance == null) {
						string goName = typeof(T).ToString();
						GameObject go = GameObject.Find(goName);
						if(go == null) {
                            go = GameObject.Find(CommonParam.UIRootName); //new GameObject();
							_instance = go.AddComponent<T>();
							//go.name = "(Singleton)" + typeof(T).ToString();
							DontDestroyOnLoad(go);
						}
					}
				}
				return _instance;
			}
		}
	}
}



public class SingletonData<T> where T : new()
{
    private static T instance;

    public static T Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }
    }

    public static void Reset()
    {
        instance = new T();
    }
}