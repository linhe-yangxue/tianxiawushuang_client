using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DataTable;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Core;

public class WWWRequester : Singleton<WWWRequester> {

	public enum RequestType {
		Text,
		AssetBundles,
		Zip
	}

	RequestType rt;

	float retryTime = 3f;
	System.Action<object> CacheCallback;

	void Request(string url, System.Action<object> RequestCallback) {
		StartCoroutine(Request(url, RequestCallback, RequestType.Text));
	}

	IEnumerator Request(string url, System.Action<object> RequestCallback, RequestType rt) {
		CacheCallback = RequestCallback;
		WWW http = new WWW(url);
		http.threadPriority = ThreadPriority.Normal;
		yield return http;
		string err = http.error;
		if(err == null && http.isDone) {
			switch(rt) {
			case RequestType.Text:
				RequestCallback(http.text);
				break;
			case RequestType.AssetBundles:
				RequestCallback(http.assetBundle);
				break;
			case RequestType.Zip:
				RequestCallback(http.bytes);
				break;
			}
		} else {
			// request err
			DEBUG.LogError("Request URL:" + url + " err. err msg:" + err + "Auto retry request.");
			Invoke("Retry", retryTime);
		}

	}

	void Retry() {
		GetSvrList(CacheCallback);
	}

	public void GetSvrList(System.Action<object> OnRequestComplete) {
		CacheCallback = null;
		Request("http://xsmtest.gdegame.com/login/serverlist.csv", OnRequestComplete);
		print("request one time");
	}

	void ZipRawData() {
		// request -> download -> unzip -> loading
		// need update -> filedownloadlist.plist -> download zip files -> 

	}

	// single file unzip
	bool UnZipFile(string filePath) {

		FileStream fs = File.OpenRead(filePath);
		using (ZipInputStream s = new ZipInputStream(fs)) {

			ZipEntry theEntry;
			while((theEntry = s.GetNextEntry()) != null) {
				DEBUG.Log(theEntry.Name);

				string directoryName = Path.GetDirectoryName(theEntry.Name);
				string fileName = Path.GetFileName(theEntry.Name);

				// create directory
				if(directoryName.Length > 0) {
					Directory.CreateDirectory(Application.dataPath + "/" + directoryName);
				}

				if(fileName != string.Empty) {
					using(FileStream streamWriter = File.Create(Application.dataPath + "/" + theEntry.Name)) {
						int size = 2048;
						byte[] data = new byte[size];
						while(true) {
							size = s.Read(data, 0, data.Length);
							if(size > 0) {
								streamWriter.Write(data, 0, size);
							}else {
								break;
							}
						}
						return true;
					}
				}

			}
			return false;
		}
	}

	// mulit file unzip
	void UseFastZipHandle() {
		FastZipEvents fze = new FastZipEvents();
		fze.Progress += ProgressHandler1;
		fze.CompletedFile += CompletedFileHandler1;
		fze.FileFailure += FileFailureHandler1;

		FastZip fz = new FastZip(fze);

		fz.ExtractZip(Application.dataPath + @"/zip/video.zip", Application.dataPath+"/zip", string.Empty);
	}

	void ProgressHandler1(object sender, ProgressEventArgs e) {
		print("正在解压文件：" + e.Name + "  (" + e.PercentComplete + ")");
	}

	void CompletedFileHandler1(object sender, ScanEventArgs e) {
		print("解压文件：" + e.Name + "成功！");
	}

	void FileFailureHandler1(object sender, ScanFailureEventArgs e) {
		print("解压文件：" + e.Name + "发生错误！");
	}
}
