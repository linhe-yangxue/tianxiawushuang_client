using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VersionControl
{
    class Packet_Error : DefaultPacketHandler<Packet_Error>
    {
        public string ErrorMsg  {get; internal set;}
        public int    ErrorCode {get; internal set;}

        public override string ExpectedName
        {
            get { return "error"; }
        }

        public override bool ParseData(Buffer dataBuff)
        {
            var str = NetPacketHelper.InterpretAsString(dataBuff);
            var index = str.IndexOf(">:");
            if(index > 0)
            {
                var error = str.Substring(0, index);
                var errorCode = -1;
                if(int.TryParse(error, out errorCode))
                    ErrorCode = errorCode;
                else
                    ErrorCode = -1;

                str = str.Substring(error.Length + ">:".Length);
            }

            ErrorMsg = str;
            return true;
        }

        public override void Process(IPacketContext ctx)
        {
            ctx.Log(LogLevel.LOGL_DEBUG, string.Format("Server response erorr: [{0}]:{1} ", ErrorCode, ErrorMsg));
            DisconnectServer();
        }
    }

    class PacketHandler_HandShake : DefaultPacketHandler<PacketHandler_HandShake>
    {
        IPacketContext mCtx;
        string mHandShakeResponse;

        public bool IsAuthoOK { get; private set; }
        public override string ExpectedName
        {
            get { return "handshake"; }
        }

        public override bool ParseData(Buffer dataBuff)
        {
            try
            {
                mHandShakeResponse = Encoding.UTF8.GetString(dataBuff.buff, dataBuff.offset, dataBuff.length);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override void Process(IPacketContext ctx)
        {
            mCtx = ctx;
            mCtx.Log(LogLevel.LOGL_DEBUG, "Server response : " + mHandShakeResponse);
            if (mHandShakeResponse.ToLower() != "ok")
            {
                IsAuthoOK = false;
                DisconnectServer();
            }
            else
            {
                IsAuthoOK = true;
                //IsDisconnectFromServer = true;
            }
        }
    }

    class PacketHandler_Update : DefaultPacketHandler<PacketHandler_Update>
    {
        int mTargetRevision;
        int mHeadRevision;

        Dictionary<string, VerCtlUpdateInfo> mAllUpdateMetaInfo;

        public int TargetRevision { get { return mTargetRevision; } }
        public int HeadRevision { get { return mHeadRevision; } }
        public Dictionary<string, VerCtlUpdateInfo> AllUpdateMetaInfo { get { return mAllUpdateMetaInfo; } }
        public override string ExpectedName
        {
            get { return "update"; }
        }

        public override bool ParseData(Buffer dataBuff)
        {
            if (mAllUpdateMetaInfo == null)
                mAllUpdateMetaInfo = new Dictionary<string, VerCtlUpdateInfo>();

            var updateInfo = Encoding.UTF8.GetString(dataBuff.buff, dataBuff.offset, dataBuff.length);

            var strReader = new StringReader(updateInfo);
            strReader.Read();   // Skip the BOM!!!

            var fuck = strReader.ReadToEnd();
            var xml = XElement.Parse(fuck);

            var xmlElement = xml.Element("Revision");
            if(xmlElement == null)
                return false;

            mTargetRevision = int.Parse(xmlElement.Value);

            xmlElement = xml.Element("HeadRev");
            if(xmlElement == null)
                return false;

            mHeadRevision = int.Parse(xmlElement.Value);

            var allUpdates = xml.Elements("Update");

            if (allUpdates == null)
                return true;        // there is nothing to update

            foreach (var upt in allUpdates)
            {
                var fileLRI = upt.Element("LRI").Value;
                var update = (UPDATE_TYPE)Enum.Parse(typeof(UPDATE_TYPE), upt.Element("UPT").Value);
                var fileMD5 = upt.Element("MD5").Value;

                mAllUpdateMetaInfo.Add(fileLRI, new VerCtlUpdateInfo
                {
                    FileLRI = fileLRI,
                    UpdateType = update,
                    NewRevMD5 = fileMD5,
                    ForRevision = mTargetRevision,
                    DownLoadedFilePath = null
                });
            }

            return true;
        }

        public override void Process(IPacketContext ctx)
        {}
    }

    class Packet_FileSize : DefaultPacketHandler<Packet_FileSize>
    {
        public long TotalSize {get; internal set;}
        public Dictionary<string, long> AllFileSize {get; internal set;} 

        public Packet_FileSize()
        {
            TotalSize = 0;
            AllFileSize = new Dictionary<string,long>();
        }

        public override string ExpectedName
        {
            get { return "size"; }
        }

        public override bool ParseData(Buffer dataBuff)
        {
            AllFileSize.Clear();
            if(dataBuff.length <= 0)
                return true;

            var response = NetPacketHelper.InterpretAsString(dataBuff);

            if(string.IsNullOrEmpty(response))
            {
                SetError(0, string.Format("size packet : Invalid packet data"));
                return false;
            }
            var index = response.IndexOf(">:");
            if (index <= 0)
            {
                SetError(1, string.Format("size packet : Invalid packet data"));
                return false;
            }
            var total = response.Substring(0, index);
            long temp = 0;
            if(!long.TryParse(total, out temp))
            {
                SetError(2, string.Format("size packet : Invalid packet data"));
                return false;
            }

            TotalSize = temp;

            var list = response.Substring(index + ">:".Length);
            if(string.IsNullOrEmpty(list))
                return true;

            char[] spliter = {';'};
            var pairInfo = list.Split(spliter);

            foreach(var pair in pairInfo)
            {
                if(string.IsNullOrEmpty(pair))
                    continue;

                index = pair.IndexOf(":");
                var LRI = pair.Substring(0, index);
                var size = long.Parse(pair.Substring(index + ":".Length));

                AllFileSize.Add(LRI, size);
            }

            return true;
        }

        public override void Process(IPacketContext ctx)
        {
            ;
        }
    }

    class Packet_FileDataBlock : DefaultPacketHandler<Packet_FileDataBlock>
    {
        Buffer mBlockData;
        public long Offset {get; private set;}
        public int  Length {get; private set;}

        public Buffer BlockData { get { return mBlockData; } }

        public Packet_FileDataBlock()
        {
            Offset = -1;
            Length = 0;
            mBlockData = new Buffer();
        }

        public override string ExpectedName
        {
            get { return "resblk"; }
        }

        public override bool ParseData(Buffer dataBuff)
        {
            if(!PackParamPairs.ContainsKey("off") || !PackParamPairs.ContainsKey("len"))
            {
                SetError(0, "error packet received for [resblk]");
                return false;
            }

            long offset = 0L;
            int len = 0;

            if(!long.TryParse(PackParamPairs["off"], out offset) || !int.TryParse(PackParamPairs["len"], out len))
            {
                SetError(1, "error packet received for [resblk]");
                return false;
            }

            Offset = offset;
            Length = len;

            if(Offset < 0 || Length <= 0 || Length != dataBuff.length)
            {
                SetError(2, "error packet received for [resblk]");
                return false;
            }

            mBlockData.buff = dataBuff.buff;
            mBlockData.offset = dataBuff.offset;
            mBlockData.length = dataBuff.length;

            return true;
        }
    }
}
