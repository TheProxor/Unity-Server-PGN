using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;
using PGN;

namespace GameCore.Commands
{
    [Synchronizable, ProtoContract]
    public class GameCommand : IServerCall
    {
        [Synchronizable, ProtoContract]
        public class Connect : GameCommand
        {

        }

        [Synchronizable, ProtoContract]
        public class Disconnect : GameCommand
        {

        }

        [Synchronizable, ProtoContract]
        public class ErrorMessage
        {
        }

        [Synchronizable, ProtoContract]
        public class LevelUp
        {
            [ProtoMember(1)]
            public uint level;
        }
    }


    [Synchronizable, ProtoContract]
    public class UserValidateCommand : IServerCall
    {
        [Synchronizable, ProtoContract]
        public class BuyCommand : UserValidateCommand
        {
            [Synchronizable, ProtoContract]
            public class BuyColor : BuyCommand
            {
                [ProtoMember(1)]
                public string color;

                public BuyColor(string color)
                {
                    this.color = color;
                }
            }

            [Synchronizable, ProtoContract]
            public class BuyError : BuyCommand
            {
                [ProtoMember(1)]
                public uint current;
                [ProtoMember(2)]
                public uint cost;

                public BuyError(uint current, uint cost)
                {
                    this.current = current;
                    this.cost = cost;
                }
            }
        }
    
        [Synchronizable, ProtoContract]
        public class FindUser : UserValidateCommand
        {
            public FindUser(string userName)
            {
                this.userName = userName;
            }

            [ProtoMember(1)]
            public string userName;
        }

        [Synchronizable, ProtoContract]
        public class GetUserData : UserValidateCommand
        {
            public GetUserData(string id)
            {
                this.id = id;
            }

            [ProtoMember(1)]
            public string id;
        }

        [Synchronizable, ProtoContract]
        public class ListOnlineUsers : UserValidateCommand
        {
            public ListOnlineUsers()
            {

            }
        }
    }
}
