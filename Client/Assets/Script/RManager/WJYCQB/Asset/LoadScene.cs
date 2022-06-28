using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Asset
{
    public class LoadScene : MonoBehaviour
    {
        public string SceneName = "";
        public void Start()
        {
            AssetManager.Init(this);
            SceneAsset.DontDestroyOnLoad(this.gameObject);
            if (!string.IsNullOrEmpty(SceneName))
            {
                AssetManager.LoadScene(SceneName, false, delegate()
                {
                    //DEBUG.LogError("load scene:" + name);
                });
            }
        }
        public void Update()
        {
            AssetManager.Update();
        }
    }
}
