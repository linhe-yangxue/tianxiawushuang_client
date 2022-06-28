using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using DataTable;


public class ResourcesPack
{
    
	public ResourcesPack(){}

    ~ResourcesPack()
    {
        close();
    }

    public bool load()
    {
        if (mPackDataStream == null)
            return false;
        try
        {
            if (mPackDataStream.Seek(mPackDataStream.Length-sizeof(int), SeekOrigin.Begin)<0)
                return false;

            byte[] val = new byte[sizeof(int)];
            if (mPackDataStream.Read(val, 0, sizeof(int)) != sizeof(int))
                return false;

            int indexPos = BitConverter.ToInt32(val, 0);
            int size = (int)(mPackDataStream.Length-sizeof(int)-indexPos);
            if (size<=0)
                return false;


            DataBuffer d = new DataBuffer(size);

			if (mPackDataStream.Seek((long)indexPos, SeekOrigin.Begin)<0)
				return false;
            if (mPackDataStream.Read(d.mData, 0, size)!=size)
                return false;

            mResourceIndex = new NiceTable();
            if (!mResourceIndex.restore(ref d))
                return false;
            //mResourceIndex.SaveTable("d:/test_read_pack.csv");
           return true;

        }
        catch
        {
            DEBUG.LogError("Init Read from pack error");
            return false;
        }
		return true;
    }

    public void close()
    {
        if (mPackDataStream != null)
            mPackDataStream.Close();
        mPackDataStream = null;
    }

	public bool load( string packFileName, bool isStrict )
    {
        mPackFileName = packFileName;
        try
        {
            mPackDataStream = new FileStream(mPackFileName, FileMode.Open);
        }
        catch
        {
			if(isStrict)
            	DEBUG.LogError("Resources pack is not exist > " + mPackFileName);
            return false;
        }
		return load();
	}

    public DataBuffer loadResource(string pathFilename)
	{
		string szPathFilename = pathFilename.ToLower();
        szPathFilename.Replace('\\', '/');
        DataRecord r = mResourceIndex.GetRecord(szPathFilename);
        if (r != null)
        {
            int p = r["POS"];
            int size = r["SIZE"];

            if (p >= 0 && size > 0)
            {
				if (mPackDataStream.Seek((long)p, SeekOrigin.Begin)>=0)
				{
                	DataBuffer d = new DataBuffer(size);
                	if (mPackDataStream.Read(d.mData, 0, size) == size)
                    	return d;
				}
            }
        }

		return null;
	}

    public int Export(string destPath)
    {
        int resultCount = 0;
        foreach (KeyValuePair<int, DataRecord> vRe in mResourceIndex.GetAllRecord())
        {
            string fieldIndex = (string)vRe.Value.get(0);
            DataBuffer d = loadResource(fieldIndex);
            if (d != null)
            {
                FileStream f = new FileStream(destPath + fieldIndex, FileMode.Create);
                f.Write(d.mData, 0, d.size());
                f.Close();

                ++resultCount;
            }
        }
        return resultCount;
    }

	static bool _EncryptData(DataBuffer scrData, int length){ return true; }
	static bool _DecryptData(DataBuffer scrData, int length){ return true; }


	string			    mPackFileName;
	Stream		        mPackDataStream;
	public NiceTable	mResourceIndex;
}
