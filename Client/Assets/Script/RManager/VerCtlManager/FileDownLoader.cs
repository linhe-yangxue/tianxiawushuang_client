using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Assets.Script.RManager;
using System.Xml;
using System.Xml.Linq;
using System;
using FileDownLoad;
using VersionControl;

namespace FileDownLoad
{
    public interface IFileReceiver
    {
        void StartReceive(string fileLRI, long length, string MD5);

        long GetCurrentBlockOffset();
        void AcceptReceive(byte[] bytes, int offet, int len);

        bool EndReceive(string fileMD5, long fileSize);
    }

    public class FileDownLoadInfo
    {
        public string Theater {get; set;}
        public string FileLRI {get; set;}   // related to theater path, with file name
        public int    Revision {get; set;} 
        public string FileMD5 { get; set;}
        public long   FileSize { get; set;}
        public bool   IsDownloaded {get; set;}

        public DateTime DownLoadStart {get; set;}
        public DateTime DownLoadEnd {get; set;}
        public IFileReceiver FileReceiver {get; set;}

        public FileDownLoadInfo()
        {
            FileLRI = "";
            Revision = -1;
            FileMD5 = "";
            FileSize = 0;
            IsDownloaded = false;
            FileReceiver = null;
        }
    }
}

public class FileDownLoader 
{
    bool  mIsInBussy = false;
    bool  mIsAborted = false;
    string mCurrentDownLoading = "";
    string mAbortReason = "";
    
 #region Properties
    public string ServerIP { get; set; }
    public int ServerPort { get; set; }

    public bool IsDownLoading { get { return mIsInBussy; } }
    public bool IsAborted { get {return mIsAborted; }}
    public string AbortReason { get {return mAbortReason; }}
    public string FilerServerEntryToken { get; set; }

    public TcpClient NetClient { get; set; }
    public NetPacket NetProtocol { get; set; }

    Dictionary<string, FileDownLoadInfo> AllFilesTobeDownLoad { get; set; }

    public float CurrentProgress { get; private set; }

    public int   Completed { get; internal set; }
    public int   Total     { get; internal set; }

    public FileDownLoadInfo CurrentDownloading
    {
        get
        {
            if (AllFilesTobeDownLoad.ContainsKey(mCurrentDownLoading))
                return AllFilesTobeDownLoad[mCurrentDownLoading];
            else
                return null;
        }
    }

 #endregion

    /// <summary>
    /// FileDownLoader ctor
    /// </summary>
    public FileDownLoader()
    {
        mIsInBussy = false;
        ServerIP = "";
        ServerPort = 0;
        Completed = 0;
        Total = 0;

        AllFilesTobeDownLoad = new Dictionary<string, FileDownLoadInfo>();

        CurrentProgress = 0.0f;
    }

#region NetLogger used in file downloading
    class NetLogger : ILogger
    {
        FileDownLoader mCtx;
        public NetLogger(FileDownLoader downloadCtx)
        {
            mCtx = downloadCtx;
        }

        public void Log(LogLevel lgl, string log)
        {
            mCtx.InformMessage(MessageInformType.MSG_LOG, log);
        }

    }
#endregion

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="message"></param>
    public void InformMessage(MessageInformType msgType, string message)
    {
        ;
    }

