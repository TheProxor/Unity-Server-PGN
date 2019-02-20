using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PGN.DataBase
{
    [Serializable]
    public class UserInfo
    {
        public static uint attributeCount = 0;

        public string id { get; set; }

        public Dictionary<string, DataProperty> dataAttributes;

        public UserInfo(int capacity)
        {
            this.id = string.Empty;
            dataAttributes = new Dictionary<string, DataProperty>(capacity);
        }

        private static BinaryFormatter binaryFormatter = new BinaryFormatter();

        public byte[] bytes
        {
            get
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, this);
                    return Data.NetData.Compress(memoryStream.GetBuffer(), System.IO.Compression.CompressionLevel.Optimal);
                }
            }
        }

        public static byte[] GetUserInfoArrayBytes(UserInfo[] infos)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, infos);
                return memoryStream.GetBuffer();
            }
        }

        public static UserInfo RecoverBytes(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(Data.NetData.Decompress(bytes)))
                    return binaryFormatter.Deserialize(memoryStream) as UserInfo;
            }
            catch
            {
                return null;
            }
        }

        public static UserInfo[] RecoverArrayBytes(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                    return binaryFormatter.Deserialize(memoryStream) as UserInfo[];
            }
            catch
            {
                return null;
            }
        }
    }
}
