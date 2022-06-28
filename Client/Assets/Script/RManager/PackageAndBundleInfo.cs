using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RManager
{
    public class PackageAndBundleInfo
    {
        /// <summary>
        /// bundle 名称，用于管理器中识别一个 bundle 包
        /// </summary>
        public string   Name            {get; set;}

        /// <summary>
        /// 如果bundle 是多个Asset集合包，则此域应为false
        /// 注意： 当前不支持多个Asset 打包为一个bundle
        /// </summary>
        public bool     IsOneAsset      {get; set;}          

        /// <summary>
        /// 如果 IsOneAsset 为 true, 并且包中的资源为 贴图、sahder、 sound 等等，则此域为true
        /// </summary>
        public bool     IsAtomic        {get; set;}

        /// <summary>
        /// 注意： 当前不支持多个Asset 打包为一个bundle
        /// </summary>
        public List<string> AllAssets   {get; set;}

        /// <summary>
        /// 当前 bundle 被引用的次数
        /// </summary>
        public int      BundleRefCount  {get; set;}

        /// <summary>
        /// 当前 asset 被引用次数
        /// </summary>
        public int      AssetRefCount   {get; set;}

        public Dictionary<string, PackageAndBundleInfo> Dependers {get; set;}
        public List<PackageAndBundleInfo> Dependens {get; set;}

        public string       BundlePath  {get; set;}
        public AssetBundle  Bundle      {get; internal set;}

        public UnityEngine.Object   MainAsset   {get; set;}

        public int          AssetSize   {get; internal set;}

        public bool         IsAssetValid        {get { return MainAsset != null; }}
        public bool         IsBundleInPlace     {get { return Bundle != null; }}
        //public string       BytesMD5 {get; set;}

        public bool         IsBundleInLoading   {get; set;}

        public PackageAndBundleInfo()
        {
            Name = "";
            IsOneAsset = false;
            AllAssets = null;
            BundleRefCount = 0;
            AssetRefCount = 0;
            Dependers = null;
            Dependens = null;
            Bundle = null;
            MainAsset = null;
            //BytesMD5 = null;
            IsBundleInLoading = false;
        }

        public UnityEngine.Object GetMainAsset()
        {
            if(!IsAssetValid)
                return null;

            AssetRefCount++;
            return MainAsset;
        }

        public void ReleaseMainAsset()
        {
            AssetRefCount--;

            if(AssetRefCount <= 0)
            {
                GameObject.Destroy(MainAsset);
                MainAsset = null;
            }
        }

        public void IncAllDependsBundleRef()
        {
            if(Dependens == null)
                return;

            foreach(var d in Dependens)
            {
                d.BundleRefCount++;
                d.IncAllDependsBundleRef();
            }
        }
    }
}
