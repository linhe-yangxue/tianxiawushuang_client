using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using Object = UnityEngine.Object;
using Asset;
using System.IO;
using System.Reflection;
using Common;

public class ExportItem
{
    public static void InitExportDirs()
    {
        Helper.EnsureDirValid(Helper.CombinePath(Application.dataPath, "Resources/Temp/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.dataPath, "Export/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Scenes/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Sounds/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "TextAssets/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Images/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Meshs/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Prefabs/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Controllers/"));
        Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Animations/"));
        //Helper.EnsureDirValid(Helper.CombinePath(Application.streamingAssetsPath, "Config/"));
    }
    public static void ExportObject2Item(
        GameObject g,
        Dictionary<string, ExportItem> olist, 
        List<GameObject> gameObjectList, 
        List<AssetRecord> recordList,
        List<MonoBehaviour> scriptsList)
    {
        PrefabUtility.DisconnectPrefabInstance(g);
        List<string> tmpList = new List<string>();
        bool needExport = false;
        Mesh mesh = null; 
        Material mate = null;
        byte renderType = 0;// SkinnedMeshRenderer:1;MeshFilter:2;Particle:3;MeshCollider:4;UISprite:5;UILabel:6
        SkinnedMeshRenderer renderer = g.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null && ((renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0) || renderer.sharedMesh != null)) renderType = 1;
        else GameObject.DestroyImmediate(renderer);
        MeshFilter filter = g.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            if (g.renderer == null || g.renderer.enabled == null) GameObject.DestroyImmediate(filter);
            else renderType = 2;
        }
        else
        {
            if (filter != null && g.renderer != null) GameObject.DestroyImmediate(g.renderer);
            GameObject.DestroyImmediate(filter);
        }
        ParticleSystem particle = g.GetComponent<ParticleSystem>();
        if (particle != null && particle.renderer.sharedMaterials != null && particle.renderer.sharedMaterials.Length > 0)
        {
            if (renderer != null) GameObject.DestroyImmediate(renderer);
            renderType = 3;
        }
        else GameObject.DestroyImmediate(particle);
        ParticleRenderer particleRenderer = g.GetComponent<ParticleRenderer>();
        if (particleRenderer != null && particleRenderer.sharedMaterials != null && particleRenderer.sharedMaterials.Length > 0) renderType = 3;
        else GameObject.DestroyImmediate(particleRenderer);
        AudioSource audio = g.GetComponent<AudioSource>();
        List<string> objList = new List<string>();
        List<byte> paras = new List<byte>();
        paras.Add(g.isStatic ? (byte)1 : (byte)0);
        paras.Add(renderType);
        paras.Add(g.activeSelf ? (byte)1 : (byte)0);

        AssetRecord ar = new AssetRecord();
        ar.Paras = paras.ToArray();
        if (renderType > 0) needExport = true;

        foreach (string s in ExportSceneConfig.DurationEnableList)
        {
            MonoBehaviour c = g.GetComponent(s) as MonoBehaviour;
            if (c != null && c.enabled)
            {
                c.enabled = false;
                needExport = true;
                scriptsList.Add(c);
            }
        }

        if (needExport)
        {
            if (g.renderer != null)
            {
                Material[] ms = g.renderer.sharedMaterials;
                string[] mStrings = new string[ms.Length];
                for (int i = 0; i < ms.Length; i++)
                {
                    mStrings[i] = getTexture(ms, i, tmpList, olist, mate);
                }
                g.renderer.sharedMaterials = ms;
                ar.Materials = mStrings;
                /*
                foreach (string s in tmpList)
                {
                    DEBUG.Log(s); 
                }*/
                ar.Textures = tmpList.ToArray();
            }
            switch (renderType)
            {
                case 1:
                    {
                        mesh = renderer.sharedMesh;
                        if (!ExportSceneHelp.IsDebug) renderer.sharedMesh = null;
                        break;
                    }
                case 2:
                    {
                        mesh = filter.sharedMesh;
                        if (!ExportSceneHelp.IsDebug) filter.sharedMesh = null;
                        break;
                    }
                case 3:
                    {
                        ParticleSystemRenderer pr = g.GetComponent<ParticleSystemRenderer>();
                        if (pr != null)
                        {
                            mesh = pr.mesh as Mesh;
                            if (!ExportSceneHelp.IsDebug) pr.mesh = null;
                        }
                        break;
                    }
            }
            if (mesh != null)
            {
                ExportItem item = new ExportItem(mesh, "Meshs", AssetDatabase.GetAssetPath(mesh));
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item; 
                ar.Mesh = item.ObjName;
            }
        }
        MeshCollider collider = g.GetComponent<MeshCollider>();
        if (collider != null && collider.sharedMesh != null)
        {
            needExport = true;
            Mesh meshCollider = collider.sharedMesh;
            ExportItem item = new ExportItem(meshCollider, "Meshs", AssetDatabase.GetAssetPath(meshCollider)); 
            if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
            objList.Add(string.Concat("m*", item.ExportFileName));
            if (!ExportSceneHelp.IsDebug) collider.sharedMesh = null;
        }
        Light lightFlare = g.GetComponent<Light>();
        if (lightFlare != null)
        {
            needExport = true;
            Flare flare = lightFlare.flare;
            if (flare != null)
            {
                ExportItem item = new ExportItem(flare, "Prefabs", AssetDatabase.GetAssetPath(flare));
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("flare*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug) lightFlare.flare = null;
            }
        }
        UISprite uISprite = g.GetComponent<UISprite>();
        if (uISprite != null)
        {
            needExport = true;
            if (uISprite.atlas != null)
            {
                Material mat = null;
                UIAtlas atlasValue = operateUIAtlas(uISprite.atlas, out mat);
                ExportItem item = new ExportItem(atlasValue, "Prefabs", AssetDatabase.GetAssetPath(uISprite.atlas));
                item.DependedMaterial = mat;
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("a*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug) uISprite.atlas = null;
            }
        }
        UILabel uILabel = g.GetComponent<UILabel>();
        if (uILabel != null)
        {
            needExport = true;
            if (uILabel.ambigiousFont != null)
            {
                Material mat = null;
                Object fontValue = operateFont(uILabel.ambigiousFont, out mat);
                ExportItem item = new ExportItem(fontValue, "Prefabs", AssetDatabase.GetAssetPath(uILabel.ambigiousFont));
                item.DependedMaterial = mat;
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("f*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug)
                {
                    uILabel.trueTypeFont = null;
                    uILabel.bitmapFont = null;
                }
            }
        }
        UITexture uITexture = g.GetComponent<UITexture>();
        if (uITexture != null)
        {
            needExport = true;
            if (uITexture.mainTexture != null)
            {
                Texture textureValue = uITexture.mainTexture;
                ExportItem item = new ExportItem(textureValue, "Images", AssetDatabase.GetAssetPath(textureValue));
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("t*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug) uITexture.mainTexture = null;
            }
            if (uITexture.material != null)
            {
                Material materialValue = operateMaterial(uITexture.material);
                ExportItem item = new ExportItem(materialValue, "Prefabs",AssetDatabase.GetAssetPath(uITexture.material));
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("t*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug) uITexture.mainTexture = null;
            }
        }
        UIPopupList uIPopupList = g.GetComponent<UIPopupList>();
        if (uIPopupList != null)
        {
            needExport = true;
            if (uIPopupList.atlas != null)
            {
                Material mat = null;
                UIAtlas atlasValue = operateUIAtlas(uIPopupList.atlas, out mat);
                ExportItem item = new ExportItem(atlasValue, "Prefabs", AssetDatabase.GetAssetPath(uIPopupList.atlas));
                item.DependedMaterial = mat;
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("pa*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug) uIPopupList.atlas = null;
            }
            if (uIPopupList.ambigiousFont != null)
            {
                Material mat = null;
                Object fontValue = operateFont(uIPopupList.ambigiousFont, out mat);
                ExportItem item = new ExportItem(fontValue, "Prefabs", AssetDatabase.GetAssetPath(uIPopupList.ambigiousFont));
                item.DependedMaterial = mat;
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("pf*", item.ExportFileName));
                if (!ExportSceneHelp.IsDebug)
                {
                    uIPopupList.trueTypeFont = null;
                    uIPopupList.bitmapFont = null;
                }
            }
        }
        if (g.animation != null)
        {
            needExport = true;
            if (!ExportSceneHelp.IsDebug) g.animation.clip = null;
            List<string> clipList = new List<string>();
            foreach (AnimationState s in g.animation)
            {
                clipList.Add(s.name);
                ExportItem item = new ExportItem(s.clip, "Animations", AssetDatabase.GetAssetPath(s.clip));
                if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
                objList.Add(string.Concat("ani*", item.ExportFileName));
            }
            if (!ExportSceneHelp.IsDebug)
            {
                foreach (string s in clipList)
                {
                    g.animation.RemoveClip(s);
                }
            }
        }
        if (audio != null && audio.clip != null)
        {
            needExport = true;
            ExportItem item = new ExportItem(audio.clip, "Sounds", AssetDatabase.GetAssetPath(audio.clip));
            if (!olist.ContainsKey(item.Key)) olist[item.Key] = item;
            ar.Audios = new string[] { string.Concat("aud*", item.ExportFileName) };//new string[] { item.ObjName };
            audio.clip = null;
        }
        if (g.GetComponent<AssetRecordRoot>() != null)
        {
            needExport = true;
            if (g.name.IndexOf("skybox") == -1) g.SetActive(false);
        }
        if (needExport)
        {
            ar.Objects = objList.ToArray();
            gameObjectList.Add(g); 
            recordList.Add(ar);
        }
    }
    public static UIAtlas operateUIAtlas(UIAtlas at, out Material specificMateral)
    {
        GameObject g = copyPrefab("UIAtlas", at.gameObject);
        at = g.GetComponent<UIAtlas>();
        specificMateral = at.spriteMaterial;
        at.spriteMaterial = operateMaterial(at.spriteMaterial);
        return at;
    }
    public static Object operateFont(Object font, out Material specificMateral)
    {
        UIFont bf = font as UIFont;
        specificMateral = null;
        if (bf != null)
        {
            GameObject g = copyPrefab("UIFont", bf.gameObject);
            bf = g.GetComponent<UIFont>();
            Material mate = operateMaterial(bf.material);
            specificMateral = bf.material;
            bf.material = mate;
            return bf;
        }
        return font;
    }

    public static GameObject copyPrefab(string type, GameObject resource)
    {
        string assetPath = AssetDatabase.GetAssetPath(resource);
        GameObject g = resource;
        if (!ExportSceneHelp.DefaultAsset(assetPath))
        {
            string key = string.Concat(type, "_", assetPath);
            if (fileExistsList.ContainsKey(key))
            {
                g = fileExistsList[key];
            }
            else
            {
                string newPath = string.Concat("Assets/Resources/Temp/", resource.name, ".prefab");
                string newName = "Assets/Resources/Temp/" + resource.name;
                AssetDatabase.CopyAsset(assetPath, newPath);
                AssetDatabase.Refresh();
                g = AssetDatabase.LoadAssetAtPath(newPath, typeof(GameObject)) as GameObject;//Resources.Load(newName) as GameObject;
                ExportSceneHelp.AddTmpFile(g);
                fileExistsList[key] = g;
            }
        }
        return g;
    }
    public static void ClearTempFile()
    {
        fileExistsList.Clear();
    }
    static Dictionary<string, GameObject> fileExistsList = new Dictionary<string, GameObject>();
    public static Material operateMaterial(Material mat)
    {
#if !UNITY_ANDROID
        if (mat != null && mat.HasProperty("_Alpha"))
        {
            Texture alpha = mat.GetTexture("_Alpha");
            if (alpha != null)
            {
                Material newMat = new Material(mat);
                newMat.name = mat.name;
                AssetDatabase.CreateAsset(newMat, string.Concat("Assets/Resources/Temp/", newMat.name, ".mat"));
                alpha = newMat.GetTexture("_Alpha");
                newMat.SetTexture("_Alpha", null);
                ExportSceneHelp.AddTmpFile(newMat);
                mat = newMat;
            }
             
        }
#endif
        return mat;
    }
    static bool createMaterialFolderState = false;
    static void createMaterialFolder()
    {
        if (createMaterialFolderState) return;
        createMaterialFolderState = true;
        string folder = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Resources");
        if (!System.IO.Directory.Exists(folder)) AssetDatabase.CreateFolder("Assets", "Resources");
        folder = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Material");
        if (!System.IO.Directory.Exists(folder)) AssetDatabase.CreateFolder("Assets/Resources", "Material");
        folder = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Shader");
        if (!System.IO.Directory.Exists(folder)) AssetDatabase.CreateFolder("Assets/Resources", "Shader");
    }
    public static void CollectStaticAssets(bool b)
    {
        if (b)
        {
            createMaterialFolder(); 
            staticList = new Dictionary<string, Object>();
            TextAsset sourceList = Resources.Load("Material/Static") as TextAsset;
            if (sourceList == null) return;
            List<string> strList = AssetItem.GetStringList(sourceList.text);
            if (strList.Count > 0)
            {
                Object tmp = null;
                for (int i = 0; i < strList.Count; i++)
                {
                    tmp = Resources.Load(string.Concat("Material/", strList[i]));
                    if (tmp != null)
                    {
                        Material m = tmp as Material;
                        staticList[m.shader.name] = tmp;
                    }
                }
            }
        }
        else
        {
            if (staticList != null)
            {
                AssetDatabase.DeleteAsset("Assets/Resources/Material/Static.txt");
                List<Object> list = new List<Object>();
                Material tmpMaterial = null;
                string tmpMaterialName = "", tmpShader = "";
                foreach (KeyValuePair<string, Object> o in staticList)
                {
                    if (!AssetDatabase.IsMainAsset(o.Value))
                    {
                        tmpMaterial = o.Value as Material;
                        if (tmpMaterial != null)
                        {
                            tmpMaterialName = tmpMaterial.shader.name.Replace("/", "_");
                            tmpMaterialName = tmpMaterialName.Replace(" ", "_");
                            tmpMaterial.name = tmpMaterialName;
                            AssetDatabase.CreateAsset(o.Value, "Assets/Resources/Material/" + tmpMaterialName + ".mat");
                            Shader sd = tmpMaterial.shader;
                            tmpShader = AssetDatabase.GetAssetPath(sd);
                            if (tmpShader.IndexOf("Resources/unity_builtin_extra") == -1)
                            {
                                AssetDatabase.CopyAsset(tmpShader, "Assets/Resources/Shader/" + tmpMaterialName + ".shader");
                            }
                        }
                    }
                    list.Add(o.Value);
                }
                using (StreamWriter sr = new StreamWriter(ExportSceneHelp.unityPath2PhysicsPath("Assets/Resources/Material/Static.txt"), false, System.Text.Encoding.UTF8))
                {
                    foreach (UnityEngine.Object m in list)
                    {
                        sr.WriteLine(m.name);
                    }
                }
            }
        }
    }
    static Dictionary<string, Object> staticList = null;
    static void addMaterialList(Material mate)
    {
        if (staticList != null && !staticList.ContainsKey(mate.shader.name))
        {
            Material mat = new Material(mate);
            if (ExportSceneConfig.ShaderList.ContainsKey(mat.shader.name))
            {
                ShaderConfig config = ExportSceneConfig.ShaderList[mat.shader.name];
                for (int i = 0; i < config.Parameters.Length; i++)
                {
                    if (config.Parameters[i].Type == ShaderParameterType.TEXTURE) mat.SetTexture(config.Parameters[i].Name, null);
                }
            }
            else
            {
                mat.mainTexture = null;
            }
            staticList[mat.shader.name] = mat;
        }
    }
    static List<Texture> tmpGetTextureList = new List<Texture>();
    public static string getTexture(Material[] ms, int index, List<string> tmpList, Dictionary<string, ExportItem> tlist, Material mate)
    {
        string mName = null;
        mate = ms[index];
        if (mate == null) return "";
        addMaterialList(mate);
        tmpGetTextureList.Clear();
        mName = ExportSceneConfig.GetString(mate, ref tmpGetTextureList);
        Texture tex = null;
        for (int i = 0; i < tmpGetTextureList.Count; i++)
        {
            tex = tmpGetTextureList[i];
            ExportItem item = new ExportItem(tex, "Images", AssetDatabase.GetAssetPath(tex));
            if (!tlist.ContainsKey(item.Key)) tlist[item.Key] = item;
            tmpList.Add(index + "*" + item.ObjName);
        }
        ms[index] = null;
        return mName;
    }
    public ExportItem(UnityEngine.Object obj, string type, string RealAssetPath)
    {
        if (obj == null) return;
        UnityPath = AssetDatabase.GetAssetPath(obj);
        RealAssetUnityPath = RealAssetPath;
        FileName = RealAssetUnityPath.Substring(RealAssetUnityPath.LastIndexOf("/") + 1);
        int pointIndex = FileName.LastIndexOf(".");
        if (pointIndex != -1) ObjName = FileName.Substring(0, FileName.LastIndexOf("."));
        if (string.Compare(ObjName, obj.name) != 0)
        {
            ObjName = ObjName + "_" + obj.name;
        }
        if (!string.IsNullOrEmpty(ObjName))
        {
            ObjName = ObjName.Replace(" ", "_");
            ObjName = ObjName.Replace("@", "_");
        }

        //
        ObjName = ObjName + "_" + AssetFileName.UniformResourceAssetPath(RealAssetUnityPath).GetMD5();
        //
        Key = RealAssetUnityPath + "&" + ObjName;
        Obj = obj;
        Type = type;
        setExportPath();
    }
    public ExportItem(string unityPath, string type, string RealAssetPath)
    {
        UnityPath = unityPath;
        RealAssetUnityPath = RealAssetPath;
        FileName = RealAssetUnityPath.Substring(RealAssetUnityPath.LastIndexOf("/") + 1);
        int pointIndex = FileName.LastIndexOf(".");
        if (pointIndex != -1) ObjName = FileName.Substring(0, FileName.LastIndexOf("."));
        Key = RealAssetUnityPath + "&" + ObjName;
        Type = type;
        setExportPath();
    }
    void setExportPath()
    {
        switch (Type)
        {
            case "Images":
                {
                    ExportFileName = string.Concat(ObjName, ".t");
                    ExportPath = string.Concat("Images/", ExportFileName);
                    Key += ".t";
                    break;
                }
            case "Meshs":
                {
                    ExportFileName = string.Concat(ObjName, ".m");
                    ExportPath = string.Concat("Meshs/", ExportFileName);
                    Key += ".m";
                    break;
                }
            case "Sounds":
                {
                    ExportFileName = string.Concat(ObjName, ".a");
                    ExportPath = string.Concat("Sounds/", ExportFileName);
                    Key += ".a";
                    break;
                }
            case "Scenes":
                {
                    ExportFileName = string.Concat(ObjName, "_export.s");
                    ExportPath = string.Concat("Scenes/", ExportFileName);
                    Key += ".s";
                    break;
                }
            case "db":
            case "Config":
                {
                    ExportFileName = string.Concat(ObjName, ".xml");
                    ExportPath = string.Concat("Config/", ExportFileName);
                    Key += ".xml";
                    break;
                }
            case "Atlas":
            case "Skill":
            case "CutScene":
            case "Prefabs":
                {
                    ExportFileName = string.Concat(ObjName, ".p");
                    ExportPath = string.Concat("Prefabs/", ExportFileName);
                    Key += ".p";
                    break;
                }
            case "Animations":
                {
                    ExportFileName = string.Concat(ObjName, ".ani");
                    ExportPath = string.Concat("Animations/", ExportFileName);
                    Key += ".ani";
                    break;
                }

            case "Controllers":
                {
                    ExportFileName = string.Concat(ObjName, ".ctl");
                    ExportPath = string.Concat("Controllers/", ExportFileName);
                    Key += ".ctl";
                    break;
                }
            case "TextAssets":
                {
                    ExportFileName = string.Concat(ObjName, ".ta");
                    ExportPath = string.Concat("TextAssets/", ExportFileName);
                    Key += ".ta";
                    break;
                }
        }
    }
    public string Type;
    public string Key;
    public string UnityPath;
    public string ObjName;
    public string FileName;
    public string ExportPath;
    public string ExportFileName;
    public string RealAssetUnityPath;
    public Material DependedMaterial;
    public UnityEngine.Object Obj;
}
