using UnityEngine;
using System.Collections.Generic;
namespace Asset
{
    public class ExportSceneConfig
    {
        private static Dictionary<string, ShaderConfig> shaderConfigList = new Dictionary<string, ShaderConfig>();
        private static bool shaderConfigInit = true;
        public static Dictionary<string, ShaderConfig> ShaderList
        {
            get
            {
                if (shaderConfigInit)
                {
                    List<ShaderConfig> cList = new List<ShaderConfig>();
                    cList.Add(new ShaderConfig("Self-Illumin/Diffuse", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"), 
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_Illum"),
                        new ShaderParameter(ShaderParameterType.FLOAT, "_EmissionLM")
                    }));

                    cList.Add(new ShaderConfig("Transparent/Cutout/Diffuse", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"), 
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));

                    cList.Add(new ShaderConfig("FXMaker/Mask Additive Tint", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_TintColor"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_Mask")
                    }));

                    cList.Add(new ShaderConfig("Active/Rim", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_RimColor"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                        }));

                    cList.Add(new ShaderConfig("Active/RimTransparent", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_RimColor"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                        }));

                    cList.Add(new ShaderConfig("Custom/AddEffect", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));

                    cList.Add(new ShaderConfig("Easy/Easy", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                    }));

                    cList.Add(new ShaderConfig("Easy/EasyTransparent", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                    }));

                    cList.Add(new ShaderConfig("Easy/TwoPassTransparent", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                    }));

                    cList.Add(new ShaderConfig("Easy/WhiteColor", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_AddColor")
                    }));

                    cList.Add(new ShaderConfig("Game/MiniMap", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MaskTex")
                    }));
                    
