using Assets.Editor.Bundle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BundleHelper
{
    public class DumpAssetInfo
    {
        ResourcesMapItemInfo assetFileInfo;
        public DumpAssetInfo(ResourcesMapItemInfo assetFileInfo_)
        {
            assetFileInfo = assetFileInfo_;
        }

        public IEnumerator StartDumpInfo()
        {
            var assetFilePathName = assetFileInfo.assetPath;

            DEBUG.Log(string.Format("Asset of {0} : ", assetFilePathName));
            yield return null;

            var assetObj = AssetDatabase.LoadAssetAtPath(assetFilePathName, typeof(UnityEngine.Object));
            if(assetObj == null)
            {
                DEBUG.Log("Can't load asset from path : " + assetFilePathName);
                yield break;
            }

            DEBUG.Log("typeinfo: " + assetObj.GetType().ToString());
            DEBUG.Log("asset name: " + assetObj.name);

            DEBUG.Log("U3d object : " + assetObj.ToString());
            
            yield return null;
        }
    }
}