    public void InformMessage(MessageInformType msgType, int msgId)
    {
        ;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="download"></param>
    /// <returns></returns>
    public bool AddToDownLoad(FileDownLoadInfo download)
    {
        if(mIsInBussy)
            return false;

        AllFilesTobeDownLoad[download.FileLRI] = download;

        return true;
    }

    public void ClearAllDownloadInfo()
    {
        AllFilesTobeDownLoad.Clear();
    }

    public string BuildFileDownLoadList()
    {
        // TODO : use XML format and Utf8 encoding
        string result = "";
        foreach(var kv in AllFilesTobeDownLoad)
        {
            if(kv.Value.IsDownloaded)
                continue;

            result += string.Format("{0}:{1};", kv.Value.FileLRI, kv.Value.Revision);
        }

        return result;
    }

    public bool StartDownLoad()
    {
        if(mIsInBussy)
            return true;

        mIsInBussy = true;
        mIsAborted = false;

        CurrentProgress = 0.0f;

        if(AllFilesTobeDownLoad.Count <= 0)
        {
            CurrentProgress = 1.0f;
            mIsInBussy = false;
            return true;
        }

        Thread downloadThread = new Thread(BackWorkThread);
        downloadThread.Start(this);
        return true;
    }

    public void AbortDownload(string reason)
    {
        mIsAborted = true;
        mAbortReason = reason;
        EndDownLoad();
    }

    public void EndDownLoad()
    {
        mIsInBussy = false;
    }

    class LambdaCaptureHelp
    {
        public long tick;
        public long read;
        public long readTemp;
        public long total;
    };
    static void BackWorkThread(object ctx)
    {
        FileDownLoader downLoadCtx = ctx as FileDownLoader;

        if(downLoadCtx.AllFilesTobeDownLoad.Count <= 0)
        {
            downLoadCtx.CurrentProgress = 1.0f;
            downLoadCtx.EndDownLoad();
            return;
        }

        var tcpClient = downLoadCtx.NetClient;

        if (tcpClient == null)
        {
            downLoadCtx.CurrentProgress = 1.0f;
            downLoadCtx.AbortDownload("server not connected.");
            return;
        }

        try
        {
            var netProtocol = downLoadCtx.NetProtocol;

            var stream = tcpClient.GetStream();
            {
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream);

                // 先获取所有文件的字节大小
                string fileListStr = downLoadCtx.BuildFileDownLoadList();
                var theater = "";
                foreach (var v in downLoadCtx.AllFilesTobeDownLoad.Values)
                {
                    theater = v.Theater;
                    break;
                }

                NetPacketHelper.WriteAsPacketToStream(stream, string.Format("GetSize]:{0}>:{1}", theater, fileListStr));
                stream.Flush();

                var receivedPackets = new List<NetPacketHelper.PacketReadResult>();
                var packetBuffer = new NetPacketHelper.PacketBuffer
                    {
                        bodyBuff = new byte[10 * 1024 * 1024 + 128],
                        headBuff = new byte[128],
                    };

                var netWait = new NetWaiting();
                long tick = 0;

                netWait.OnNetWaiting = () => { tick++; };

                var coroutine = NetPacketHelper.ReceiveOnePacket(tcpClient, netProtocol, stream, packetBuffer, netWait, receivedPackets);
                while(coroutine.MoveNext()) 
                    ;

                if (receivedPackets.Count <= 0)
                {
                    downLoadCtx.InformMessage(
                        MessageInformType.MSG_ERROR,
                        "failed to receive file size info from  server");
                    downLoadCtx.AbortDownload("failed to receive file  size info from  server");

                    packetBuffer = null;
                    return;
                }

                var result = receivedPackets[0];
                if(result.exception != null || !string.IsNullOrEmpty(result.errorMsg))
                {
                    var msg = !string.IsNullOrEmpty(result.errorMsg) ? result.errorMsg : (result.exception != null ? result.exception.Message : "");
                    downLoadCtx.InformMessage(
                        MessageInformType.MSG_ERROR,
                        "failed to receive file from server :" + msg);

                    downLoadCtx.AbortDownload("failed to receive file from server :" + msg);
                    packetBuffer = null;
                    return;
                }

                var receivedPacket = receivedPackets[0].packet as Packet_FileSize;
                if(receivedPacket == null)
                {
                    downLoadCtx.InformMessage(
                        MessageInformType.MSG_ERROR,
                        "failed to receive file size from server" );
                    downLoadCtx.AbortDownload("failed to receive file size from server ");
                    packetBuffer = null;
                    return;
                }

                try
                {
                    var packSize = receivedPacket.TotalSize;
                    // 下载所有文件
                    var totalReaded = 0.0f;
                    downLoadCtx.Total = downLoadCtx.AllFilesTobeDownLoad.Count;
                    downLoadCtx.Completed = 0;

                    var cccc = new LambdaCaptureHelp();
                    netWait.OnNetWaiting = ()        => { cccc.tick++; };
                    netWait.OnReceived   = (int len) =>
                    {
                        cccc.readTemp += len;
                        var prg = (float)(cccc.read + cccc.readTemp) / (float)cccc.total;
                        if (prg > 1.0f)
                            prg = 1.0f;
                        if (prg > downLoadCtx.CurrentProgress)
                            downLoadCtx.CurrentProgress = prg;
                    };

                    foreach (var kv in downLoadCtx.AllFilesTobeDownLoad)
                    {
                        var download = kv.Value;

                        if (download.IsDownloaded)
                        {
                            downLoadCtx.Completed++;
                            continue;
                        }

                        download.DownLoadStart = DateTime.Now;
                        var left = receivedPacket.AllFileSize.ContainsKey(download.FileLRI) ? receivedPacket.AllFileSize[download.FileLRI] : 0L;//download.FileSize;\

                        if(left <= 0)
                        {
                            downLoadCtx.InformMessage(MessageInformType.MSG_ERROR, "Error retrieving file information from server!");
                            downLoadCtx.AbortDownload("Unresolve reources : " + download.FileLRI);
                            packetBuffer = null;
                            return;
                        }

                        var fileSize = (float)left;
                        var fileRead = 0.0f;
                        downLoadCtx.CurrentProgress = 0.0f;

                        download.FileReceiver.StartReceive(download.FileLRI, left, download.FileMD5);
                        downLoadCtx.mCurrentDownLoading = download.FileLRI;

                        var offset = download.FileReceiver.GetCurrentBlockOffset();

                        cccc.tick = 0;
                        cccc.read = 0;
                        cccc.total = left;

                        do
                        {
                        REQUEST_BLOCK:
                            // 给服务器发送请求
                            var query = string.Format("resource]:{0}>:{1}?rev={2}&off={3}", theater, download.FileLRI, download.Revision, offset);
                            NetPacketHelper.WriteAsPacketToStream(stream, query);

                            var timeOut = DateTime.Now;
                            int count = 0;

                            receivedPackets.Clear();
                            timeOut = DateTime.Now;

                            cccc.tick = 0;
                            cccc.readTemp = 0;
                            coroutine = NetPacketHelper.ReceiveOnePacket(tcpClient, netProtocol, stream, packetBuffer, netWait, receivedPackets);
                            coroutine.MoveNext();

                            while(coroutine.MoveNext()) 
                            {
                                if(count++ > 1000)
                                {
                                    var now = DateTime.Now;
                                    if((now - timeOut).Seconds > 6 && receivedPackets.Count <= 0)
                                    {
                                        //throw new Exception("net work time out! ");
                                        //goto REQUEST_BLOCK;
                                    }

                                    Thread.Sleep(20);
                                }
                            }

                            if (receivedPackets.Count <= 0 || receivedPackets[0].packet == null)
                            {
                                var msg = "unkown";
                                if (receivedPackets.Count > 0)
                                {
                                    result = receivedPackets[0];
                                    msg = result.exception != null ? (result.exception.Message + "\r\n" + result.exception.StackTrace) : (!string.IsNullOrEmpty(result.errorMsg) ? result.errorMsg : "");
                                }
                                downLoadCtx.InformMessage(
                                MessageInformType.MSG_ERROR,
                                "failed to receive file block from server :" + msg);

                                downLoadCtx.AbortDownload("failed to receive file block from server :" + msg);
                                packetBuffer = null;
                                return;
                            }

                            var fileBlock = receivedPackets[0].packet as Packet_FileDataBlock;
                            if(fileBlock == null)
                            {
                                downLoadCtx.InformMessage(
                                    MessageInformType.MSG_ERROR,
                                    "failed to receive file block from server " );
                                downLoadCtx.AbortDownload("failed to receive file block from server ");
                                packetBuffer = null;
                                return;
                            }

                            var readed = fileBlock.BlockData.length;
                            left -= readed;
                            totalReaded += (float)readed;
                            fileRead += (float)readed;
                            cccc.read += readed;
                            cccc.readTemp = 0;

                            download.FileReceiver.AcceptReceive(fileBlock.BlockData.buff, fileBlock.BlockData.offset, readed);

                            // update progress
                            var newProgress = fileRead / fileSize;
                            if(newProgress > 1.0f)
                                newProgress = 1.0f;
                            if(newProgress > downLoadCtx.CurrentProgress)
                                downLoadCtx.CurrentProgress = newProgress;

                            offset += readed;

                        } while (left > 0L);

                        download.DownLoadEnd = DateTime.Now;
                        download.IsDownloaded = download.FileReceiver.EndReceive(download.FileMD5, download.FileSize);
                        downLoadCtx.Completed++;
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg  = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
                    downLoadCtx.InformMessage(MessageInformType.MSG_ERROR, "Error retrieving file from server!");
                    downLoadCtx.AbortDownload("Error retrieving file from server! " + errorMsg);
                    packetBuffer = null;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg  = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
            downLoadCtx.CurrentProgress = 1.0f;
            downLoadCtx.InformMessage(MessageInformType.MSG_ERROR, "Server connect abordted! " + errorMsg);
            downLoadCtx.AbortDownload("Server connect abordted! " + errorMsg);
            //packetBuffer = null;
            return;
        }

        downLoadCtx.CurrentProgress = 1.0f;
        downLoadCtx.EndDownLoad();
        //packetBuffer = null;
    }
}
