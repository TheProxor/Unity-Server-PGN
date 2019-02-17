﻿using System;
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

        public static bool allowNonValidTypes = false;

        private static RuntimeTypeModel model;
        private static MetaType type;

        internal static void Init()
        {
            model = RuntimeTypeModel.Default;
            type = model.Add(typeof(ISync), true);

            AddType(typeof(StringContainer), (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(string))) typesActions[typesDictionary[typeof(string)]].Invoke((data as StringContainer).value, senderID); });
            AddType(typeof(IntContainer), (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(int))) typesActions[typesDictionary[typeof(int)]].Invoke((data as IntContainer).value, senderID); });
            AddType(typeof(UintContainer), (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(uint))) typesActions[typesDictionary[typeof(uint)]].Invoke((data as UintContainer).value, senderID); });
            AddType(typeof(FloatContainer), (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(float))) typesActions[typesDictionary[typeof(float)]].Invoke((data as FloatContainer).value, senderID); });
            AddType(typeof(DoubleContainer), (object data, string senderID) => { if (typesDictionary.ContainsKey(typeof(double))) typesActions[typesDictionary[typeof(double)]].Invoke((data as DoubleContainer).value, senderID); });
        }

        internal static void InvokeClientAction(byte[] message, ushort type, int bytesCount)
        {
            NetData data = NetData.RecoverBytes(message, bytesCount);
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
            else if (allowNonValidTypes && data.broadcast)
                sender.currentRoom.BroadcastMessageTCP(message, sender);
        }

        internal static void InvokeTypeActionUDP(ushort type, byte[] message, NetData data, User sender)
        {
            if (type < typesActions.Count)
            {
                if (data.broadcast)
                    sender.currentRoom.BroadcastMessageUDP(message, sender);
                typesActions[type].Invoke(data.data, data.senderID);
            }
            else if (allowNonValidTypes && data.broadcast)
                sender.currentRoom.BroadcastMessageUDP(message, sender);
        }

        public static void AddType(Type t, Action<object, string> action)
        {
            ushort number = GetInt16HashCode(t.FullName);

            if (t.IsDefined(typeof(SynchronizableAttribute), false))
            {
                model.Add(t, true);
                type.AddSubType(number + 1, t);
            }

            typesDictionary.Add(t, number);
            types.Add(number, t);
            typesActions.Add(number, action);
        }
        

        public static void AddSyncSubType(Type t)
        {
            model.Add(t, true);
            ushort k = GetInt16HashCode(t.FullName);
            type.AddSubType(k, t);
        }

        public static ushort GetTypeID(Type type)
        {
            return typesDictionary[type];
        }

        public static Type GetTypeWithID(byte id)
        {
            return types[id];
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
