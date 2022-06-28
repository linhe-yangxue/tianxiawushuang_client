using UnityEngine;
using System.Collections;
using VersionControl;
using FileDownLoad;
using Assets.Script.RManager;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using Common;

namespace VersionControl
{
    public enum UPDATE_TYPE
    {
        UPT_NON = -1,
        UPT_ADD = 0,
        UPT_MOD = 1,
        UPT_DEL = 2
    }

    public interface IMessageInformer
    {
        void InformMessage(MessageInformType msgType, string message);
    }

    public interface IVersionControlContext : IMessageInformer
    {
        //void InformMessage(MessageInformType msgType, string message);
        void SetLoadingTitle(string title);
        void SetLoadingHint(string hint);
        void InformProgress(string hint, float progress);
        void InformPreProgress(string hint, float progress);

        Coroutine StartCoroutine(IEnumerator coroutine);
    }

    public class DefaultVerCtlCtx : IVersionControlContext
    {
        public void InformMessage(MessageInformType msgType, string message)
        {
            if(msgType == MessageInformType.MSG_INFO)
                DEBUG.Log(message);
            else if(msgType == MessageInformType.MSG_ERROR)
                DEBUG.LogError(message);
            else if(msgType == MessageInformType.MSG_WARN)
                DEBUG.LogWarning(message);
            else 
                DEBUG.Log(message);
        }

        public virtual void SetLoadingTitle(string title)
        {
            HotUpdateLoadingUI.Self.SetLoadingName(title);  
        }

        public virtual void SetLoadingHint(string hint)
        {
            HotUpdateLoadingUI.Self.SetLoadingDescription(hint);
        }

        public virtual void InformProgress(string hint, float progress)
        {
            if(!string.IsNullOrEmpty(hint))
                SetLoadingHint(hint);
            HotUpdateLoadingUI.Self.SetProgress(progress);
            //DEBUG.Log(hint);
        }

        public virtual void InformPreProgress(string hint, float progress)
        {
            if(!string.IsNullOrEmpty(hint))
                SetLoadingHint(hint);
            HotUpdateLoadingUI.Self.SetProgress(progress);
        }

        public virtual Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return null;
        }
    }

    public class VerCtlUpdateInfo
    {
        public int    ForRevision {get; set;}
        public string FileLRI { get; set;}
        public string NewRevMD5 { get; set;}
        public UPDATE_TYPE UpdateType { get; set;}
        public string DownLoadedFilePath {get; set;}

        public VerCtlUpdateInfo()
        {
            ForRevision = 0;
            FileLRI = "";
            NewRevMD5 = "";
            UpdateType = UPDATE_TYPE.UPT_NON;
            DownLoadedFilePath = null;
        }
    }

    public class VerCtlCheckInfo
    {
        public string Theater {get; set;}
        public int FromRev {get; set;}
        public int ToRev   {get; set;}

        public Dictionary<string, VerCtlUpdateInfo> Updates {get; set;}

        public void DisposeUpdates()
        {
            // TODO : delete downloaded temp file
            if(Updates == null)
                return;

            Updates.Clear();
        }
    }

    public class DownloadedFileReciever : IFileReceiver
    {
        VerCtlUpdateInfo mUpdateInfo;
        FileStream       mTempFile;
        long             mExpectedFileSize;
        string           mExpectedMD5;
        long             mCurrentFileOffset;
 
        public DownloadedFileReciever(VerCtlUpdateInfo origineUpdateInfo, string receivePath)
        {
            mUpdateInfo = origineUpdateInfo;

            var tempPath = Path.Combine(receivePath, origineUpdateInfo.FileLRI);
            origineUpdateInfo.DownLoadedFilePath = tempPath;
        }

        public void StartReceive(string fileLRI, long length, string MD5)
        {
            //throw new NotImplementedException();
            if(fileLRI != mUpdateInfo.FileLRI || mExpectedFileSize < 0)
                throw new Exception("fatal error: not exptected resource!!!");

            if (mTempFile != null)
                return;

            mExpectedMD5 = MD5;
            mExpectedFileSize = length;

            {
                mTempFile = U3DFileIO.CreateFile(mUpdateInfo.DownLoadedFilePath);
                if (mTempFile == null)
                    throw new Exception("error: failed to create download file.");

                mTempFile.SetLength(mExpectedFileSize);
                mTempFile.Seek(0, SeekOrigin.Begin);
            }
        }

        public long GetCurrentBlockOffset()
        {
            return mTempFile.Position;
        }

        public void AcceptReceive(byte[] bytes, int offset, int len)
        {
            //throw new NotImplementedException();
            mTempFile.Write(bytes, offset, len);
        }

        public bool EndReceive(string fileMD5, long fileSize)
        {
            //throw new NotImplementedException();
            mTempFile.Flush();
            mTempFile.Close();  
            return true;
        }
    }
}

