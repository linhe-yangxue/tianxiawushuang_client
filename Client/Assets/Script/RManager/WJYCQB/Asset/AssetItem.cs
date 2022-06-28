using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
namespace Asset
{
    public delegate void GameObjectState(GameObject g, bool s);
    public class AssetItem
    {
        public AssetRecord Record = null;
        public GameObject Handle = null;
        public string RootName = null;
        private Texture[] texList = null;
        private AudioClip[] audioList = null;
        private UnityEngine.Object[] objectList = null;
        private Mesh mesh = null;
        private List<IAssetRequest> reqList = new List<IAssetRequest>();
        private int allCount = 0, currentIndex = 0, texIndex = 0;
        private IAssetRequest meshRequest = null;
        public event GameObjectState StateHandle = null;
        public event TaskHandle CompleteHandle = null;
        public bool CanCombine = false;
        public bool IsComplete = false;
        public AssetItem(GameObject obj, AssetRecord record)
        {
            Record = record;
            Handle = obj;
            RootName = Handle.transform.root.name;
            if (record.Textures != null && record.Textures.Length > 0)
            {
                texList = new Texture[record.Textures.Length];
                allCount += texList.Length;
            }
            if (!string.IsNullOrEmpty(Record.Mesh))
            {
                allCount += 1;
            }
            if (record.Audios != null && record.Audios.Length > 0)
            {
                audioList = new AudioClip[record.Audios.Length];
                allCount += audioList.Length;
            }
            if (record.Objects != null && record.Objects.Length > 0)
            {
                objectList = new UnityEngine.Object[record.Objects.Length];
                allCount += objectList.Length;
            }
            currentIndex = 0;
            texIndex = 0;
        }
        public void StartLoad()
        {
            if (!string.IsNullOrEmpty(Record.Mesh))
            {
                meshRequest = LoadHelp.LoadObject("Meshs/" + Record.Mesh + ".m", loadMeshComplete);
                reqList.Add(meshRequest);
            }
            if (texList != null)
            {
                for (int i = 0; i < Record.Textures.Length; i++)
                {
                    loadTexture(i);
                }
            }
            if (audioList != null)
            {
                for (int i = 0; i < Record.Audios.Length; i++)
                {
                    loadAudio(i);
                }
            }
            if (objectList != null)
            {
                for (int i = 0; i < Record.Objects.Length; i++)
                {
                    loadObject(i);
                }
            }
            if (allCount == 0) taskComplete();
        }
        private void loadTexture(int index)
        {
            string tName = Record.Textures[index];
            if (string.IsNullOrEmpty(tName)) return;
            string[] ts = tName.Split(new char[] { '*' });
            if (string.IsNullOrEmpty(ts[1]))
            {
                allCount--;
            }
            else
            {
                reqList.Add(LoadHelp.LoadObject("Images/" + ts[1] + ".t", delegate(AssetRequest req)
                {
                    texList[index] = req.mainAsset as Texture;
                    taskComplete();
                }));
            }
        }
        private void loadAudio(int index)
        {
            string tName = Record.Audios[index];
            if (string.IsNullOrEmpty(tName))
            {
                allCount--;
            }
            else
            {
                reqList.Add(LoadHelp.LoadObject("Sounds/" + tName + ".a", delegate(AssetRequest req)
                {
                    audioList[index] = req.mainAsset as AudioClip;
                    taskComplete();
                }));
            }
        }
        private void loadObject(int index)
        {
            string tName = Record.Objects[index];
            if (string.IsNullOrEmpty(tName))
            {
                allCount--;
            }
            else
            {
                string[] paras = tName.Split(new char[] { '*' });
                string[] paras1 = paras[1].Split(new char[] { '.' });
                switch (paras1[1])
                {
                    case "m":
                        {
                            tName = string.Concat("Meshs/", paras[1]);
                            break;
                        }
                    case "p":
                        {
                            tName = string.Concat("Prefabs/", paras[1]);
                            break;
                        }
                    case "t":
                        {
                            tName = string.Concat("Images/", paras[1]);
                            break;
                        }
                    case "a":
                        {
                            tName = string.Concat("Sounds/", paras[1]);
                            break;
                        }
                    case "ani":
                        {
                            tName = string.Concat("Animations/", paras[1]);
                            break;
                        }
                }
                reqList.Add(LoadHelp.LoadObject(tName, delegate(AssetRequest req)
                {
                    objectList[index] = req.mainAsset as UnityEngine.Object;
                    taskComplete();
                })); 
            }
        }
        static List<Texture> tmpTextureList = new List<Texture>();
        private List<Texture> getTexture(int mIndex)
        {
            tmpTextureList.Clear();
            string tmpStart = mIndex + "*";
            if (texList != null)
            {
                for (int i = texIndex; i < texList.Length; i++)
                {
                    if (Record.Textures[i].StartsWith(tmpStart))
                    {
                        tmpTextureList.Add(texList[i]);
                    }
                    else
                    {
                        texIndex = i;
                        break;
                    }
                }
            }
            return tmpTextureList;
        }
        private void setMaterial(Material[] mats, int index, string setting)
        {
            if (string.IsNullOrEmpty(setting)) return;
            getTexture(index);
            bool creat = true;
            string[] matSetting = setting.Split(new char[] { ',' });
            Material mat = GetMaterial(setting, matSetting[0], out creat);
            if (creat)
            {
                ExportSceneConfig.SetMaterial(mat, tmpTextureList, matSetting);
            }
            mats[index] = mat;
        }
        private void taskComplete()
        {
            currentIndex++;
            if (currentIndex >= allCount)
            {
                currentIndex = -10000;
                IsComplete = true;
                Material[] ms = null;
                byte renderType = 0;// SkinnedMeshRenderer:1;MeshFilter:2;Particle:3;MeshCollider:4;UISprite:5;UILabel:6
                ActiveSelf = Record.Paras[2] > 0; //String.Compare(Record.Paras[2], "1") == 0;
                //byte.TryParse(Record.Paras[1], out renderType);
                renderType = Record.Paras[1];
                if (renderType > 0 && renderType < 4)
                {
                    ms = new Material[Record.Materials.Length];
                    for (int i = 0; i < ms.Length; i++)
                    {
                        setMaterial(ms, i, Record.Materials[i]);
                    }
                }
                switch (renderType)
                {
                    case 1:
                        {
                            SkinnedMeshRenderer renderer = Handle.GetComponent<SkinnedMeshRenderer>();
                            //if (renderer != null && renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0) renderType = 1;
                            if (renderer != null)
                            {
                                renderer.sharedMaterials = ms;
                                if (mesh != null) renderer.sharedMesh = mesh;
                            }
                            break;
                        }
                    case 2:
                        {
                            DEBUG.Log(RootName);
                            MeshFilter filter = Handle.GetComponent<MeshFilter>();
                            //if (filter != null) renderType = 2;
                            Handle.renderer.sharedMaterials = ms;
                            filter.sharedMesh = mesh;
                            
                            //if (string.Compare(Record.Paras[0], "1") == 0) CanCombine = true;
                            CanCombine = Record.Paras[0] > 0;
                            break;
                        }
                    case 3:
                        {
                            //ParticleSystem particle = Record.gameObject.GetComponent<ParticleSystem>();
                            //if (particle != null && particle.renderer.sharedMaterials != null && particle.renderer.sharedMaterials.Length > 0) renderType = 3;
                            Handle.renderer.sharedMaterials = ms;
                            ParticleSystemRenderer pr = Handle.GetComponent<ParticleSystemRenderer>();
                            if (pr != null)
                            {
                                pr.mesh = mesh;
                            }
                            break;
                        }
                    case 4:
                        {
                            MeshCollider collider = Handle.GetComponent<MeshCollider>();
                            if (collider != null) renderType = 4;
                            collider.sharedMesh = mesh;
                            break;
                        }
                }
                if (objectList != null)
                {
                    UnityEngine.Object tmpObj = null;
                    string[] para = null;
                    for (int i = 0; i < objectList.Length; i++)
                    {
                        tmpObj = objectList[i];
                        if (tmpObj != null)
                        {
                            para = Record.Objects[i].Split(new char[] { '*' });
                            switch (para[0])
                            {
                                case "m":
                                    {
                                        Handle.GetComponent<MeshCollider>().sharedMesh = tmpObj as Mesh;
                                        break;
                                    }
                                case "f":
                                    {
                                        Handle.GetComponent<UILabel>().ambigiousFont = tmpObj;
                                        break;
                                    }
                                case "t":
                                    {
                                        Texture tmpObject = tmpObj as Texture;
                                        if (tmpObject != null) Handle.GetComponent<UITexture>().mainTexture = tmpObj as Texture;
                                        else Handle.GetComponent<UITexture>().material = tmpObj as Material;
                                        break;
                                    }
                                case "a":
                                    {
                                        Handle.GetComponent<UISprite>().atlas = tmpObj as UIAtlas;
                                        break;
                                    }
                                case "pa":
                                    {
                                        Handle.GetComponent<UIPopupList>().atlas = tmpObj as UIAtlas;
                                        break;
                                    }
                                case "pf":
                                    {
                                        Handle.GetComponent<UIPopupList>().ambigiousFont = tmpObj;
                                        break;
                                    }
                                case "flare":
                                    {
                                        Handle.GetComponent<Light>().flare = tmpObj as Flare;
                                        break;
                                    }
                                case "ani":
                                    {
                                        Handle.animation.AddClip(tmpObj as AnimationClip, tmpObj.name);
                                        if (Handle.animation.GetClipCount() == 1)
                                        {
                                            Handle.animation.clip = tmpObj as AnimationClip;
                                            if (Handle.transform.root.GetComponent<AssetRecordRoot>() == null)
                                            {
                                                Handle.transform.root.gameObject.SetActive(false);
                                                Handle.transform.root.gameObject.SetActive(true);
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
                if (Handle.audio != null && audioList != null)
                {
                    Handle.audio.clip = audioList[0];
                    Handle.audio.Play();
                } 
                if (StateHandle != null) StateHandle(Handle, ActiveSelf);
                if (CompleteHandle != null) CompleteHandle();
            }
        }
        public void SetDurationScripts()
        {
            foreach (string s in ExportSceneConfig.DurationEnableList)
            {
                MonoBehaviour c = Handle.GetComponent(s) as MonoBehaviour;
                if (c != null) c.enabled = true;
            }
        }
        /*
        public void SetActive()
        {
            if (Handle != null && Handle.activeSelf != ActiveSelf) Handle.SetActive(ActiveSelf);
        }*/
        public float Process
        {
            get
            {
                float process = 0;
                if (IsComplete) process = 1;
                else
                {
                    if (reqList.Count != 0)
                    {
                        for (int i = 0; i < reqList.Count; i++)
                        {
                            process += reqList[i].Process;
                        }
                        process = process / reqList.Count;
                    }
                }
                return process;
            }
        }
        private void loadMeshComplete(AssetRequest req)
        {
            mesh = req.mainAsset as Mesh;
            taskComplete();
        }
        public void DeleteMesh()
        {
            if (CanCombine && mesh != null)
            {
                LoadHelp.DeleteObject(meshRequest.Path);
                reqList.RemoveAt(0);
            }
        }
        public void Dispose()
        {
            if (Handle != null && Handle.renderer != null)
            {
                Material[] ms = Handle.renderer.sharedMaterials;
                if (ms != null && ms.Length > 0)
                {
                    Material tmp = null;
                    for (int i = 0; i < ms.Length; i++)
                    {
                        tmp = ms[i];
                        if (tmp != null)
                        {
                            DeleteMaterial(tmp.name);
                        }
                    }
                }
            }
            /*
            if (objectList != null)
            {
                for (int i = 0; i < objectList.Length; i++)
                {
                    objectList[i]
                }
            }*/
            foreach (IAssetRequest i in reqList)
            {
                LoadHelp.DeleteObject(i.Path);
            }
            reqList.Clear();
        }
        public bool ActiveSelf;
        public static void AddStatic(UnityEngine.Object[] list)
        {
            if (list != null && list.Length > 0)
            {
                UnityEngine.Object tmp = null;
                for (int i = 0; i < list.Length; i++)
                {
                    tmp = list[i];
                    if(tmp != null)
                    {
                        if (typeof(Material) == tmp.GetType())
                        {
                            Material m = tmp as Material;
                            tmp.name = m.shader.name;
                            if (materialList.ContainsKey(tmp.name))
                            {
                                GameObject.DestroyImmediate(tmp, true);
                            }
                            else
                            {
                                materialList[tmp.name] = tmp as Material;
                            }
                        }
                    }
                }
            }
        }
        static Material getMaterial(string key)
        {
            Material tmp = null;
            if (materialList.ContainsKey(key))
            {
                tmp = materialList[key];
            }
            else
            {
                tmp = new Material(Shader.Find(key));
                materialList[key] = tmp;
            }
            return tmp;
        }
        static Dictionary<string, Material> materialList = new Dictionary<string, Material>();
        public static Material GetMaterial(string materialName, string shaderName, out bool creat)
        {
            MaterialItem tmp = null;
            if (shareMaterials.ContainsKey(materialName))
            {
                tmp = shareMaterials[materialName];
                creat = false;
            }
            else
            {
                tmp = new MaterialItem(new Material(getMaterial(shaderName)));
                tmp.Mat.name = materialName;
                shareMaterials[materialName] = tmp;
                creat = true;
            }
            tmp.Count++;
            return tmp.Mat;
        }
        public static void DeleteMaterial(string materialName)
        {
            int InstanceWord = materialName.IndexOf(" (Instance)");
            if (InstanceWord > -1) materialName = materialName.Substring(0, InstanceWord);
            if (shareMaterials.ContainsKey(materialName))
            {
                MaterialItem tmp = shareMaterials[materialName];
                tmp.Count--;
                if (tmp.Count <= 0)
                {
                    GameObject.Destroy(tmp.Mat);
                    shareMaterials.Remove(materialName);
                }
            }
        }
        public static void ShowMaterialLog()
        {
            DEBUG.Log("Material cout:" + shareMaterials.Count);
            foreach (KeyValuePair<string, MaterialItem> i in shareMaterials)
            {
                DEBUG.Log("Material:" + i.Value.Mat.name + ":" + i.Value.Count);
            }
        }
        public static List<string> GetStringList(string s)
        {
            List<string> result = new List<string>();
            StringReader sr = new StringReader(s);
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                result.Add(line);
            }
            return result;
        }
        private static Dictionary<string, MaterialItem> shareMaterials = new Dictionary<string, MaterialItem>();
    }
    public class MaterialItem
    {
        public MaterialItem(Material mat)
        {
            Mat = mat;
            Count = 0;
        }
        public Material Mat;
        public int Count = 0;
    }
}