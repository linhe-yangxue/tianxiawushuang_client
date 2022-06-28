using UnityEngine;
using System.Collections;
using System.IO;
using DataTable;
using System.Collections.Generic;
using DataTable;
using System.Text;


public class FileMatcher : MonoBehaviour {

	bool test = true;

	string path;
	string pixPatttern = @"*_*.meta";
	public int fileCount;
	public int needUpdateZipCount;
	public Dictionary<int, DataRecord> allRemoteRecords;
	public Dictionary<int, DataRecord> allLocalRecords;
	public List<string> updateList = new List<string>();

	void OnGUI() {
		if(GUI.Button(new Rect(50, 30, 100, 50), "LoadConfig")) {
			FileStream fs = File.Open(Application.dataPath + "/Export/SourceFileMd5.csv", FileMode.OpenOrCreate);
			NiceTable t = new NiceTable();
			t.LoadTable(fs, Encoding.Unicode);
			foreach (KeyValuePair<int, DataRecord> vRe in t.GetAllRecord())
			{
				DataRecord r = vRe.Value;
				string fileName = Path.GetFileName(r["NAME"]);
				print(fileName + "!!!" + r["MD5"]);

			}

			string [] ffs = GetLocalFileNameList(path);
			print(ffs.Length);
			string rpath = ffs[3].Replace(".meta", "");
			FileStream qfs = File.Open(Application.streamingAssetsPath + @"/Sounds/attack_type1_light_78D856087779807D6DCF5F1E449256DD.a", FileMode.Open);
			int len = (int)qfs.Length;
			DataBuffer d = new DataBuffer(len);
			qfs.Read(d.mData, 0, len);
			string mm5 = GameCommon.MakeMD5(d.mData, len);
			print("testmd5:" + mm5);
			string md5 = qfs.GetMD5();
			bool b = qfs.CanRead;
			//print("TableCount:" + t.GetRecordCount());B1710F95B8DC129C14C758EEDC84555D
			// src md5 E4F10BCA153B53543511E6DA549B7EA7
			// 44f9fa4200393739ebd2983e063f93c0
		}
	}

	void Start() {
		path = test ? Application.dataPath + @"/zip/" : Application.persistentDataPath + "/wsxsm/";
	}

	public string[] GetLocalFileNameList(string path) {
		bool existsDir = Directory.Exists(path);
		if(!existsDir) {
			DEBUG.Log("Directory can't find," + path);
			return null;
		}

		string[] fileNames = Directory.GetFiles(path, pixPatttern, SearchOption.AllDirectories);
		fileCount = fileNames.Length;
		if(fileCount > 0) return fileNames;
		else return null;
	}


	public void MatchRemoteFileMD5(string fileName) {
		int remoteFileLength = allRemoteRecords.Count;
		foreach (KeyValuePair<int, DataRecord> vRe in allRemoteRecords)
		{
			DataRecord r = vRe.Value;
			string remoteFileName = (string)r["NAME"];
			if(remoteFileName == fileName) {
				string lmd5 = GetLocalFileMD5ByName(fileName);
				string rmd5 = (string)r["MD5"];
				if(lmd5 != rmd5) {
					string durl = (string)r["DOWNLOADURL"];
					updateList.Add(durl);
					needUpdateZipCount ++;
					break;
				} else {
					DEBUG.Log("file:" + remoteFileName + " wouldn't need update.");
				}
			}
		}
	}

	public string  GetLocalFileMD5ByName(string localFileName) {
		foreach(KeyValuePair<int, DataRecord> vRe in allLocalRecords) {
			DataRecord r = vRe.Value;
			string fileName = (string)r["NAME"];
			if(fileName.Equals(localFileName)) {
				string md5 = (string)r["MD5"];
				return md5;
			}
		}
		return string.Empty;
	}

}
