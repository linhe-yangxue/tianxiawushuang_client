using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
public class Material_Texture_Calculation : MonoBehaviour {

    public class AllInfo
    {
        private  string materialName;
        private List<string> names;
        private List<float> sizes;
        float maxsize;
        public AllInfo()
        {
            maxsize = 0;
            materialName = "";
            names = new List<string>();
            sizes = new List<float>();
        }
        public string MaterialName
        {
            get
            {
                return materialName;
            }
            set
            {
                value = materialName;
            }
        }
        public int Length
        {
            get
            {
                return names.Count;
            }
        }
        public float MaxSize
        {
            get
            {
                return maxsize;
            }
        }
        public List<string> TextureNames
        {
            get
            {
                return names;
            }
        }
        public List<float> TextureSizes
        {
            get
            {
                return sizes;
            }
        }
        public void Add(string texturename, float size)
        {
            names.Add(texturename);
            sizes.Add(size);
        }
        public void Add(string materialname)
        {
            materialName = materialname;
        }
        public void Sort()
        {
            if (Length > 0)
            {
                for (int i = 0; i < Length - 1; i++)
                {
                    for (int j = i + 1; j < Length; j++)
                    {
                        float tempint;
                        string tempstr;
                        if (sizes[i] < sizes[j])
                        {
                            tempint = sizes[j];
                            sizes[j] = sizes[i];
                            sizes[i] = tempint;
                            tempstr = names[j];
                            names[j] = names[i];
                            names[i] = tempstr;
                        }
                    }
                }
                maxsize = sizes[0];
            }  
        }
        public void Print()
        {
            UnityEngine.Debug.LogWarning("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            UnityEngine.Debug.LogWarning("Material Name: " + MaterialName);
            for (int i = 0; i < Length; i++)
            {
                UnityEngine.Debug.Log("      |Texture Name:" + names[i] + "   Texture Size:" + sizes[i] + " KB|");
            }
        }
        
    }
    [MenuItem("Tools/Calculations")]
    public static void Calculation()
    {
        GameObject[] gameObjects =  GameObject.FindObjectsOfType<GameObject>();
        List<string> materialnames = new List<string>();
        List<string> pngnames = new List<string>();
        List<AllInfo> calculation = new List<AllInfo>();
    //    UnityEngine.Debug.LogWarning("The number of the gameobjects are：" + gameObjects.Length);
        for (int i = 0; i < gameObjects.Length; i++)
        {
            Renderer renderer = gameObjects[i].GetComponent<Renderer>();
            if (null != renderer)
            {
                Material material = renderer.sharedMaterial;
                Material[] materials = renderer.sharedMaterials;
                int count = materials.Length;
                for (int j = 0; j < count; j++)
                {
                    if (null != materials[j])
                    {
                        if (materialnames.Contains(materials[j].name))
                            continue;
                        materialnames.Add(materials[j].name);
                        AllInfo allinfo = new AllInfo();
                        allinfo.Add(materials[j].name);
                        Shader shader = materials[j].shader;
                        if (null != shader)
                        {
                            int propertyNum = ShaderUtil.GetPropertyCount(shader);
                            for (int k = 0; k < propertyNum; k++)
                            {
                                if (ShaderUtil.GetPropertyType(shader, k) == ShaderUtil.ShaderPropertyType.TexEnv)
                                {
                                    string textureName = ShaderUtil.GetPropertyName(shader, k);
                                    Texture texture = materials[j].GetTexture(textureName);
                                    if (null != texture)
                                    {
                                        float textureSize = Profiler.GetRuntimeMemorySize(texture) / 2048.0f;
                                        string textureRealName = texture.name;
                                        if (!pngnames.Contains(textureRealName))
                                        {
                                            pngnames.Add(textureRealName);
                                            allinfo.Add(textureRealName, textureSize);
                                        }
                                    }
                                }
                            }
                        }
                        allinfo.Sort();
                        calculation.Add(allinfo);
                    }
                }
              //  if (null != material)
              //  {
              //      AllInfo allinfo = new AllInfo();
              //      allinfo.Add(material.name);
              //      Shader shader = material.shader;
              //      if (null != shader)
              //      {
              //          int propertyNum = ShaderUtil.GetPropertyCount(shader);
              //          for (int j = 0; j < propertyNum; j++)
              //          {
              //              if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
              //              {
              //                  string textureName = ShaderUtil.GetPropertyName(shader, j);
              //                  Texture texture =  material.GetTexture(textureName);
              //                  if (null != texture)
              //                  {
              //                      float textureSize = Profiler.GetRuntimeMemorySize(texture)/2048.0f;
              //                      string textureRealName = texture.name;
              //                      allinfo.Add(textureRealName, textureSize);
              //                  }
              //              }
              //          }
              //      }
              //      allinfo.Sort();
              //      calculation.Add(allinfo);
              //  }
            }
        }
        AddByDescending(calculation);
        Print(calculation);
        PrintToFile(calculation);
        UnityEngine.Debug.LogWarning("Materials Number: " + materialnames.Count);
        UnityEngine.Debug.LogWarning("Textures Number: " + pngnames.Count);
        PrintResults(materialnames, pngnames);
    }
    public static void AddByDescending(List<AllInfo> list)
    {
        int count = list.Count;
        for (int i = 0; i < count-1; i++)
        {
            for (int j = i+1; j < count; j++)
            {
                AllInfo allinfo;
                if (list[i].MaxSize < list[j].MaxSize)
                {
                    allinfo = list[i];
                    list[i] = list[j];
                    list[j] = allinfo;

                }
            }
        }
    }
    public static  void Print(List<AllInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Print();
        }
    }
    public static void PrintToFile(List<AllInfo> list)
    {
        string path = Application.dataPath + "/Calculation.txt";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream filestream = File.Create(path);
        StreamWriter streamwriter = new StreamWriter(filestream);
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            int length = list[i].Length;
            string zhushi = "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++";
            streamwriter.WriteLine(zhushi);
            string material = "  ->|Material Name: " + list[i].MaterialName+"|<-";
            streamwriter.WriteLine(material);
            for (int j = 0; j < length; j++)
            {
                string name = "      |Texture Name:" + list[i].TextureNames[j] + "   Texture Size:" + list[i].TextureSizes[j] + " KB|";
                streamwriter.WriteLine(name);
            }
        }
        streamwriter.Flush();
        streamwriter.Close();
        filestream.Close();
     //   Process.Start(path);
    }
    public static void PrintResults(List<string> mlist,List<string> plist)
    {
        string path = Application.dataPath + "/Calculation.txt";
        if (File.Exists(path))
        {
        //    FileStream filestream = File.Create(path);
            StreamWriter streamwriter = new StreamWriter(path, true);
            string zhushi1 = "***************************************************************************";
            string str1 = "Materials Number: " + mlist.Count;
            string zhushi2 = "***************************************************************************";
            string str2 = "Textures Number: " + plist.Count;
            string zhushi3 = "***************************************************************************";
            streamwriter.WriteLine(zhushi1);
            streamwriter.WriteLine(str1);
            streamwriter.WriteLine(zhushi2);
            streamwriter.WriteLine(str2);
            streamwriter.WriteLine(zhushi3);
            streamwriter.Flush();
            streamwriter.Close();
     //       filestream.Close();
            Process.Start(path);
        }
    }
}
