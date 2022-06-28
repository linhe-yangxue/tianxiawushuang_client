using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace Utilities
{
    public class Serializer
    {
        static Serializer()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        }

        public static bool TrySerialize<T>(T obj, out byte[] buffer)
        {          
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                return true;
            }
            catch (Exception ex)
            {
                DEBUG.LogError("Serialization Failed : " + ex.Message);
                buffer = null;
                return false;
            }
        }

        public static bool TrySerialize<T>(T obj, out string str)
        {
            byte[] buffer;

            if (TrySerialize<T>(obj, out buffer))
            {
                str = Convert.ToBase64String(buffer);
                return true;
            }
            else 
            {
                str = "";
                return false;
            }
        }

        public static bool TryDeserialize<T>(byte[] buffer, out T obj)
        {           
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(buffer);
                obj = (T)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
                return true;
            }
            catch (Exception ex)
            {
                DEBUG.LogError("Deserialization Failed : " + ex.Message);
                obj = default(T);
                return false;
            }
        }

        public static bool TryDeserialize<T>(string str, out T obj)
        {
            byte[] buffer = Convert.FromBase64String(str);
            return TryDeserialize<T>(buffer, out obj);
        }
    }

    //public class GamePrefs
    //{
    //    public static void DeleteKey(string key)
    //    {
    //        if (!string.IsNullOrEmpty(key))
    //        {
    //            PlayerPrefs.DeleteKey(key);
    //        }
    //    }
    //
    //    public static bool HasKey(string key)
    //    {
    //        return !string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key);
    //    }
    //
    //    public static bool TrySet<T>(string key, T obj)
    //    {
    //        string str;
    //
    //        if (!string.IsNullOrEmpty(key) && Serializer.TrySerialize<T>(obj, out str))
    //        {
    //            PlayerPrefs.SetString(key, str);
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //
    //    public static bool TryGet<T>(string key, out T obj)
    //    {
    //        if (!PlayerPrefs.HasKey(key))
    //        {
    //            obj = default(T);
    //            return false;
    //        }
    //
    //        string str = PlayerPrefs.GetString(key, "");
    //        return Serializer.TryDeserialize<T>(str, out obj);
    //    }
    //}


    public class GamePrefs
    {
        private const string ROOT_DIR = "@RT/";

        public static void DeleteAll()
        {
            if (PlayerPrefs.HasKey(ROOT_DIR))
            {
                RawDeleteDir(ROOT_DIR);
                RawDeleteKey(ROOT_DIR);
            }
        }

        public static void DeleteKey(string key)
        {
            key = key.Trim('/');
            key = ROOT_DIR + key;

            if (PlayerPrefs.HasKey(key))
            {
                RawDeleteKey(key);
            }     
        }

        public static void DeleteDir(string dir)
        {
            dir = dir.Trim('/');
            dir = ROOT_DIR + dir + '/';

            if (PlayerPrefs.HasKey(dir))
            {
                RawDeleteDir(dir);
                RawDeleteKey(dir);
            }         
        }

        public static string[] GetAllKeysAndDirs(string dir)
        {
            string temp = dir.Trim('/');

            if (string.IsNullOrEmpty(temp))
            {
                dir = ROOT_DIR;
            }
            else
            {
                temp += "/";
                dir = ROOT_DIR + temp;
            }

            if (!PlayerPrefs.HasKey(dir))
            {
                return new string[0];
            }

            List<string> indices = null;

            if (RawTryGet<List<string>>(dir, out indices))
            {
                List<string> result = new List<string>();

                foreach (var sub in indices)
                {
                    result.Add(temp + sub);
                }

                return result.ToArray();
            }

            return new string[0];
        }

        public static bool IsDir(string dir)
        {
            return dir.EndsWith("/");
        }

        public static bool HasKey(string key)
        {
            key = key.Trim('/');
            key = ROOT_DIR + key;
            return PlayerPrefs.HasKey(key);
        }

        public static bool HasDir(string dir)
        {
            dir = dir.Trim('/');
            dir = ROOT_DIR + dir + "/";
            return PlayerPrefs.HasKey(dir);
        }

        public static bool Set<T>(string key, T obj)
        {
            key = key.Trim('/');
            key = ROOT_DIR + key;

            if (PlayerPrefs.HasKey(key))
            {
                return RawTrySet<T>(key, obj);
            }
            else 
            {
                if (RawTrySet<T>(key, obj))
                {
                    BuildDir(key);
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

        public static T Get<T>(string key, T defaultValue)
        {
            T result;

            if (TryGet<T>(key, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }
        
        public static bool TryGet<T>(string key, out T obj)
        {
            key = key.Trim('/');
            key = ROOT_DIR + key;
            return RawTryGet<T>(key, out obj);
        }        

        private static void RawDeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            int i = key.TrimEnd('/').LastIndexOf('/');

            if (i >= 0)
            {
                string parent = key.Substring(0, i + 1);
                string child = key.Substring(i + 1, key.Length - i - 1);
                RemoveDir(parent, child);
            }
        }

        private static void RawDeleteDir(string dir)
        {
            List<string> indices = null;

            if (RawTryGet<List<string>>(dir, out indices))
            {
                foreach (var sub in indices)
                {
                    if (sub.EndsWith("/"))
                    {
                        RawDeleteDir(dir + sub);
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey(dir + sub);
                    }
                }
            }

            PlayerPrefs.DeleteKey(dir);
        }

        private static bool RawTryGet<T>(string key, out T obj)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                obj = default(T);
                return false;
            }

            string str = PlayerPrefs.GetString(key, "");
            return Serializer.TryDeserialize<T>(str, out obj);
        }

        private static bool RawTrySet<T>(string key, T obj)
        {
            string str;

            if (Serializer.TrySerialize<T>(obj, out str))
            {
                PlayerPrefs.SetString(key, str);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool AddDir(string parentDir, string child)
        {
            List<string> indices = null;

            if (RawTryGet<List<string>>(parentDir, out indices))
            {
                if (!indices.Contains(child))
                {
                    indices.Add(child);
                    RawTrySet<List<string>>(parentDir, indices);
                }

                return true;
            }
            else 
            {
                indices = new List<string>();
                indices.Add(child);
                RawTrySet<List<string>>(parentDir, indices);
                return false;
            }
        }

        private static bool RemoveDir(string parentDir, string child)
        {
            List<string> indices = null;

            if (RawTryGet<List<string>>(parentDir, out indices))
            {
                if (indices.Remove(child))
                {
                    RawTrySet<List<string>>(parentDir, indices);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static void BuildDir(string key)
        {
            string temp = key.TrimEnd('/');
            int i = temp.LastIndexOf('/');

            if (i < 0)
            {
                return;
            }

            string parent = key.Substring(0, i + 1);
            string child = key.Substring(i + 1, key.Length - i - 1);

            if (!AddDir(parent, child))
            {
                BuildDir(parent);
            }
        }
    }
}