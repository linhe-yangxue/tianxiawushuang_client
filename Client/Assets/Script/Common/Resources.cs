using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Game
{
	public abstract class Resource
	{
        public abstract string Type();
        public abstract void SetResourceName(string name);
        public abstract DataBuffer ReadData();
        public abstract bool WriteData(DataBuffer data, int nBeginPos, int writeCount);
	}

    public class FileResource : Resource
    {
        public string mPathFileName;

        public override string Type()
        {
            return "FILE";
            //throw new NotImplementedException();
        }

        public override void SetResourceName(string name)
        {
            mPathFileName = name;
            //throw new NotImplementedException();
        }

        public override DataBuffer ReadData()
        {
            string pathFile = GameCommon.MakeGamePathFileName(mPathFileName);
            if (!File.Exists(pathFile))
                return (DataBuffer)null;
            FileStream fs = new FileStream(pathFile, FileMode.Open);
            DataBuffer data = new DataBuffer((int)fs.Length);

            if (fs.Read(data.getData(), 0, (int)fs.Length) != fs.Length)
                data = null;

            fs.Close();

            return data;
            //throw new NotImplementedException();
        }

        public override bool WriteData(DataBuffer data, int nBeginPos, int writeCount)
        {
            int len = data.getData().Length;
            if (len - nBeginPos < writeCount)
                return false;

            string pathFile = GameCommon.MakeGamePathFileName(mPathFileName);
            if (File.Exists(pathFile))
                File.Delete(pathFile);

            FileStream fs = new FileStream(pathFile, FileMode.CreateNew);
            fs.Write(data.getData(), nBeginPos, writeCount);

            fs.Close();

            return true;
            //throw new NotImplementedException();
        }
    }
}
