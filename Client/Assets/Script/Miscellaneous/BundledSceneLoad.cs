using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BundledSceneLoad : MonoBehaviour {

    public string sceneName = "";
    public string sceneBundleName = "";
    public string loadFromURL = "";
    public int version = 1;

    static bool isLoading = false;
	// Use this for initialization
	void Start () {
        if(!isLoading)
        {
            isLoading = true;
            StartCoroutine(LoadSceneFromBundle(sceneBundleName, loadFromURL, version));
        }
	}

    void OnLevelWasLoaded(int level)
    {
        DEBUG.Log("Level of " + level + " loaded.");
    }

    void OnLevelWasLoaded()
    {
        DEBUG.Log("Level  loaded.");
    }
   
    IEnumerator LoadSceneFromBundle(string name, string sceneURL, int version)
    {
        DEBUG.Log("Loading scene bundle [" +  name + "] from bundle file : " + sceneURL);

        Caching.CleanCache();

        using (var www = WWW.LoadFromCacheOrDownload(sceneURL, version))
        {
            yield return www;

            if (www.error != null)
            {
                DEBUG.LogError(www.error);
                yield break;
            }

            // force to be loaded in memory
            var bundle = www.assetBundle;

            if (bundle == null)
            {
                DEBUG.LogError("there is NO assetbundle loaded!");
                yield break;
            }

            {
                var objects = bundle.LoadAll();
                DEBUG.Log("bunlde loaded, object count = [" + objects.Length + "]; now loading u3d binary of name [" + name + "]...");
            }

            var u3d = bundle.Load(name, typeof(TextAsset)) as TextAsset;

            if (u3d != null)
            {
                DEBUG.Log(" u3d object loaded : name = " + u3d.name);

                var bytes = u3d.bytes;

                DEBUG.Log(" constructing scene bundle from bytes of length: " + bytes.Length);

                var u3dObjet = AssetBundle.CreateFromMemory(bytes);
                yield return u3dObjet;
                //var u3dObjet = AssetBundle.CreateFromMemoryImmediate(bytes);

                // unload origin bundle since we have done with it
                bundle.Unload(true);

                if (u3dObjet.assetBundle == null)
                {
                    DEBUG.LogError("Failed to construct u3d bundle");
                    yield break;
                }

                DEBUG.Log(string.Format("u3d bundle succeed loaded, now try loading scene [{0}] form it ", sceneName));

                Application.LoadLevel(sceneName);
                // there will be error if we call Unload here, even with 'false' supplied as argument,
                // we must defer the unload operation to the time when some job have done.
                //u3dObjet.assetBundle.Unload(false);
                DEBUG.Log("Done!");

                // it will be ok to unload resource when level loaded 
                yield return new WaitForSeconds(1);
                //u3dObjet.Unload(false);
                u3dObjet.assetBundle.Unload(false);
            }
            else
            {
                DEBUG.Log(" u3d object is null");
                // use standad loading API
                yield break;
            }

            if (Application.loadedLevelName.CompareTo(sceneName) == 0)
            {
                DEBUG.Log("scene [" + sceneName + "] loaded.");
            }
            else
            {
                DEBUG.Log("lelvel [" + Application.loadedLevelName + "] loaded.");
            }

            Resources.UnloadUnusedAssets();
        }
    }

}
