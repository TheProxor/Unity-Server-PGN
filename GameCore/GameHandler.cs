using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;
using PGN;

namespace GameCore
{
    public class GameHandler
    {
        [Synchronizable, ProtoContract]
        public class PlayerCondition : ISync
        {
            public PlayerCondition()
            {

            }

            public PlayerCondition(float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
            {
                this.posX = posX;
                this.posY = posY;
                this.posZ = posZ;
                this.rotX = rotX;
                this.rotY = rotY;
                this.rotZ = rotZ;
            }

            [ProtoMember(1)]
            public float posX;
            [ProtoMember(2)]
            public float posY;
            [ProtoMember(3)]
            public float posZ;

            [ProtoMember(4)]
            public float rotX;
            [ProtoMember(5)]
            public float rotY;
            [ProtoMember(6)]
            public float rotZ;
        }
    }
}
