using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Bundle
{
    public enum ResourcesType
    {
        GENERIC = 0,
        MODEL   = 1,
        TEXTRUE = 2,
        MESH    = 3,
        MATERIAL    = 4,
        ANIMATION   = 5,
        TRANSFORM   = 6,
        SCENE   = 7,
        SOUND   = 8,
        CONTROLLER  = 9,
        PREFAB = 10,
        SHADER = 11,
        FONT   = 12,
        TEXTASSET   = 13,
        UIATLAS = 14,
        OTHER   = 20
    }

    public struct ResourcesMapItemInfo
    {
        public string name;
        public UnityEngine.Object assetObj;
        public string assetPath;
        public string assetFolder;
        public Type   unityResosurceType;
        public string LURL;
        public string MD5;
        public long   size;
        public ResourcesType resourceType;
        public bool   isAtom;
        public bool   isRealAsset;
    }

    public class U3DAssetsFileInfo
    {
        public static string GetFileMD5(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                return MD5.CalculateMD5(file);
            }
        }

        public static ResourcesType GetResourceType(UnityEngine.Object assetObj, string assetExt)
        {
            if (assetObj is Texture)
                return ResourcesType.TEXTRUE;
            else if (assetObj is Material)
                return ResourcesType.MATERIAL;
            else if (assetObj is MonoScript)
                return ResourcesType.OTHER;
            else if (assetObj is TextAsset)
                return ResourcesType.TEXTASSET;
            else if (assetObj is UIAtlas)
                return ResourcesType.UIATLAS;
            else if (assetExt.IndexOf(".fbx") >= 0)
                return ResourcesType.MODEL;
            else if (assetExt.IndexOf(".prefab") >= 0)
                return ResourcesType.PREFAB;
            else if (assetExt.IndexOf(".unity") >= 0)
                return ResourcesType.SCENE;
            else if (assetObj is Shader)
                return ResourcesType.SHADER;
            else if (assetObj is AudioClip)
                return ResourcesType.SOUND;
            else if (assetObj is RuntimeAnimatorController)
                return ResourcesType.CONTROLLER;
            else if (assetObj is AnimationClip)
                return ResourcesType.ANIMATION;
            else if (assetObj is Font)
                return ResourcesType.FONT;

            return ResourcesType.GENERIC;
        }

        public static bool IsAtomResource(ResourcesType resType)
        {
            if(resType == ResourcesType.TEXTRUE
                || resType == ResourcesType.SOUND
                || resType == ResourcesType.SHADER
                || resType == ResourcesType.ANIMATION
                || resType == ResourcesType.FONT)
            {
                return true;
            }

            return false;
        }

        public static ResourcesMapItemInfo GetAssetFileInfo(UnityEngine.Object assetObj, bool calculateMD5)
        {
            var filePath = AssetDatabase.GetAssetPath(assetObj);
            var fileAttr = File.GetAttributes(filePath);
            var isRealAsset = (fileAttr & FileAttributes.Directory) != FileAttributes.Directory;

            var fileInfo = new FileInfo(filePath);
            var resInfo = new ResourcesMapItemInfo
                {
                    assetObj = assetObj,
                    name = assetObj.name,
                    assetPath = filePath,
                    assetFolder = Path.GetDirectoryName(filePath),
                    unityResosurceType = assetObj.GetType(),
                    LURL = filePath,
                    size = isRealAsset ? fileInfo.Length : 0,
                    resourceType = GetResourceType(assetObj, filePath.ToLower()),
                    isAtom = false,
                    isRealAsset = isRealAsset
                };

            resInfo.isAtom = IsAtomResource(resInfo.resourceType);

            if (calculateMD5 && isRealAsset)
            {
                resInfo.MD5 = GetFileMD5(filePath);
            }

            return resInfo;
        }
    }
}
