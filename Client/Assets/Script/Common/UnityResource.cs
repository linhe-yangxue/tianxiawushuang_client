using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Game
{
	class UnityResource : Resource
	{
        public string mName;

        public override string Type()
        {
            return "UNITY";
            //throw new NotImplementedException();
        }

        public override void SetResourceName(string name)
        {
            string temp = name.ToLower();
            char[] p = { '/' };
            string[] str = temp.Split(p);
            if (str[0]=="resources")
                temp = name.Substring(10, name.Length-10);
			string path = Path.GetDirectoryName(temp);
            mName = path + "/" + Path.GetFileNameWithoutExtension(temp);
        }

        public override DataBuffer ReadData()
        {
            UnityEngine.Object obj = Resources.Load(mName, typeof(TextAsset));
            if (obj == null)
                return (DataBuffer)null;

            TextAsset textAsset = obj as TextAsset;
            if (textAsset == null)
                return null;

            DataBuffer data = new DataBuffer();
            data._setData(textAsset.bytes);
            //Resources.UnloadUnusedAssets();
            return data;
        }

        public override bool WriteData(DataBuffer data, int nBeginPos, int writeCount)
        {
            throw new Exception("Error: not use unity resource write data");
        }
	}
}