                    cList.Add(new ShaderConfig("Projector/MouseCoord", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_ShadowTex")
                    }));
                    
                    cList.Add(new ShaderConfig("NewOutLine", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_OutlineColor")
                    }));

                    cList.Add(new ShaderConfig("Game/NiceSprite", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MaskTex")
                    }));

                    cList.Add(new ShaderConfig("Projector/Multiply", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_ShadowTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_FalloffTex")
                    }));

                    cList.Add(new ShaderConfig("Particles/Additive", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_TintColor"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));

                    cList.Add(new ShaderConfig("Particles/Additive(Soft)", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));

                    cList.Add(new ShaderConfig("Particles/Alpha Blended", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_TintColor"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));
                    
                    cList.Add(new ShaderConfig("Scene/Water", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_shui"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_node_2_copy"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_nodm")
                    }));
                    
                    cList.Add(new ShaderConfig("Diffuse", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_Color"), 
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));
                    cList.Add(new ShaderConfig("Mobile/Transparent Cutout", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_Alpha"),
                        new ShaderParameter(ShaderParameterType.FLOAT, "_Cutoff")
                    }));
                    cList.Add(new ShaderConfig("Mobile/Particles/Additive Culled", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_TintColor"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_Alpha")
                    }));

                    cList.Add(new ShaderConfig("Mobile/Particles/Additive", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                    }));

                    cList.Add(new ShaderConfig("Mobile/Particles/Alpha Blended", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex")
                    }));

                    cList.Add(new ShaderConfig("Mobile/Particles/Alpha BlendedColored", new ShaderParameter[] {
                        new ShaderParameter(ShaderParameterType.COLOR, "_TintColor"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_MainTex"),
                        new ShaderParameter(ShaderParameterType.TEXTURE, "_Alpha")
                    }));
                    foreach (ShaderConfig c in cList)
                    {
                        shaderConfigList[c.Name] = c;
                    }
                    shaderConfigInit = false;
                }
                return shaderConfigList;
            }
        }
        public static string GetString(Material mate, ref List<Texture> list)
        {
            ShaderConfig tmpConfig = null;
            ShaderParameter tmpParameter = null;
            Texture tex = null;
            System.Text.StringBuilder mName = new System.Text.StringBuilder();
            if (ShaderList.ContainsKey(mate.shader.name))
            {
                tmpConfig = ShaderList[mate.shader.name];
                mName.Append(tmpConfig.Name);
                for (int i = 0; i < tmpConfig.Parameters.Length; i++)
                {
                    tmpParameter = tmpConfig.Parameters[i];
                    switch (tmpParameter.Type)
                    {
                        case ShaderParameterType.COLOR:
                            {
                                Color32 c = mate.GetColor(tmpParameter.Name);
                                mName.Append(",");
                                mName.Append(c.r.ToString());
                                mName.Append("*");
                                mName.Append(c.g.ToString());
                                mName.Append("*");
                                mName.Append(c.b.ToString());
                                mName.Append("*");
                                mName.Append(c.a.ToString());
                                break;
                            }
                        case ShaderParameterType.TEXTURE:
                            {
                                if (string.Compare(tmpParameter.Name, "_Alpha") == 0)
                                {
#if UNITY_ANDROID
                                    tex = mate.GetTexture(tmpParameter.Name);
                                    if (tex != null)
                                    {
                                        list.Add(tex);
                                        mName.Append(",");
                                        mName.Append(tex.name);
                                    }
                                    else
                                    {
                                        mName.Append(",");
                                    }
#else
                                    mName.Append(",");
#endif
                                }
                                else
                                {
                                    tex = mate.GetTexture(tmpParameter.Name);
                                    if (tex != null)
                                    {
                                        list.Add(tex);
                                        mName.Append(",");
                                        mName.Append(tex.name);
                                    }
                                    else
                                    {
                                        mName.Append(",");
                                    }
                                }
                                break;
                            }
                        case ShaderParameterType.FLOAT:
                            {
                                mName.Append(",");
                                float f = mate.GetFloat(tmpParameter.Name);
                                mName.Append(f.ToString("f6"));
                                break;
                            }
                    }
                }
            }
            else
            {
                tex = mate.mainTexture;
                mName.Append(mate.shader.name);
                if (tex != null)
                {
                    list.Add(tex);
                    mName.Append(",");
                    mName.Append(tex.name);
                }
                else
                {
                    mName.Append(",");
                }
            }
            return mName.ToString();
        }
        public static void SetMaterial(Material mat, List<Texture> list, string[] setting)
        {
            if (ShaderList.ContainsKey(mat.shader.name))
            {
                ShaderConfig config = ExportSceneConfig.ShaderList[mat.shader.name];
                ShaderParameter tmpParameter = null;
                int textureIndex = 0;
                for (int i = 0; i < config.Parameters.Length; i++)
                {
                    tmpParameter = config.Parameters[i];
                    string str = setting[i + 1];
                    switch (tmpParameter.Type)
                    {
                        case ShaderParameterType.COLOR:
                            {
                                string[] colorItem = str.Split(new char[] { '*' });
                                mat.SetColor(tmpParameter.Name, new Color32(byte.Parse(colorItem[0]), byte.Parse(colorItem[1]), byte.Parse(colorItem[2]), byte.Parse(colorItem[3])));
                                break;
                            }
                        case ShaderParameterType.TEXTURE:
                            {
                                if (string.Compare(tmpParameter.Name, "_Alpha") == 0)
                                {
#if UNITY_ANDROID
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        if (textureIndex < list.Count)
                                        {
                                            mat.SetTexture(tmpParameter.Name, list[textureIndex]);
                                            mat.SetTextureScale(tmpParameter.Name, Vector2.one);
                                        }
                                        textureIndex++;
                                    }
#endif
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        if (textureIndex < list.Count)
                                        {
                                            mat.SetTexture(tmpParameter.Name, list[textureIndex]);
                                            mat.SetTextureScale(tmpParameter.Name, Vector2.one);
                                        }
                                        textureIndex++;
                                    }
                                }
                                break;
                            }
                        case ShaderParameterType.FLOAT:
                            {
                                mat.SetFloat(tmpParameter.Name, float.Parse(str));
                                break;
                            }
                    }
                }
            }
            else
            {
                if (list.Count >= 1)
                {
                    mat.mainTexture = list[0];
                    mat.mainTextureScale = Vector2.one;
                }
            }
        }
        public static List<string> DurationEnableList
        {
            get
            {
                if (durationEnableInit)
                {
                    durationEnableList.Add("RoleSetting");
                    durationEnableList.Add("AllPetAttributeInfoUI");
                    durationEnableList.Add("CPlaySound");
                    durationEnableList.Add("pvp_active_point");
                    durationEnableList.Add("CutsceneManager");
                    durationEnableList.Add("UILabel");
                    durationEnableList.Add("UITexture");
                    durationEnableList.Add("UISprite");
                    durationEnableList.Add("UIPopupList");
                    durationEnableList.Add("TypewriterEffect");

                    durationEnableList.Add("NcUvAnimation");
                    durationEnableList.Add("ActiveBirth");
                    durationEnableList.Add("MainProcess");
                    durationEnableList.Add("NewFightUIEffect");
                    durationEnableList.Add("TipContainer");
                    durationEnableList.Add("AudioSourceEx");
                    durationEnableInit = false;

                }
                return durationEnableList;
            }
        }

        public static Dictionary<string,int> DefferedEnableMonoscripts 
        {
            get 
            {
                if(defferedEnableMonoscripts.Count != DurationEnableList.Count)
                {
                    defferedEnableMonoscripts.Clear();
                    foreach(var s in durationEnableList)
                        defferedEnableMonoscripts[s] = 0;
                }

                return defferedEnableMonoscripts;
            }
        }
        private static bool durationEnableInit = true;
        private static List<string> durationEnableList = new List<string>();
        private static Dictionary<string,int> defferedEnableMonoscripts = new Dictionary<string, int>();
    }
    public class ShaderConfig
    {
        public ShaderConfig(string name, ShaderParameter[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }
        public string Name = null;
        public ShaderParameter[] Parameters = null;
    }
    public class ShaderParameter
    {
        public ShaderParameter(ShaderParameterType type, string name)
        {
            Type = type;
            Name = name;
        }
        public ShaderParameterType Type;
        public string Name = null;
    }
    public enum ShaderParameterType
    {
        FLOAT = 0,
        COLOR,
        TEXTURE
    }
}