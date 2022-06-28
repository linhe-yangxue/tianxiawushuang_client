// Main.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.
//

using ICSharpCode.SharpZipLib.Zip.Compression;
using System;

namespace MyTest
{
    public class ZipTool
    {
        static int DEFAULT_BUFFER_SIZE = 1024 * 128;
        static int MAX_BUFFER_SIZE = 1024 * 1024; // MAX (1M)
        static Inflater mUnCompressTool = new Inflater();
        static Deflater mCompressTool = new Deflater();
        static byte[] zipBuffer = new byte[DEFAULT_BUFFER_SIZE];
        static byte[] resultBuffer = new byte[DEFAULT_BUFFER_SIZE];

        static public bool ReadyZipBuffer(out byte[] getBuffer, int needSize)
        {
            if (needSize > MAX_BUFFER_SIZE)
            {
                getBuffer = null;
                return false;
            }

            if (needSize > zipBuffer.Length)
            {
                do 
                {
                    zipBuffer = new byte[zipBuffer.Length + DEFAULT_BUFFER_SIZE];
                } while (needSize>zipBuffer.Length);
            }
            getBuffer = zipBuffer;
            return true;
        }
		
		static public int ZipData(byte[] scrData, int dataPos, int dataSize, out byte[] resultData)
		{
			mCompressTool.Reset();
            mCompressTool.SetInput(scrData, dataPos, dataSize);
            mCompressTool.Finish();
            _ResetResultBuffer(dataSize);
            int re = mCompressTool.Deflate(resultBuffer);
            resultData = resultBuffer;
            return re;
		}

        static public bool RestoreZipData(byte[] zipData, int zipDataPos, int zipSize, ref byte[] resultData, int resultDataSize)
		{
			mUnCompressTool.Reset();
            mUnCompressTool.SetInput(zipData, zipDataPos, zipSize);            
            _ResetResultBuffer(resultDataSize);
            int re = mUnCompressTool.Inflate(resultData, 0, resultDataSize);
            //resultData = resultBuffer;
            return re==resultDataSize;
		}
		
		

        static public bool PeekResultBuffer(out byte[] getBuffer)
        {
            getBuffer = resultBuffer;
            return true;
        }

        static bool _ResetResultBuffer(int needSize)
        {
            if (needSize > MAX_BUFFER_SIZE)
                return false;

            if (needSize > resultBuffer.Length)
            {
                do
                {
                    resultBuffer = new byte[resultBuffer.Length + DEFAULT_BUFFER_SIZE];
                } while (needSize > resultBuffer.Length);
            }
            
            return true;
        }

        static public int Compress(int scrDataSize)
        {            
            mCompressTool.Reset();
            mCompressTool.SetLevel(Deflater.DEFAULT_COMPRESSION);

            mCompressTool.SetInput(zipBuffer, 0, scrDataSize);
            mCompressTool.Finish();
            if (_ResetResultBuffer(scrDataSize))
                return mCompressTool.Deflate(resultBuffer);

            return 0;
        }

        static public int UnCompress(int scrZipDataSize, int resultDataSize)
        {           
            mUnCompressTool.Reset();
            mUnCompressTool.SetInput(zipBuffer, 0, scrZipDataSize);

            if (_ResetResultBuffer(resultDataSize))
                return mUnCompressTool.Inflate(resultBuffer, 0, resultDataSize);

            return 0;
        }
    }
}