public class VersionControlManager
{
#region Helper contexts
    class NetLogger : ILogger
    {
        IVersionControlContext mCtx;
        public NetLogger(IVersionControlContext ctx)
        {
            mCtx = ctx;
        }

        public void Log(LogLevel lgl, string log)
        {
            if(mCtx != null)
            {
                mCtx.InformMessage(MessageInformType.MSG_LOG, log);
            }
        }
    }
    class PacketContext : IPacketContext
    {
        IVersionControlContext mCtx;
        public PacketContext(IVersionControlContext ctx)
        {
            mCtx = ctx;
        }

        public void Log(LogLevel lgl, string log)
        {
            if(mCtx != null)
            {
                mCtx.InformMessage(MessageInformType.MSG_LOG, log);
            }
        }
    }
#endregion
    const string VersionControlFolder = "VerCtl/";
    const string VerCtledResourceBase = "Resource";
    const int    MaxTryConnectCount = 1;

    static string TheaterDefault = CommonParam.ResourceTheater;

#region fields
    string  mServerIP;
    int     mServerPort;
    string  mFileServerEntryToken;
    bool    mIsAuthorized;

    // 网络组件
    TcpClient   mNetClient;
    NetPacket   mNetProtocol;

    string  mTheater;

    // 当前版本( < 0 则尚未建立任何版本的资源)
    int     mCurrentRevision;

    // 如果正在更新，则为更新目标版本
    int     mTargetRevision;

    // 如果是强制更新到某个版本，则设置此值；否则， 此值为-1， 表示更新到 Head revision
    int     mSetedTargetRevision;

    // 资源的最新版本； 每次询问服务器时更新
    int     mHeadRevision;

    VerCtlCheckInfo mCheckedInfo;

    // 资源文件下载器
    FileDownLoader mFileDownLoader;

    // 其它
    IVersionControlContext mVerCtlContext;
    float          mLastProgressTime;
    float          mLastProgess;
#endregion

#region properties
    public IVersionControlContext VerCtlContext { get { return mVerCtlContext; } set { mVerCtlContext = value;} }

    public string Theater { get{ return mTheater; } set { mTheater = value; } }
    public string ResourceBaseDir { get; internal set; }
#endregion
    /// <summary>
    /// ctor
    /// </summary>
    public VersionControlManager()
    {
        TheaterDefault = CommonParam.ResourceTheater;

        mFileServerEntryToken = "";
        mIsAuthorized = false;
        mCurrentRevision = -1;
        mTargetRevision = -1;
        mSetedTargetRevision = -1;
        mHeadRevision = -1;
        mTheater = TheaterDefault;
        mCheckedInfo = new VerCtlCheckInfo();
        mFileDownLoader = new FileDownLoader(); 
        mVerCtlContext = new DefaultVerCtlCtx();
        ResourceBaseDir = VerCtledResourceBase;
        mNetClient =  new TcpClient();
        mNetProtocol = new NetPacket(new NetLogger(mVerCtlContext));

        if(!U3DFileIO.Exists(VersionControlFolder))
        {
            bool isCreated = U3DFileIO.CreateFolder(VersionControlFolder);
            if(!isCreated)
                U3DFileIO.DeleteFile(VersionControlFolder.TrimEnd('/'));

            isCreated = U3DFileIO.CreateFolder(VersionControlFolder);
        }

        IPacketHandler packetPrototype = new PacketHandler_HandShake();
        mNetProtocol.RegisterPacketHandler(packetPrototype.ExpectedName, packetPrototype);
        packetPrototype = new PacketHandler_Update();
        mNetProtocol.RegisterPacketHandler(packetPrototype.ExpectedName, packetPrototype);
        packetPrototype = new Packet_FileSize();
        mNetProtocol.RegisterPacketHandler(packetPrototype.ExpectedName, packetPrototype);
        packetPrototype = new Packet_FileDataBlock();
        mNetProtocol.RegisterPacketHandler(packetPrototype.ExpectedName, packetPrototype);

        //LoadLocalVerCtlInfo();
    }

