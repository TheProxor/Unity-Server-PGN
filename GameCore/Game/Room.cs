using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class Room
    {
        public string[] opponentsIDs;

        public bool matchEnded;

        public Room(uint count)
        {
            opponentsIDs = new string[count];
        }
    }
}
