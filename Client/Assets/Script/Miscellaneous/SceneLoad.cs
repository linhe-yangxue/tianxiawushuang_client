using UnityEngine;
using System.Collections;
using System.IO;

public class SceneLoad : MonoBehaviour {

    public string sceneName = "";
    public string loadFromURL = "";
    public int version = 0;
    public bool unload = false;

    static bool isLoading = false;
	// Use this for initialization
	void Start () {
        if(!isLoading)
        {
            isLoading = true;
            if(string.IsNullOrEmpty(sceneName))
            {
                sceneName = Path.GetFileNameWithoutExtension(loadFromURL);
            }
            StartCoroutine(LoadSceneFromU3d(sceneName, loadFromURL, version));
        }
	}

    
    IEnumerator LoadSceneFromU3d(string name, string sceneURL, int version)
    {
        Caching.CleanCache();
        Resources.UnloadUnusedAssets();

        DEBUG.Log("Loading scene [" +  name + "] from u3d file : " + sceneURL);
        var www = WWW.LoadFromCacheOrDownload(sceneURL, version);
        yield return www;

        if(www.error != null)
        {
            DEBUG.LogError("www: " + www.error);
            yield break;
        }

        {
            // force to be loaded in memory
            var bundel = www.assetBundle;

            if(bundel == null)
            {
                DEBUG.LogError("there is NO assetbundle loaded!");
                yield break;
            }

            Application.LoadLevel(name);

            if(Application.loadedLevelName.CompareTo(name) == 0)
            {
                DEBUG.Log("scene [" + name + "] loaded.");
            }
            else
            {
                DEBUG.Log("lelvel [" + Application.loadedLevelName + "] loaded.");
            }

            if(unload)
                bundel.Unload(false);
        }
    }

}