    public void SetTCPServer(string IP, int Port)
    {
        mServerIP = IP;
        mServerPort = Port;
    }

    void RemoveTempResourceItem(string filePath)
    {
        if(U3DFileIO.Exists(filePath))
            U3DFileIO.DeleteFile(filePath);
    }

    /// <summary>
    /// 从本地加载更新信息
    /// </summary>
    void LoadUpdateInfo()
    {
        mCheckedInfo.DisposeUpdates();
        LoadLocalVerCtlInfo();
    }

    /// <summary>
    /// TODO : 将更新信息保存到本地，以支持 "断点" 续传
    /// </summary>
    void SaveUpdateInfo()
    {}

    /// <summary>
    /// 将服务器返回来的更新信息与本地原有的更新信息合并，并保存到文件系统中
    /// </summary>
    /// <param name="newUpdateInfo"></param>
    void MergeAndSaveUpdateInfo(Dictionary<string, VerCtlUpdateInfo> newUpdateInfo)
    {
        if (mCheckedInfo.Updates == null)
        {
            mCheckedInfo.Updates = new Dictionary<string, VerCtlUpdateInfo>(newUpdateInfo);
            mCheckedInfo.FromRev = mCurrentRevision;
            mCheckedInfo.ToRev = mTargetRevision;
            mCheckedInfo.Theater = mTheater;

            SaveUpdateInfo();
            return;
        }

        foreach(var kv in newUpdateInfo)
        {
            if(!mCheckedInfo.Updates.ContainsKey(kv.Key))
            {
                mCheckedInfo.Updates.Add(kv.Key, kv.Value);
                continue;
            }

            var old = mCheckedInfo.Updates[kv.Key];
            old.UpdateType = kv.Value.UpdateType;

            if(kv.Value.UpdateType == UPDATE_TYPE.UPT_DEL || kv.Value.UpdateType == UPDATE_TYPE.UPT_NON)
            {
                old.ForRevision = kv.Value.ForRevision;
                old.NewRevMD5 = kv.Value.NewRevMD5;
                continue;
            }

            if (old.ForRevision != kv.Value.ForRevision || old.NewRevMD5 != kv.Value.NewRevMD5)
            {
                old.ForRevision = kv.Value.ForRevision;

                if (old.NewRevMD5 != kv.Value.NewRevMD5)
                {
                    if (!string.IsNullOrEmpty(old.DownLoadedFilePath))
                    {
                        // dispose old downloaded
                        RemoveTempResourceItem(old.DownLoadedFilePath);
                    }

                    old.DownLoadedFilePath = null;
                    old.NewRevMD5 = kv.Value.NewRevMD5;
                }
            }
        }

        mCheckedInfo.FromRev = mCurrentRevision;
        mCheckedInfo.ToRev = mTargetRevision;
        mCheckedInfo.Theater = mTheater;

        SaveUpdateInfo();
    }

    bool CheckForDownload(Dictionary<string, VerCtlUpdateInfo> updateInfo)
    {
        var haveToDownload = false;
        foreach (var kv in updateInfo)
        {
            if (kv.Value.UpdateType == UPDATE_TYPE.UPT_DEL || kv.Value.UpdateType == UPDATE_TYPE.UPT_NON)
                continue;

            if(!CheckResourceItemNeedToDownLoad(kv.Value))
                continue;

            mFileDownLoader.AddToDownLoad(new FileDownLoadInfo
            {
                Theater = mTheater,
                FileLRI = kv.Value.FileLRI,
                FileMD5 = kv.Value.NewRevMD5,
                Revision = kv.Value.ForRevision,
                FileReceiver = new DownloadedFileReciever(kv.Value, "download")
            });

            haveToDownload = true;
        }
        return haveToDownload;
    }

    bool CheckResourceItemNeedToDownLoad(VerCtlUpdateInfo updateInfo)
    {
        if(string.IsNullOrEmpty(updateInfo.DownLoadedFilePath))
            return true;

        if(!U3DFileIO.Exists(updateInfo.DownLoadedFilePath))
        {
            updateInfo.DownLoadedFilePath = null;
            return true;
        }

        // TODO: check file MD5

        return false;
    }

