using System;
using UnityEngine;
[Serializable]
public class AssetRecord
{
    public string Mesh = "";
    //public string[] Paras = null;
    public byte[] Paras = null;
    public string[] Materials = null;
    public string[] Textures = null;
    public string[] Audios = null;
    public string[] Objects = null;
}