using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Editor.Bundle;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine;

namespace BundleHelper
{
    public interface IAssetInfoCollector
    {
        IEnumerator StartCollection(List<ResourcesMapItemInfo> result);
    }

    public class SelectedAssetsInfoCollector : IAssetInfoCollector
    {
        bool  mIsSkipMonoScript = false;

        public SelectedAssetsInfoCollector()
        {
            IsCollectAny = false;
        }

        public bool SkipMonoScript { get { return mIsSkipMonoScript; } set { mIsSkipMonoScript = value;} }
        public bool IsCollectAny { get; set; }

        public Func<UnityEngine.Object, bool> FilterPredicate {get; set;}

        public IEnumerator StartCollection(List<ResourcesMapItemInfo> result)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Assets info collection", "collecting all assets info that selected...", 0))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            var selected = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets); //.objects;//
            var i = 0;
            var c = selected.Length;

            try
            {
                foreach (var a in selected)
                {
                    if(mIsSkipMonoScript && (a is MonoScript))
                        continue;

                    if(FilterPredicate != null && !FilterPredicate(a))
                        continue;

                    var assetInfo = U3DAssetsFileInfo.GetAssetFileInfo(a, true);
                    if(assetInfo.isRealAsset || IsCollectAny)
                        result.Add(assetInfo);

                    i++;

                    if (EditorUtility.DisplayCancelableProgressBar("Assets info collection", "collecting all assets info that selected...", (float)i / (float)c))
                    {
                        EditorUtility.ClearProgressBar();
                        yield break;
                    }
                }
            }
            finally
            {
                //DEBUG.LogError("error when collcting assets information : " + ex.Message);
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
