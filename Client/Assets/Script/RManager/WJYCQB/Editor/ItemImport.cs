//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//
//public class ItemImport : AssetPostprocessor 
//{
//    void OnPreprocessTexture()
//    {
//        TextureImporter textureImporter = assetImporter as TextureImporter;
//        textureImporter.maxTextureSize = 2048;
//        //textureImporter.wrapMode = TextureWrapMode.Clamp;
//        textureImporter.generateMipsInLinearSpace = false;
//        string assetPathLower = assetPath.ToLower();
//        if (assetPathLower.Contains(".movie"))
//        {
//            textureImporter.textureType = TextureImporterType.GUI;
//            textureImporter.npotScale = TextureImporterNPOTScale.None;
//            textureImporter.isReadable = false;
//            textureImporter.normalmap = false;
//            //textureImporter.textureFormat = TextureImporterFormat.DXT5;
//        }
//        else if (assetPathLower.Contains(".png") || assetPathLower.Contains(".psd") || assetPathLower.Contains(".tga"))
//        {
//            textureImporter.textureType = TextureImporterType.Advanced;
//            textureImporter.normalmap = false;
//#if UNITY_ANDROID
//            textureImporter.textureFormat = TextureImporterFormat.ETC2_RGBA8;
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#elif  UNITY_IPHONE
//            textureImporter.textureFormat = TextureImporterFormat.PVRTC_RGBA4;
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#elif  UNITY_STANDALONE_WIN
//            textureImporter.textureFormat = TextureImporterFormat.DXT5;
//#else //STANDALONE mac
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#endif
//        }
//        else if (assetPathLower.Contains(".jpg"))
//        {
//            textureImporter.textureType = TextureImporterType.Advanced;
//            textureImporter.normalmap = false;
//#if UNITY_ANDROID
//            textureImporter.textureFormat = TextureImporterFormat.ETC_RGB4;
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#elif  UNITY_IPHONE
//            textureImporter.textureFormat = TextureImporterFormat.PVRTC_RGBA4;
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#elif  UNITY_STANDALONE_WIN
//            textureImporter.textureFormat = TextureImporterFormat.DXT1;
//#else //STANDALONE mac
//            textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
//            textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
//#endif
//
//        }
//    }
//    void OnPreprocessAudio()
//    {
//        AudioImporter audioImporter = assetImporter as AudioImporter;
//        audioImporter.format = AudioImporterFormat.Compressed;
//        audioImporter.compressionBitrate = 64 * 1024;
//    }
//}
