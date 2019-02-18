using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;
using PGN.General;
using PGN.Containers;

using ProtoBuf;

namespace PGN.Data
{
    [ProtoContract]
    public class NetData
    {
        public NetData()
        {
            senderID = string.Empty;
            data = null;
        }

        public NetData(object data, string senderID, bool broadcast)
        {
            if (data is ISync)
                this.data = data as ISync;
            else if (data is string)
                this.data = new StringContainer(data as string);
            else if (data is int)
                this.data = new IntContainer((int)data);
            else if (data is uint)
                this.data = new UintContainer((uint)data);
            else if (data is float)
                this.data = new FloatContainer((float)data);
            else if (data is double)
                this.data = new DoubleContainer((double)data);
            else
            {
                Handler.OnLogReceived("Your data is not supported!");
                return;
            }

            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(string data, string senderID, bool broadcast)
        {
            this.data = new StringContainer(data);
            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(int data, string senderID, bool broadcast)
        {
            this.data = new IntContainer(data);
            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(uint data, string senderID, bool broadcast)
        {
            this.data = new UintContainer(data);
            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(float data, string senderID, bool broadcast)
        {
            this.data = new FloatContainer(data);
            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(double data, string senderID, bool broadcast)
        {
            this.data = new DoubleContainer(data);
            this.senderID = senderID;
            this.broadcast = broadcast;
        }

        public NetData(ISync data, string senderID, bool broadcast)
        {
            this.data = data;
            this.senderID = senderID;
            this.broadcast = broadcast;
        }


        public NetData(object data, bool broadcast)
        {
            if (data is ISync)
                this.data = data as ISync;
            else if (data is string)
                this.data = new StringContainer(data as string);
            else if (data is int)
                this.data = new IntContainer((int)data);
            else if (data is uint)
                this.data = new UintContainer((uint)data);
            else if (data is float)
                this.data = new FloatContainer((float)data);
            else if (data is double)
                this.data = new DoubleContainer((double)data);
            else
            {
                Handler.OnLogReceived("Your data is not supported!");
                return;
            }

            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(string data, bool broadcast)
        {
            this.data = new StringContainer(data);
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(int data, bool broadcast)
        {
            this.data = new IntContainer(data);
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(uint data, bool broadcast)
        {
            this.data = new UintContainer(data);
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(float data, bool broadcast)
        {
            this.data = new FloatContainer(data);
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(double data, bool broadcast)
        {
            this.data = new DoubleContainer(data);
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        public NetData(ISync data, bool broadcast)
        {
            this.data = data;
            this.senderID = Handler.user.ID;
            this.broadcast = broadcast;
        }

        [ProtoMember(1)]
        public string senderID;
        [ProtoMember(2)]
        public ISync data;
        [ProtoMember(3)]
        internal bool broadcast;

        public byte[] bytes
        {
            get
            {
                return GetBytesData(this);
            }
        }

        public static byte[] GetBytesData(NetData message)
        {
            using (var memoryStream = new MemoryStream())
            {
                ushort _type = SynchronizableTypes.GetTypeID(message.data.GetType());
                if (SynchronizableTypes.TypeExists(_type))
                {
                    Serializer.NonGeneric.SerializeWithLengthPrefix(memoryStream, message, PrefixStyle.Base128, _type);
                    byte[] source = memoryStream.ToArray();
                    byte[] dest = new byte[source.Length + 2];
                    byte[] type = BitConverter.GetBytes(_type);
                    dest[0] = type[0];
                    dest[1] = type[1];
                    for (int i = 2; i < dest.Length; i++)
                        dest[i] = source[i - 2];
                    return dest;
                }
                else
                    return null;
            }
        }

        public static NetData RecoverBytes(byte[] bytesData, int bytesCount, out ushort type)
        {
            ushort _type = type = BitConverter.ToUInt16(bytesData, 0);
            if (bytesCount > 2 && SynchronizableTypes.TypeExists(type))
            {
                byte[] bytes = new byte[bytesCount - 2];
                for (int i = 0; i < bytesCount - 2; i++)
                    bytes[i] = bytesData[i + 2];

                object message = null;
                try
                {
                    using (var memoryStream = new MemoryStream(bytes))
                        Serializer.NonGeneric.TryDeserializeWithLengthPrefix(memoryStream, PrefixStyle.Base128, filed => typeof(NetData), out message);
                    return message as NetData;
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }

        public static byte[] Compress(byte[] data, CompressionLevel compressionLevel)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, compressionLevel))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
