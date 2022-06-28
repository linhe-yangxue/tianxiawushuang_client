using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;


    public class DynamicBuffer
    {

        private static int _defaultMinSize = 1024;
        private static int _defualtMaxSzie = 8096;


        public byte[] Buffer { get; set; }
        public int WritePosition { get; set; }
        public int ReadPosition { get; set; }
        /// <summary>
        /// short int long 这三种数据类型是否需要网络字节序的来回转换
        /// </summary>
        public bool ConvertNetOrder { get; set; }

        /// <summary>
        /// 字符串的编码类型、默认为unicode
        /// </summary>
        public Encoding CharSet { get; set; }

        protected int m_markReadPosition;
        protected int m_bufferLimit;

        internal DynamicBuffer()
        {
            init(_defaultMinSize, _defualtMaxSzie);
        }


        internal DynamicBuffer(int initSize, int maxSize)
        {
            init(initSize, maxSize);
        }

        private void init(int initSize, int maxSize)
        {
            this.Buffer = new byte[initSize];
            this.m_bufferLimit = maxSize;

            this.WritePosition = 0;
            this.ReadPosition = 0;
            this.m_markReadPosition = -1;

            CharSet = Encoding.Unicode;
            ConvertNetOrder = true;
        }

        /// <summary>
        /// 获得剩余的可以写的buffer的偏移和长度
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void GetWriteRemainBuffer(out int offset, out int length)
        {
            offset = this.WritePosition;
            length = this.Buffer.Length - this.WritePosition;
        }

        internal bool HasReadRemainBuffer(int length)
        {
            return ( ( ReadPosition+length> WritePosition) ? false : true );
        }

        /// <summary>
        /// 获得还有buffer中还有多少没有被读取的数据长度
        /// </summary>
        /// <returns></returns>
        internal int GetReadRemainBufferLength()
        {
            return this.WritePosition - this.ReadPosition;
        }
        /// <summary>
        /// 获得剩余的可以写的buffer的长度
        /// </summary>
        /// <returns></returns>
        internal int GetWriteRemainBufferLength()
        {
            return this.Buffer.Length - this.WritePosition;
        }

        /// <summary>
        /// 增加写游标的长度
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        internal bool AddWritePosition(int length)
        {
            this.WritePosition += length;
            if (this.GetWriteRemainBufferLength() < this.Buffer.Length / 4)
            {
                this.ExpandCapacity(0);
            }

            return true;
        }

        /// <summary>
        /// 增加读游标的长度
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        internal bool AddReadPosition(int length)
        {
            this.ReadPosition += length;
            
            return true;
        }
        


        /// <summary>
        /// 标记读游标的位置，与RestReadPostion对应
        /// </summary>
        internal void MarkReadPostion()
        {
            this.m_markReadPosition = this.ReadPosition;
        }
        /// <summary>
        /// 清楚标记的位置
        /// </summary>
        internal void CleanMarkReadPostion()
        {
            this.m_markReadPosition = -1;
        }
        /// <summary>
        /// 回复读游标的位置为之前MarkReadPostion标记的位置
        /// </summary>
        internal void RestReadPostion()
        {
            if (this.m_markReadPosition == -1)
                return;

            this.ReadPosition = this.m_markReadPosition;
        }


        ////////////////////////////////// Read Start //////////////////////////////////////////////////////////

        internal void Read(int arrayLength, out byte[] outArray, out int offset)
        {
            if (this.ReadPosition + arrayLength > this.WritePosition)
            {
                outArray = null;
                offset = -1;
                throw new IndexOutOfRangeException();
            }
            outArray = this.Buffer;
            offset = this.ReadPosition;
            this.ReadPosition += arrayLength;
        }

        internal void Read(int arrayLength, out byte[] outArray)
        {
            if (this.ReadPosition + arrayLength > this.WritePosition)
            {
                outArray = null;
                throw new IndexOutOfRangeException();
            }
            outArray = new byte[arrayLength];
            Array.Copy(this.Buffer, this.ReadPosition, outArray, 0, outArray.Length);
            this.ReadPosition += arrayLength;
        }

        internal void Peek(out bool value)
        {
            if (ReadPosition + sizeof(bool) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }
            value = BitConverter.ToBoolean(this.Buffer, ReadPosition);
        }

        internal void Read(out bool value)
        {
            Peek(out value);
            ReadPosition += sizeof(bool);
        }

        internal void Peek(out sbyte value)
        {
            if (ReadPosition + sizeof(sbyte) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }
            value = (sbyte)this.Buffer[ReadPosition];
        }

        internal void Read(out sbyte value)
        {
           Peek(out value);
           ReadPosition += sizeof(sbyte);
        }

        internal void Peek(out byte value)
        {
            if (ReadPosition + sizeof(byte) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }
            value = this.Buffer[ReadPosition];
        }

        internal void Read(out byte value)
        {
           Peek(out value);
           ReadPosition += sizeof(byte);
        }

        internal void Peek(out short value)
        {
            if (ReadPosition + sizeof(short) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }

            value = BitConverter.ToInt16(this.Buffer, ReadPosition);

            if (ConvertNetOrder)
            {
                value = System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        internal void Read(out short value)
        {
            Peek(out value);
            ReadPosition += sizeof(short);
        }

        internal void Peek(out ushort value)
        {
            short tmp;
            Peek(out tmp);
            value = (ushort)tmp;
        }

        internal void Read(out ushort value)
        {
            Peek(out value);
            ReadPosition += sizeof(ushort);
        }


        internal void Peek(out int value)
        {
            if (ReadPosition + sizeof(int) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }

            value = BitConverter.ToInt32(this.Buffer, ReadPosition);

            if (ConvertNetOrder)
            {
                value = System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        internal void Read(out int value)
        {
            Peek(out value);
            ReadPosition += sizeof(int);
        }

        internal void Peek(out uint value)
        {
            int tmp;
            Peek(out tmp);
            value = (uint)tmp;
        }

        internal void Read(out uint value)
        {
            Peek(out value);
            ReadPosition += sizeof(uint);
        }


        internal void Peek(out long value)
        {
            if (ReadPosition + sizeof(long) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }

            value = BitConverter.ToInt64(this.Buffer, ReadPosition);

            if (ConvertNetOrder)
            {
                value = System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        internal void Read(out long value)
        {
            Peek(out value);
            ReadPosition += sizeof(long);
        }

        internal void Peek(out ulong value)
        {
            long tmp;
            Peek(out tmp);
            value = (ulong)tmp;
        }

        internal void Read(out ulong value)
        {
            Peek(out value);
            ReadPosition += sizeof(ulong);
        }

        internal void Peek(out float value)
        {
            if (ReadPosition + sizeof(float) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }

            value = BitConverter.ToSingle(this.Buffer, ReadPosition);
        }

        internal void Read(out float value)
        {
            Peek(out value);
            ReadPosition += sizeof(float);
        }

        internal void Peek(out double value)
        {
            if (ReadPosition + sizeof(double) > WritePosition)
            {
                throw new IndexOutOfRangeException();
            }

            value = BitConverter.ToDouble(this.Buffer, ReadPosition);
        }

        internal void Read(out double value)
        {
            Peek(out value);
            ReadPosition += sizeof(double);
        }

        internal void Read(out string value)
        {
            value = string.Empty;
            int num;
            this.Read(out num);

            if (num == 0)
            {
                value = "";
            }
            num = 2 * num;

            int offset;
            byte[] data;

            this.Read(num, out data, out offset);
            value = CharSet.GetString(data, offset, num);
        }

        ///////////////////////////////// Read End //////////////////////////////////////////////////////////

        ///////////////////////////////// Write Start //////////////////////////////////////////////////////////////

        internal void Write(bool value)
        {
            Write((byte)(value ? 1 : 0));
        }

        internal void Write(byte value)
        {
            Write(new byte[] { value });
        }

        internal void Write(sbyte value)
        {
            Write((byte)value);
        }

        internal void Write(short value)
        {
            if (ConvertNetOrder)
                value = System.Net.IPAddress.HostToNetworkOrder(value);
            Write(BitConverter.GetBytes(value));
        }

        internal void Write(ushort value)
        {
            Write((short)value);
        }


        internal void Write(int value)
        {
            if (ConvertNetOrder)
                value = System.Net.IPAddress.HostToNetworkOrder(value);
            Write(BitConverter.GetBytes(value));
        }

        internal void Write(uint value)
        {
            Write((int)value);
        }

        internal void Write(long value)
        {
            if (ConvertNetOrder)
                value = System.Net.IPAddress.HostToNetworkOrder(value);
            Write(BitConverter.GetBytes(value));
        }

        internal void Write(ulong value)
        {
            Write((long)value);
        }


        internal void Write(float value)
        {
            Write(BitConverter.GetBytes(value));
        }

        internal void Write(double value)
        {
            Write(BitConverter.GetBytes(value));
        }

        internal void Write(string value)
        {
            //if (value == default(string))
            //    return;

            this.Write(value.Length);
            this.Write(CharSet.GetBytes(value));
        }


        /// <summary>
        /// write a byte array to the buffer
        /// </summary>
        /// <param name="value">the byte array</param>
        internal void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        internal bool Write(byte[] buffer, int offset, int length)
        {
            //enough
            if (length > this.GetWriteRemainBufferLength())
            {
                // First try clean the buffer that already be readed.
                this.Clean();
                if (length > this.GetWriteRemainBufferLength())
                {
                    // expand capacity
                    this.ExpandCapacity(length);
                }
            }
            Array.Copy(buffer, offset, this.Buffer, this.WritePosition, length);
            this.WritePosition += length;
            return true;
        }

        internal void Write(DynamicBuffer value)
        {
            this.Write(value.Buffer, 0, value.WritePosition);
        }


        /////////////////////////////////////////// Write End //////////////////////////////////////////////

        /// <summary>
        /// 扩充buffer长度
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        private bool ExpandCapacity(int needSize)
        {
            // Buffer is too long . expand capacity is not need
            if (this.Buffer.Length == this.m_bufferLimit
                || needSize > this.m_bufferLimit - this.GetWriteRemainBufferLength())
            {
                return false;
            }
            // expand capacity and copy
            int newLength = (this.Buffer.Length * 2 < this.m_bufferLimit) ? this.Buffer.Length * 2 : this.m_bufferLimit;
            newLength = (needSize > (newLength - this.WritePosition)) ? this.m_bufferLimit : newLength;

            byte[] tmpBuffer = new byte[newLength];
            Array.Copy(this.Buffer, tmpBuffer, WritePosition);
            this.Buffer = tmpBuffer;

            return true;
        }

        /// <summary>
        /// 清理已经被读走的数据，并且平移未被读走的数据到buffer的头部
        /// </summary>
        internal void Clean()
        {

            int reamaining = WritePosition - ReadPosition;
            if (reamaining == 0)
            {
                this.CleanALL();
            }
            else
            {
                Array.Copy(this.Buffer, this.ReadPosition, this.Buffer, 0, reamaining);
                this.ReadPosition = 0;
                this.WritePosition = reamaining;
            }
        }

        /// <summary>
        /// clean the buffer and all the postion
        /// </summary>
        internal void CleanALL()
        {
            this.m_markReadPosition = -1;
            this.ReadPosition = 0;
            this.WritePosition = 0;
        }


    }

