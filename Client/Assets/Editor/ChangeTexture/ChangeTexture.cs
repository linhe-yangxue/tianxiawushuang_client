using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
//using System.Threading;

public class ChangeTexture : MonoBehaviour
{
    private static bool mbIsNeedLog = true;
    [MenuItem("Edit/ChangeTexture")]
    public static void ChangeTextures()
	{
        int i = 0;
        object[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        int iCount = objs.Length;
        ShowLog("count is " + objs.Length);
        //// Uses the second Core or Processor for the Test
        //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2);
        //// Prevents "Normal" processes 
        //// from interrupting Threads
        //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        //// Prevents "Normal" Threads 
        //Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();

        Stopwatch stopwatch2 = new Stopwatch();
        foreach (var o in objs)
        {
            stopwatch2.Reset();
            stopwatch2.Start();

            Texture2D obj = o as Texture2D;
            if (null == obj)
                continue;

            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));

            TextureImporterSettings settings = new TextureImporterSettings();
            bool needUpdate = false;
            settings.ApplyTextureType(TextureImporterType.Advanced, true);
            ti.ReadTextureSettings(settings);

            if (ti.DoesSourceTextureHaveAlpha())
            {
                if (obj.format != TextureFormat.ETC2_RGBA8)
                {
                    settings.textureFormat = TextureImporterFormat.ETC2_RGBA8;
                    needUpdate = true;
                }
            }
            else
            {
                if (obj.format != TextureFormat.ETC2_RGB)
                {
                    settings.textureFormat = TextureImporterFormat.ETC2_RGB4;
                    needUpdate = true;
                }
            }
            if (needUpdate)
            {
                ti.SetTextureSettings(settings);
                settings.ApplyTextureType(TextureImporterType.Advanced, true);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj));
            }
            i++;
            stopwatch2.Stop();
            ShowLog("NO." + i + ": end Ticks2: " + stopwatch2.ElapsedTicks + " mS2: " + stopwatch2.ElapsedMilliseconds);
        }
        ShowLog("total end Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
        stopwatch.Stop();
	}

    private static void ShowLog(string text)
    {
        if (mbIsNeedLog)
            UnityEngine.Debug.Log(text);
    }
}