    bool MakeBackup(string filePath)
    {
        var backup = filePath + ".bak";
        if(U3DFileIO.Exists(filePath))
            return U3DFileIO.MoveFile(filePath, backup);
        else
            return false;
    }

    void ReformFromBackup(string filePath)
    {
        var backup = filePath + ".bak";
        if (U3DFileIO.Exists(backup))
            U3DFileIO.MoveFile(backup, filePath);
    }

    void DisposeBackup(string filePath)
    {
        var backup = filePath + ".bak";
        if (U3DFileIO.Exists(backup))
            U3DFileIO.DeleteFile(backup);
    }

    void DisposeAllBackup(string baseDir)
    {
        var allBackups = U3DFileIO.GetFiles(baseDir, "*.bak", SearchOption.AllDirectories);
        foreach(var backup in allBackups)
        {
            var filePath = backup.Substring(0, backup.LastIndexOf(".bak"));
            var u3dFilePath = U3DFileIO.GetFileU3dPath(filePath);
            if(u3dFilePath == null || u3dFilePath == "")
                DisposeBackup(filePath);
            else
                DisposeBackup(u3dFilePath);
        }
    }

    void ReformAllBackup(string baseDir)
    {
        var allBackups = U3DFileIO.GetFiles(baseDir, "*.bak", SearchOption.AllDirectories);
        foreach(var backup in allBackups)
        {
            var filePath = backup.Substring(0, backup.LastIndexOf(".bak"));
            var u3dFilePath = U3DFileIO.GetFileU3dPath(filePath);
            if(u3dFilePath == null || u3dFilePath == "")
                ReformFromBackup(filePath);
            else
                ReformFromBackup(u3dFilePath);
        }
    }

    bool ImportNewResource(VerCtlUpdateInfo updateInfo)
    {
        var resourcePath = Path.Combine(ResourceBaseDir, updateInfo.FileLRI);
        var hasBackup = false;
        var result = false;

        if(U3DFileIO.Exists(resourcePath))
            hasBackup = MakeBackup(resourcePath);

        var resourceFolder = Path.GetDirectoryName(resourcePath);
        if(!U3DFileIO.Exists(resourceFolder))
            U3DFileIO.CreateFolder(resourceFolder);

        result = U3DFileIO.MoveFile(updateInfo.DownLoadedFilePath, resourcePath);
        if(!result && hasBackup)
            ReformFromBackup(resourcePath);
        //if(result && hasBackup)
        //    U3DFileIO.DeleteFile(backup);

        return result;
    }

    void DeleteOldResource(VerCtlUpdateInfo updateInfo)
    {
        var resourcePath = Path.Combine(VersionControlFolder, updateInfo.FileLRI);
        U3DFileIO.DeleteFile(resourcePath);
    }

    bool UpdateResourceItem(VerCtlUpdateInfo updateInfo)
    {
        mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, string.Format("updating game resource {0} : {1}", updateInfo.FileLRI, updateInfo.UpdateType));
        
        var result  = false;
        switch (updateInfo.UpdateType)
        {
            case UPDATE_TYPE.UPT_MOD:
            case UPDATE_TYPE.UPT_ADD:
                result = ImportNewResource(updateInfo);
                break;
            case UPDATE_TYPE.UPT_DEL:
                DeleteOldResource(updateInfo);
                break;
            default:
                break;
        }

