using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace GameBox
{

    public class GConverter
    {

        /// <summary>
        /// Int数字转 单byte
        /// 数值必须小于等于255
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte IntToByte(int value)
        {
            if (value > 255)
            {
                Debug.LogError($"Value  is  more than 255, need to convert to byte[]");
                return 0;
            }

            return (byte)(value & 0xff);
        }

        public static int ByteToInt(byte value)
        {
            return (int)(value & 0xff);
        }


        /// <summary>
        /// GZIP 压缩字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] GZipCompress(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
                zip.Write(data, 0, data.Length);
                zip.Close();
                byte[] buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Close();
                return buffer;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// GZIP 解压字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] GZipDecompress(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, true);
                MemoryStream msreader = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                while (true)
                {
                    int reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }

                    msreader.Write(buffer, 0, reader);
                }

                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}