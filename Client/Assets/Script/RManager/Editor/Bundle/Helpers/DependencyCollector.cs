using Assets.Editor.Bundle;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BundleHelper
{
    public class AssetDependency
    {
        Object asset;
        List<AssetDependency>   dependencies;
    }

    public class DependencyCollector
    {
        ResourcesMapItemInfo mainAssetInfo;
        FileStream           tempFile;

        //public AssetDependency dependencies;

        public DependencyCollector(ResourcesMapItemInfo assetInfo, FileStream exportFile)
        {
            mainAssetInfo = assetInfo;
            tempFile = exportFile;
        }

        public IEnumerator StartCollectDependencies()
        {
            string[] mainAssetName = { mainAssetInfo.assetPath };
            var allDepends = AssetDatabase.GetDependencies(mainAssetName);
            int c = allDepends.Count() - 1;

            var stringList = new List<string>();
            var str = "";

            if(c <= 0)
            {
                str = string.Format("资源 {0} 没有其它依赖.", mainAssetInfo.assetPath);
                DEBUG.Log(str);
                stringList.Add(str);
                goto EXIT;
            }

            str = string.Format("正在分析 {0} 的依赖...", mainAssetInfo.assetPath);
            DEBUG.Log(str);
            stringList.Add(str);
            yield return null;
            int i = 1;

            foreach(var item in allDepends)
            {
                if(item == mainAssetInfo.assetPath)
                    continue;

                str = string.Format("depends :  {0}  {1}/{2}", item, i, c);
                stringList.Add(str);
                DEBUG.Log(str);
                i++;
                yield return null;
            }

            str = string.Format(" {0} 的依赖分析完毕", mainAssetInfo.assetPath);
            stringList.Add(str);
            DEBUG.Log(str);
EXIT:
            if(tempFile == null)
                yield break;

            using (var streamWriter = new StreamWriter(tempFile))
            {
                foreach(var str_ in stringList)
                {
                    streamWriter.WriteLine(str_);
                }
            }
        }
    }
}
