using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PGN.General;
using PGN.Data;
using PGN.Containers;

using ProtoBuf;
using ProtoBuf.Meta;

namespace PGN
{
    public static class SynchronizableTypes
    {
        internal static Dictionary<ushort, Type> types = new Dictionary<ushort, Type>(ushort.MaxValue);
        internal static Dictionary<Type, ushort> typesDictionary = new Dictionary<Type, ushort>(ushort.MaxValue);

        internal static Dictionary<ushort, Action<object, string>> typesActions = new Dictionary<ushort, Action<object, string>>(ushort.MaxValue);

        internal static Action<object, string>[] tst = new Action<object, string>[ushort.MaxValue];

        internal static bool allowTransitNonValidTypes = true;

        private static RuntimeTypeModel model;
        private static MetaType type;

        public static void EnableTransitNonValidTypes()
        {
            allowTransitNonValidTypes = true;
        }

        public static void DisableTransitNonValidTypes()
        {
            allowTransitNonValidTypes = false;
        }

        internal static void Init()
        {
            model = RuntimeTypeModel.Default;
            type = model.Add(typeof(ISync), true);

            var containerType =  type.AddSubType(6, typeof(Container));

            containerType.AddSubType(1, typeof(StringContainer));
            typesActions.Add(1, (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(string))) typesActions[typesDictionary[typeof(string)]].Invoke((data as StringContainer).value, senderID); });
            typesDictionary.Add(typeof(StringContainer), 1);
            types.Add(1, typeof(StringContainer));

            containerType.AddSubType(2, typeof(IntContainer));
            typesActions.Add(2, (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(int))) typesActions[typesDictionary[typeof(int)]].Invoke((data as IntContainer).value, senderID); });
            typesDictionary.Add(typeof(IntContainer), 2);
            types.Add(2, typeof(IntContainer));

            containerType.AddSubType(3, typeof(UintContainer));
            typesActions.Add(3, (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(uint))) typesActions[typesDictionary[typeof(uint)]].Invoke((data as UintContainer).value, senderID); });
            typesDictionary.Add(typeof(UintContainer), 3);
            types.Add(3, typeof(UintContainer));

            containerType.AddSubType(4, typeof(FloatContainer));
            typesActions.Add(4, (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(float))) typesActions[typesDictionary[typeof(float)]].Invoke((data as FloatContainer).value, senderID); });
            typesDictionary.Add(typeof(FloatContainer), 4);
            types.Add(4, typeof(FloatContainer));

            containerType.AddSubType(5, typeof(DoubleContainer));
            typesActions.Add(5, (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(double))) typesActions[typesDictionary[typeof(double)]].Invoke((data as DoubleContainer).value, senderID); });
            typesDictionary.Add(typeof(DoubleContainer), 5);
            types.Add(5, typeof(DoubleContainer));
        }

        internal static void InvokeClientAction(byte[] message, int bytesCount)
        {
            ushort type;
            NetData data = NetData.RecoverBytes(message, bytesCount, out type);
            
            if (data != null)
                typesActions[type].Invoke(data.data, data.senderID);
        }

        internal static void InvokeTypeActionTCP(ushort type, byte[] message, NetData data, User sender)
        {
            if (typesActions.ContainsKey(type))
            {
                if (data.broadcast)
                    sender.currentRoom.BroadcastMessageTCP(NetData.GetBytesData(data), sender);
                typesActions[type].Invoke(data.data, data.senderID);
            }
        }

        internal static void InvokeTypeActionUDP(ushort type, byte[] message, NetData data, User sender)
        {
            if (type < typesActions.Count)
            {
                if (data.broadcast)
                    sender.currentRoom.BroadcastMessageUDP(message, sender);
                typesActions[type].Invoke(data.data, data.senderID);
            }
        }

        public static void AddType(Type t, Action<object, string> action)
        {
            ushort number = GetInt16HashCode(t.FullName);

            if (t.IsDefined(typeof(SynchronizableAttribute), false))
            {
                model.Add(t, true);
                type.AddSubType(number, t);
            }

            typesDictionary.Add(t, number);
            types.Add(number, t);
            typesActions.Add(number, action);
        }


        public static void AddSyncSubType(Type t)
        {
            ushort number = GetInt16HashCode(t.FullName);
            typesDictionary.Add(t, number);
            types.Add(number, t);
            model.Add(t, true);
            type.AddSubType(number, t);
        }

        public static ushort GetTypeID(Type type)
        {
            return typesDictionary[type];
        }

        public static Type GetTypeWithID(ushort id)
        {
            return types[id];
        }

        public static bool TypeExists(ushort type)
        {
            return types.ContainsKey(type);
        }

        public static bool TypeExists(Type type)
        {
            return typesDictionary.ContainsKey(type);
        }

        internal static ushort GetInt16HashCode(string source)
        {
            ushort hashCode = 0;
            if (!string.IsNullOrEmpty(source))
            {
                byte[] byteContents = Encoding.Unicode.GetBytes(source);
                System.Security.Cryptography.SHA256 hash = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                ushort hashCodeStart = BitConverter.ToUInt16(hashText, 0);
                ushort hashCodeMedium = BitConverter.ToUInt16(hashText, 8);
                ushort hashCodeEnd = BitConverter.ToUInt16(hashText, 24);
                hashCode = (ushort)(hashCodeStart ^ hashCodeMedium ^ hashCodeEnd);
            }
            return (hashCode);
        }
    }
}