        if(!result)
            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, string.Format("updating game resource {0} : {1} Failed!", updateInfo.FileLRI, updateInfo.UpdateType));
        return result;
    }

    void RemoveTempDownload(VerCtlUpdateInfo updateInfo)
    {
        if(updateInfo.DownLoadedFilePath != null 
            && updateInfo.DownLoadedFilePath != "" 
            && U3DFileIO.Exists(updateInfo.DownLoadedFilePath))
        {
            U3DFileIO.DeleteFile(updateInfo.DownLoadedFilePath);
        }

        updateInfo.DownLoadedFilePath = null;
    }

    void RemoveUpdateInfo(string key)
    {
        mCheckedInfo.Updates.Remove(key);
    }

    /// <summary>
    /// 发布更新到资源目录
    /// </summary>
    void DistributeUpdatas()
    {
        if (mCheckedInfo.Updates == null || mCheckedInfo.Updates.Count <= 0)
        {
            if (mCurrentRevision != mCheckedInfo.ToRev)
            {
                mCurrentRevision = mCheckedInfo.ToRev;
                SaveVerCtlInfo();
            }
            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, string.Format("resources updated to revision [{0}]", mCurrentRevision));
            return;
        }

        var copy = new Dictionary<string, VerCtlUpdateInfo>(mCheckedInfo.Updates);
        bool isHasBadUpdate = false;
        foreach(var kv in copy)
        {
            var updateInfo = kv.Value;
            if(UpdateResourceItem(updateInfo))
            {
                RemoveTempDownload(updateInfo);
                RemoveUpdateInfo(kv.Key);
            }
            else
            {
                isHasBadUpdate = true;
            }
        }

        if(!isHasBadUpdate)
        {
            mCurrentRevision = mCheckedInfo.ToRev;
            SaveVerCtlInfo();
            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, string.Format("resources updated to revision [{0}]", mCurrentRevision));

            // remove all backup
            DisposeAllBackup(ResourceBaseDir);
        }
        else
        {
            // rollback all backup
            ReformAllBackup(ResourceBaseDir);
            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, string.Format("resources updat to revision [{0}] failed, something bad have been found.", mCurrentRevision));
        }
    }

    /// <summary>
    /// 清除当前的版本，包括正在更新中的中间数据，但是不会清除资源目录和目录中的资源
    /// </summary>
    void DisposeVerCtl()
    {
        mCurrentRevision = -1;
        mTargetRevision = -1;
        mTheater = TheaterDefault;

        mCheckedInfo.DisposeUpdates();
    }

    /// <summary>
    /// 程序初始化 资源版本管理 的入口
    ///  设置初始资源版本、版本线及其他;
    /// 这些设置会立即生效
    /// </summary>
    public void ResetVersionControl(string theater, string resourceBaseFolder, int resourceVersion)
    {
        if(mCurrentRevision >= 0)
        {
            // 已经存在有版本，清除之
            DisposeVerCtl();
        }

        mTheater = theater;
        ResourceBaseDir = resourceBaseFolder;
        mCurrentRevision = resourceVersion;

        //保存至本地
        SaveVerCtlInfo();
    }

    /// <summary>
    /// 加载保存于本地的版本控制信息
    /// </summary>
    void LoadLocalVerCtlInfo()
    {
        var fileName = VersionControlFolder + "VerCtl.xml";
        if (!U3DFileIO.Exists(fileName))
            return;

        ResourceBaseDir = "Resource";
        mTheater = TheaterDefault;
        mCurrentRevision = -1;
        mSetedTargetRevision = -1;
        mTargetRevision = -1;

        try
        {
            var xml = XElement.Load(U3DFileIO.GetFileWirtableFullPath(fileName));
            {
                var xmlel = xml.Element("Theater");
                mTheater = xmlel == null ? mTheater : xmlel.Value;
                xmlel = xml.Element("Revision");
                mCurrentRevision = xmlel == null ? mCurrentRevision : int.Parse(xmlel.Value);
                xmlel = xml.Element("ResourceBase");
                ResourceBaseDir = xmlel == null? ResourceBaseDir : xmlel.Value;
            }

            xml = null;
        }
        catch (Exception ex)
        {
            mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Can't Open version control file!! error : " + ex.Message );
        }
    }

    /// <summary>
    /// 保存版本控制信息到本地
    /// </summary>
    void SaveVerCtlInfo()
    {
        FileStream VerCtlFile;
        var fileName = VersionControlFolder + "VerCtl.xml";
        var backupFileName = VersionControlFolder + "VerCtl.bak.xml";
        var tempFileName = VersionControlFolder + "VerCtl.temp.xml";

        //bool hasBackUp = false;

        if (U3DFileIO.Exists(fileName))
        {
            if (!U3DFileIO.MoveFile(fileName, backupFileName))
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Can't careate version control backup file!!");
                return;
            }

            //hasBackUp = true;
        }

        try
        {
            using (var xml = XmlWriter.Create(U3DFileIO.GetFileWirtableFullPath(tempFileName)))
            { 
                xml.WriteStartDocument();
                xml.WriteStartElement("VerCtl");

                xml.WriteElementString("Theater", mTheater);
                xml.WriteElementString("Revision", mCurrentRevision.ToString());

                xml.WriteElementString("ResourceBase", ResourceBaseDir);

                xml.WriteEndElement();
                xml.WriteEndDocument();

                xml.Flush();
                xml.Close();
            }
            
            if (!U3DFileIO.MoveFile(tempFileName, fileName))
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Can't careate version control backup file!!");
                return;
            }

        }
        catch
        {
            mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Can't careate version control file!!");
        }
    }

    /// <summary>
    /// 与 版本管理服务器 建立连接 并鉴权
    /// </summary>
    /// <returns></returns>
    IEnumerator ConnectAndAuthToVerCtlServer()
    {
        if(mNetClient.Connected)
            yield break;

        var isAsyncEnd = false;
        Exception connectException = null;
        var async = mNetClient.BeginConnect(mServerIP, mServerPort, ar =>
        {
            Thread.MemoryBarrier();
            try
            {
                mNetClient.EndConnect(ar);

            }
            catch (Exception ex)
            {
                connectException = ex;
            }

            isAsyncEnd = true;
            Thread.MemoryBarrier();
        }, this);

        var count = 0;
        Thread.MemoryBarrier();
        while (!async.IsCompleted && !isAsyncEnd)
        {
            if(count++ > 5)
            {
                count = 0;
                var now = Time.time;
                if(now - mLastProgressTime > 0.5f)
                {
                    mLastProgressTime = now;
                    mLastProgess += 0.1f;
                    if(mLastProgess > 1.0f)
                        mLastProgess = 1.0f;
                    mVerCtlContext.InformPreProgress("", mLastProgess);
                }
            }
            yield return null;
        }

        if (connectException != null)
        {
            mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Failed to connect to VerCtlServer : " + connectException.Message);
            yield break;
        }

        if (!mNetClient.Connected)
        {
            mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Failed to connect to VerCtlServer ");
            yield break;
        }

        var stream = mNetClient.GetStream();
        {
            try
            {
                NetPacketHelper.WriteAsPacketToStream(stream, "session]:Hello VerCtl");
                stream.Flush();
            }
            catch (Exception ex)
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Failed to autho to VerCtlServer : " + ex.Message);
                mNetClient.Close();
                yield break;
            }

            var packetList = new List<NetPacketHelper.PacketReadResult>();

            var now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            var packBuffer = new NetPacketHelper.PacketBuffer
            {
                headBuff = new byte[128],
                bodyBuff = new byte[128]
            };

            var netWait = new NetWaiting();

            yield return mVerCtlContext.StartCoroutine(NetPacketHelper.ReceiveOnePacket(mNetClient, mNetProtocol, stream, packBuffer, netWait, packetList));

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }


            if(packetList.Count <= 0)
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, string.Format("Failed to receive data from VerCtlServer") );
                mNetClient.Close();
                yield break;
            }

            var packetResult = packetList[0];
            if(packetResult.packet == null)
            {
                var ex = packetResult.exception;
                var error = packetResult.errorMsg;

                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, string.Format("Failed to autho from VerCtlServer : {0} {1}", error, (ex == null ? "" : ex.Message)));
                mNetClient.Close();
                yield break;
            }
            
            try
            {
                var hanshake = packetResult.packet as PacketHandler_HandShake;
                if (hanshake == null)
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Failed to autho to VerCtlServer : Not expected data");
                }

                hanshake.Process(new PacketContext(mVerCtlContext));
                mIsAuthorized = hanshake.IsAuthoOK;
            }
            catch (Exception ex)
            {
                mNetClient.Close();
                mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Failed to autho to VerCtlServer : " + ex.Message);
            }

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }
        }
    }

    /// <summary>
    /// 向版本管理服务器查询更新
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckUpdates_(CoroutineResult<Dictionary<string, VerCtlUpdateInfo>> result)
    {
        if (mCurrentRevision > 0 && mCurrentRevision == mTargetRevision)
        {
            result.IsDone = true;
            result.HasError = true;
            result.ErrorCode    = -1;
            result.ErrorMessage = "Invalid revision data provided.";
            yield break;
        }

        if(!mNetClient.Connected)
        {
            result.IsDone = true;
            result.HasError = true;
            result.ErrorCode    = 0;
            result.ErrorMessage = "Invalid net client provided.";
            yield break;
        }

        var stream = mNetClient.GetStream();
        {
            var queryString = string.Format("update]:?Theater={0}&Ver={1}&TargetVer={2}", Theater, mCurrentRevision, mTargetRevision);
            var bodyBuff = Encoding.UTF8.GetBytes(queryString);
            try
            {
                var head = BitConverter.GetBytes((Int32)bodyBuff.Length);
                stream.Write(head, 0, head.Length);
                stream.Write(bodyBuff, 0, bodyBuff.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                result.IsDone = true;
                result.HasError = true;
                result.ErrorCode    = 0;
                result.ErrorMessage = "Failed to cehck update from VerCtlServer : " + ex.Message;
                yield break;
            }

            var now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            var packetList = new List<NetPacketHelper.PacketReadResult>();

            var packBuffer = new NetPacketHelper.PacketBuffer
            {
                headBuff = new byte[128],
                bodyBuff = new byte[1024]
            };
            var netWait = new NetWaiting();

            yield return mVerCtlContext.StartCoroutine(NetPacketHelper.ReceiveOnePacket(mNetClient, mNetProtocol, stream, packBuffer, netWait, packetList));

            if(packetList.Count <= 0)
            {
                result.IsDone = true;
                result.HasError = true;
                result.ErrorCode    = 0;
                result.ErrorMessage = "Failed to receive update from VerCtlServer";
                yield break;
            }

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            var packetResult = packetList[0];
            if(packetResult.packet == null)
            {
                var ex = packetResult.exception;
                var error = packetResult.errorMsg;

                result.IsDone = true;
                result.HasError = true;
                result.ErrorCode    = 0;
                result.ErrorMessage = string.Format("Failed to receive update from VerCtlServer : {0} {1}", error, (ex == null ? "" : ex.Message));
                yield break;
            }

            var update = packetResult.packet as PacketHandler_Update;

            try
            {
                update.Process(new PacketContext(mVerCtlContext));
            }
            catch(Exception ex)
            { 
                result.IsDone = true;
                result.HasError     = true;
                result.ErrorCode    = -1;
                result.ErrorMessage = "Failed to cehck update from VerCtlServer : " + ex.Message;
                yield break;
            }

            mTargetRevision = update.TargetRevision;
            mHeadRevision = update.HeadRevision;

            foreach (var kv in update.AllUpdateMetaInfo)
                result.Reuslt.Add(kv.Key, kv.Value);

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }
        }

        result.IsDone = true;
    }

    CoroutineResult<Dictionary<string, VerCtlUpdateInfo>> CheckUpdates(Dictionary<string, VerCtlUpdateInfo> preallocated)
    {
        var result = new CoroutineResult<Dictionary<string, VerCtlUpdateInfo>>();

        result.Coroutine = CheckUpdates_(result);
        result.Reuslt = preallocated;

        return result;
    }

    /// <summary>
    /// 检查本地资源与服务器的资源版本，必要的话更新本地资源
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartUpdate()
    {
        LoadUpdateInfo();

        var allUpdates = new Dictionary<string, VerCtlUpdateInfo>();

        var tryCount = 0;

    CONNECT_SERVER:
        mNetClient = new TcpClient();
        mVerCtlContext.SetLoadingTitle("正在连接服务器...");

        mLastProgressTime = Time.time;

        mLastProgess = 0.05f;
        mVerCtlContext.InformPreProgress("", mLastProgess);
        yield return mVerCtlContext.StartCoroutine(ConnectAndAuthToVerCtlServer());

        if(!mIsAuthorized || !mNetClient.Connected)
        {
            mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "VerCtl server connected failed!!");
            mNetClient.Close();
            mNetClient = null;
            if(++tryCount > MaxTryConnectCount)
                yield break;
            else
                goto CONNECT_SERVER;
        }

        // override revision info to test
        //mCurrentRevision = -1;
        //mTargetRevision = 0;

        do
        {
            mVerCtlContext.SetLoadingTitle("正在检查更新...");

            var now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "start to check updates ...");

            var croutineResult = CheckUpdates(allUpdates);
            yield return mVerCtlContext.StartCoroutine(croutineResult.Coroutine);

            if(croutineResult.HasError)
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_WARN, croutineResult.ErrorMessage);
                mNetClient.Close();
                mNetClient = null;

                if (croutineResult.ErrorCode != 0 || ++tryCount > MaxTryConnectCount)
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_ERROR, "Resource update failed!!!");
                    yield break;
                }
                else
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "Retry to check updates ...");
                    //mNetClient = new TcpClient();
                    yield return new WaitForSeconds(1);
                    goto CONNECT_SERVER;
                }
            }

            MergeAndSaveUpdateInfo(allUpdates);

            if (allUpdates.Count <= 0)
            {
                DistributeUpdatas();

                if (mCurrentRevision == mHeadRevision || mCurrentRevision == mSetedTargetRevision)
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_INFO, "resources are newest.");
                    break;
                }
                else
                {
                    goto RECHECK;
                }
            }

            allUpdates.Clear();

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            mFileDownLoader.ClearAllDownloadInfo();
            var haveToDownload = CheckForDownload(mCheckedInfo.Updates);

            now = Time.time;
            if (now - mLastProgressTime > 0.5f)
            {
                mLastProgressTime = now;
                mLastProgess += 0.1f;
                if (mLastProgess > 1.0f)
                    mLastProgess = 1.0f;
                mVerCtlContext.InformPreProgress("", mLastProgess);
            }

            if (!haveToDownload)
            {
                //发布更新到资源目录
                DistributeUpdatas();
                if (mCurrentRevision == mHeadRevision || mCurrentRevision == mSetedTargetRevision)
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_INFO, "resources are updated.");
                    break;
                }
                else
                {
                    goto RECHECK;
                }
            }

            mVerCtlContext.InformProgress("", 1.0f);

            mVerCtlContext.SetLoadingTitle(string.Format("正在下载资源(0/{0})...", mFileDownLoader.Total));
            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "start to download updates ...");

            //下载所需的资源
            mFileDownLoader.ServerIP = mServerIP;
            mFileDownLoader.ServerPort = mServerPort;
            mFileDownLoader.FilerServerEntryToken = mFileServerEntryToken;
            mFileDownLoader.NetClient = mNetClient;
            mFileDownLoader.NetProtocol = mNetProtocol;
            mFileDownLoader.StartDownLoad();

            var lastProgress = 0.0f;

            mVerCtlContext.InformProgress("", lastProgress);

            while (mFileDownLoader.IsDownLoading)
            {
                if(mFileDownLoader.CurrentProgress > lastProgress && (mFileDownLoader.CurrentProgress - lastProgress) * 10000.0f < 1.0f)
                {
                    yield return null;
                    continue;
                }

                string hint;
                var progress = mFileDownLoader.CurrentProgress;

                if(progress < 0.01)
                {
                    mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, ">>>>>>>>>>>>");
                }

#if UNITY_EDITOR
                hint = mFileDownLoader.CurrentDownloading == null ? "dowloading ... " : string.Format("dowloading {0} .. {1:P}", mFileDownLoader.CurrentDownloading.FileLRI, progress);
#else
                hint = "需要一些时间，请稍候...";
#endif

                mVerCtlContext.InformProgress(hint, progress);
                mVerCtlContext.SetLoadingTitle(string.Format("正在下载资源({0}/{1})...", mFileDownLoader.Completed + 1, mFileDownLoader.Total));

                lastProgress = progress;

                yield return null;
            }

            if (mFileDownLoader.IsAborted)
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "download aborted : " + mFileDownLoader.AbortReason);
                mNetClient.Close();
                mNetClient = null;

                if(++tryCount > MaxTryConnectCount)
                    yield break;

                yield return new WaitForSeconds(1);
                goto CONNECT_SERVER;
                //continue;
            }

            mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "download OK.");
            //发布所有更新到资源目录中
            DistributeUpdatas();

            if(mCurrentRevision == mHeadRevision || mCurrentRevision == mSetedTargetRevision)
            {
                mVerCtlContext.InformMessage(MessageInformType.MSG_LOG, "resources update OK.");
                break;
            }

        RECHECK:
            mTargetRevision = mSetedTargetRevision;

            /*
            var rand = new System.Random();
        Test:
            mTargetRevision ++;
        
            if(mTargetRevision > mHeadRevision)
                break;
*/
            mLastProgess = 0.02f;

        } while (true);

        mVerCtlContext.InformProgress("", 1.0f);
        yield return null;

        mNetClient.Close();
        mNetClient = null;
    }
}
